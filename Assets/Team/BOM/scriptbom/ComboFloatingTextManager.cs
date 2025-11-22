using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ComboFloatingTextManager : MonoBehaviour
{
    public static ComboFloatingTextManager Instance { get; private set; }

    [Header("Setup")]
    [Tooltip("Parent ที่จะวางเลขคอมโบ (เช่น Panel ใน Canvas)")]
    public RectTransform container;

    [Tooltip("Prefab ของ TextMeshProUGUI สำหรับเลขคอมโบ 1 อัน")]
    public TextMeshProUGUI comboTextPrefab;

    [Header("Layout")]
    [Tooltip("ระยะห่างแนวตั้งระหว่างเลขแต่ละอัน")]
    public float verticalSpacing = 30f;

    [Tooltip("จำนวนเลขสูงสุดที่ให้แสดงพร้อมกันบนจอ")]
    public int maxEntries = 6;

    [Header("Color By Combo")]
    [Tooltip("สีตอนคอมโบน้อย (เช่น ขาว/เหลืองอ่อน)")]
    public Color lowComboColor = Color.white;

    [Tooltip("สีตอนคอมโบสูง (เช่น แดงจัด)")]
    public Color highComboColor = Color.red;

    [Tooltip("คอมโบเท่าไหร่ถึงจะใช้สี highComboColor เต็มที่")]
    public int comboForMaxColor = 50;

    [Header("Animation")]
    [Tooltip("ระยะที่เลขจะลอยขึ้น (พิกเซล) ระหว่างอนิเมชัน")]
    public float moveUpDistance = 40f;

    [Tooltip("เวลาในการลอย + เฟดหาย (วินาที)")]
    public float lifetime = 0.7f;

    [Header("Wiggle (การสั่นด้านข้าง)")]
    [Tooltip("ระยะสั่นซ้าย-ขวา (พิกเซล)")]
    public float wiggleAmplitude = 8f;

    [Tooltip("ความถี่ของการสั่น (รอบต่อ 1 life)")]
    public float wiggleFrequency = 3f;

    [Header("Scale Punch (การเด้งขยาย)")]
    [Tooltip("ขนาดเด้งเพิ่มจาก 1.0 (0.25 = ขยาย +25%)")]
    public float scalePunchAmount = 0.25f;

    private readonly List<RectTransform> _activeEntries = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (container == null)
            container = GetComponent<RectTransform>();
    }

    /// <summary>
    /// เรียกตอนคอมโบเปลี่ยน ให้เลขใหม่โผล่แล้วของเก่าเลื่อนขึ้น
    /// </summary>
    public void ShowCombo(int comboValue)
    {
        if (comboTextPrefab == null || container == null)
        {
            Debug.LogWarning("[ComboFloatingTextManager] Missing prefab or container.");
            return;
        }

        // สร้าง Text ตัวใหม่
        TextMeshProUGUI entry = Instantiate(comboTextPrefab, container);
        entry.text = comboValue.ToString();  // จะเปลี่ยนเป็น $"x{comboValue}" หรือ $"{comboValue} HIT!" ก็ได้

        RectTransform rect = entry.rectTransform;

        // -------------------------------
        // สีตามระดับคอมโบ (จากขาว -> แดง)
        // -------------------------------
        float colorT = 1f;
        if (comboForMaxColor > 0)
            colorT = Mathf.Clamp01((float)comboValue / comboForMaxColor);

        // ไล่สีจาก lowComboColor -> highComboColor
        Color baseColor = Color.Lerp(lowComboColor, highComboColor, colorT);
        baseColor.a = 1f;          // เริ่มด้วย alpha เต็ม
        entry.color = baseColor;

        // ตำแหน่งเริ่ม (ล่างสุด ของ container)
        rect.anchoredPosition = Vector2.zero;
        rect.localScale       = Vector3.one;

        // ขยับของเก่าทั้งหมดให้เลื่อนขึ้นทีละ verticalSpacing
        for (int i = 0; i < _activeEntries.Count; i++)
        {
            RectTransform r = _activeEntries[i];
            if (r == null) continue;
            r.anchoredPosition += new Vector2(0f, verticalSpacing);
        }

        _activeEntries.Add(rect);

        // ถ้าเกิน maxEntries ให้บังคับลบตัวบนสุด (เก่าที่สุด) ทิ้ง
        if (_activeEntries.Count > maxEntries)
        {
            RectTransform oldest = _activeEntries[0];
            _activeEntries.RemoveAt(0);
            if (oldest != null)
                Destroy(oldest.gameObject);
        }

        // เริ่มอนิเมชันลอยขึ้น + wiggle + scale punch + เฟด
        StartCoroutine(FadeMoveAndAnimate(entry, rect, baseColor));
    }

    private IEnumerator FadeMoveAndAnimate(TextMeshProUGUI text, RectTransform rect, Color baseColor)
    {
        float t = 0f;
        Vector2 startPos  = rect.anchoredPosition;
        Vector2 targetPos = startPos + new Vector2(0f, moveUpDistance);

        // เริ่มที่ scale 1
        rect.localScale = Vector3.one;

        while (t < lifetime)
        {
            t += Time.deltaTime;
            float alpha01 = Mathf.Clamp01(t / lifetime);

            // -------------------------
            // ลอยขึ้น + wiggle ซ้ายขวา
            // -------------------------
            Vector2 pos = Vector2.Lerp(startPos, targetPos, alpha01);

            if (wiggleAmplitude > 0f && wiggleFrequency > 0f)
            {
                float wiggle = Mathf.Sin(alpha01 * wiggleFrequency * Mathf.PI * 2f) * wiggleAmplitude;
                pos.x += wiggle;
            }

            rect.anchoredPosition = pos;

            // -------------------------
            // Scale Punch (เด้งตอนเริ่ม แล้วค่อยๆ ลด)
            // -------------------------
            if (scalePunchAmount != 0f)
            {
                // sin(0..π) → 0..1..0 ทำให้เด้งตอนกลางช่วง
                float punch = Mathf.Sin(alpha01 * Mathf.PI); 
                float scale = 1f + punch * scalePunchAmount;
                rect.localScale = Vector3.one * scale;
            }

            // -------------------------
            // เฟดจาก 1 → 0
            // -------------------------
            Color c = baseColor;
            c.a = Mathf.Lerp(1f, 0f, alpha01);
            text.color = c;

            yield return null;
        }

        _activeEntries.Remove(rect);
        Destroy(text.gameObject);
    }
}
