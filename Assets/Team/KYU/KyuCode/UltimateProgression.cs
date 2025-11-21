using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UltimateProgression : MonoBehaviour
{
    [Header("Progression")]
    public int currentClicks = 0;
    public int clicksToUlt = 100;

    [Header("UI References")]
    public Slider ultSlider;        // ลาก Slider มาใส่ตรงนี้
    public TextMeshProUGUI ultText; // ลาก Text (TMP) มาใส่ตรงนี้

    [Header("Links")]
    public UltimateSkill ultimateSkill;

    // ตัวเช็คว่าอัลติทำงานอยู่ไหม
    public bool IsUltimateActive
    {
        get
        {
            return ultimateSkill != null && ultimateSkill.isActiveAndEnabled;
        }
    }

    private void Start()
    {
        // ตั้งค่าเริ่มต้นให้หลอด (เช่น เริ่มมาเป็น 0/100)
        UpdateUI();
    }

    public void RegisterClick()
    {
        // ถ้าอัลติทำงานอยู่ ไม่ต้องนับเพิ่ม
        if (IsUltimateActive) return;

        // 1. เพิ่มจำนวนคลิก
        currentClicks++;
        
        // Debug เพื่อเช็คความชัวร์
        Debug.Log($"UltimateProgression: Click {currentClicks}/{clicksToUlt}");

        // 2. เช็คว่าเต็มหรือยัง
        if (currentClicks >= clicksToUlt)
        {
            Debug.Log("⚡ ULT READY & ACTIVATED! ⚡");
            
            // สั่งเปิดอัลติ
            ActivateUltimate();
            
            // 3. รีเซ็ตค่ากลับเป็น 0 ทันที
            currentClicks = 0;
        }

        // 4. อัปเดตหน้าจอ (หลอดจะขยับตาม currentClicks ที่เปลี่ยนไป)
        UpdateUI();
    }

    private void ActivateUltimate()
    {
        if (ultimateSkill != null)
        {
            ultimateSkill.StartUltimateDuration();
        }
        else
        {
            Debug.LogWarning("UltimateProgression: ลืมใส่ UltimateSkill ใน Inspector หรือเปล่าครับ?");
        }
    }

    // ฟังก์ชันจัดการ Slider และ Text
    private void UpdateUI()
    {
        // อัปเดตหลอด Slider
        if (ultSlider != null)
        {
            ultSlider.maxValue = clicksToUlt; // ตั้งค่าเต็มหลอด (100)
            ultSlider.value = currentClicks;  // ตั้งค่าปัจจุบัน (จะขยับตามคลิก)
        }

        // อัปเดตตัวเลข Text
        if (ultText != null)
        {
            ultText.text = $"{currentClicks} / {clicksToUlt}";
        }
    }
}