using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComboOverheatSystem : MonoBehaviour
{
    public static ComboOverheatSystem Instance { get; private set; }

    // ===============================
    //  COMBO
    // ===============================

    [Header("Combo Core")]
    [Tooltip("ระยะเวลาที่คอมโบจะไม่หลุดถ้าไม่มีการโจมตีเพิ่ม (วินาที)")]
    public float comboWindow = 2f;

    [Tooltip("จำนวนคอมโบปัจจุบัน")]
    public int combo;

    private float comboTimer;

    public enum ComboTier { D, C, B, A, S }

    [Header("Combo Tier Thresholds")]
    public int comboForTierC = 5;
    public int comboForTierB = 10;
    public int comboForTierA = 20;
    public int comboForTierS = 35;

    [SerializeField]
    private ComboTier currentTier = ComboTier.D;

    // ===============================
    //  OVERHEAT
    // ===============================

    [Header("Overheat")]
    [Tooltip("ค่าความร้อนปัจจุบัน (0 - overheatThreshold)")]
    public float heat;

    [Tooltip("ค่าความร้อนที่เพิ่มเมื่อโจมตีโดน")]
    public float heatPerHit = 0.02f;

    [Tooltip("ค่าความร้อนที่เพิ่มเมื่อคลิกพลาด")]
    public float heatPerMiss = 0.15f;

    [Tooltip("ค่าที่ความร้อนจะลดลงต่อวินาที")]
    public float heatDecayPerSecond = 0.25f;

    [Tooltip("ค่าความร้อนสูงสุดก่อน Overheat")]
    public float overheatThreshold = 1f;

    [Tooltip("เวลาที่ติด Overheat (วินาที)")]
    public float overheatDuration = 1.5f;

    [Tooltip("กำลังอยู่ในสถานะ Overheat หรือไม่")]
    public bool isOverheated;

    private float overheatTimer;

    // ===============================
    //  DAMAGE SCALING
    // ===============================

    [Header("Damage Scaling")]
    [Tooltip("ดาเมจเพิ่มขึ้นต่อ 1 คอมโบ (0.02 = +2% ต่อคอมโบ)")]
    public float damagePerComboStack = 0.02f;

    [Tooltip("โบนัสดาเมจพิเศษต่อ Tier")]
    public float tierDExtra = 0f;
    public float tierCExtra = 0.10f;
    public float tierBExtra = 0.20f;
    public float tierAExtra = 0.35f;
    public float tierSExtra = 0.50f;

    /// <summary>
    /// ตัวคูณดาเมจจากคอมโบ/โอเวอร์ฮีท
    /// </summary>
    public float ClickDamageMultiplier
    {
        get
        {
            float tierBonus = currentTier switch
            {
                ComboTier.C => tierCExtra,
                ComboTier.B => tierBExtra,
                ComboTier.A => tierAExtra,
                ComboTier.S => tierSExtra,
                _           => tierDExtra
            };

            return 1f + combo * damagePerComboStack + tierBonus;
        }
    }

    // ===============================
    //  UI
    // ===============================

    [Header("UI")]
    public Slider          overheatSlider;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI tierText;
    public Animator        comboUiAnimator;

    [Header("Animator Parameter Names")]
    public string comboChargeTriggerName     = "ComboCharge";
    public string comboTierUpTriggerName     = "ComboTierUp";
    public string overheatTriggerName        = "Overheat";
    public string overheatRecoverTriggerName = "OverheatRecover";

    // ===============================
    //  VFX & SFX
    // ===============================

    [Header("VFX")]
    public GameObject tierUpVfxPrefab;
    public GameObject overheatVfxPrefab;
    public Transform  vfxSpawnPoint;

    [Header("SFX")]
    public AudioSource audioSource;
    public AudioClip   tierUpClip;
    public AudioClip   overheatClip;
    public AudioClip   overheatRecoverClip;

    // ===============================
    //  LIFECYCLE
    // ===============================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (vfxSpawnPoint == null)
            vfxSpawnPoint = transform;

        Debug.Log($"[ComboOverheat] Awake on {gameObject.name}");
    }

    private void OnEnable()
    {
        // optional: ถ้าใช้ GameEvents อยู่แล้วก็จะทำงานอัตโนมัติ
        GameEvents.OnClickHit    += HandleClickHit;
        GameEvents.OnClickMiss   += HandleClickMiss;
        GameEvents.OnUltSlashHit += HandleUltHit;
    }

    private void OnDisable()
    {
        GameEvents.OnClickHit    -= HandleClickHit;
        GameEvents.OnClickMiss   -= HandleClickMiss;
        GameEvents.OnUltSlashHit -= HandleUltHit;
    }

    private void Start()
    {
        // set UI ครั้งแรก
        UpdateComboTier(forceLog: true);
        UpdateUI();
    }

    private void Update()
    {
        UpdateComboTimer();
        UpdateHeatAndOverheat();
        UpdateUI();
    }

    // ===============================
    //  PUBLIC API (ถ้าไม่อยากใช้ GameEvents)
    // ===============================

    public void RegisterClickHit(Monster monster, float damage)
    {
        InternalClickHit(monster, damage);
    }

    public void RegisterUltHit(Monster monster, float damage)
    {
        InternalUltHit(monster, damage);
    }

    public void RegisterClickMiss()
    {
        InternalClickMiss();
    }

    // ===============================
    //  EVENT BRIDGES
    // ===============================

    private void HandleClickHit(Monster monster, float damage) => InternalClickHit(monster, damage);
    private void HandleUltHit(Monster monster, float damage)  => InternalUltHit(monster, damage);
    private void HandleClickMiss()                            => InternalClickMiss();

    // ===============================
    //  CORE LOGIC
    // ===============================

    private void InternalClickHit(Monster monster, float damage)
    {
        if (isOverheated) return;

        combo++;
        comboTimer = comboWindow;

        heat += heatPerHit;
        heat = Mathf.Clamp(heat, 0f, overheatThreshold);

        if (comboUiAnimator != null && !string.IsNullOrEmpty(comboChargeTriggerName))
            comboUiAnimator.SetTrigger(comboChargeTriggerName);

        UpdateComboTier(forceLog: false);
    }

    private void InternalUltHit(Monster monster, float damage)
    {
        if (isOverheated) return;

        combo++;
        comboTimer = Mathf.Max(comboTimer, comboWindow * 0.5f);

        UpdateComboTier(forceLog: false);
    }

    private void InternalClickMiss()
    {
        if (isOverheated) return;

        heat += heatPerMiss;
        heat = Mathf.Clamp(heat, 0f, overheatThreshold);
    }

    private void UpdateComboTimer()
    {
        if (combo <= 0) return;

        comboTimer -= Time.deltaTime;
        if (comboTimer <= 0f)
        {
            combo = 0;
            UpdateComboTier(forceLog: true);
        }
    }

    private void UpdateComboTier(bool forceLog)
    {
        ComboTier newTier;

        if      (combo >= comboForTierS) newTier = ComboTier.S;
        else if (combo >= comboForTierA) newTier = ComboTier.A;
        else if (combo >= comboForTierB) newTier = ComboTier.B;
        else if (combo >= comboForTierC) newTier = ComboTier.C;
        else                             newTier = ComboTier.D;

        if (newTier != currentTier || forceLog)
        {
            currentTier = newTier;

            // 🔊 Debug log ทุกครั้งที่ tier เปลี่ยน หรือถูก forceLog
            Debug.Log($"[ComboOverheat] Tier = {currentTier}  (combo = {combo})");

            // เล่นเอฟเฟ็กต์เฉพาะตอนขึ้น tier จริง ๆ (ไม่ใช่ตอน reset ลงมา D)
            if (!forceLog && newTier != ComboTier.D)
            {
                PlayTierUpVfx();
                PlayTierUpSfx();
                TriggerTierUpUIAnim();
            }
        }

        if (comboText != null)
            comboText.text = combo.ToString();

        if (tierText != null)
            tierText.text = currentTier.ToString();
    }

    private void UpdateHeatAndOverheat()
    {
        if (!isOverheated)
        {
            heat -= heatDecayPerSecond * Time.deltaTime;
            if (heat < 0f) heat = 0f;

            if (heat >= overheatThreshold)
            {
                isOverheated  = true;
                overheatTimer = overheatDuration;
                OnEnterOverheat();
            }
        }
        else
        {
            overheatTimer -= Time.deltaTime;
            if (overheatTimer <= 0f)
            {
                isOverheated = false;
                heat         = overheatThreshold * 0.25f;
                OnExitOverheat();
            }
        }
    }

    private void OnEnterOverheat()
    {
        Debug.Log("[ComboOverheat] ENTER OVERHEAT");

        if (comboUiAnimator != null && !string.IsNullOrEmpty(overheatTriggerName))
            comboUiAnimator.SetTrigger(overheatTriggerName);

        if (overheatVfxPrefab != null)
            Instantiate(overheatVfxPrefab, vfxSpawnPoint.position, Quaternion.identity);

        if (audioSource != null && overheatClip != null)
            audioSource.PlayOneShot(overheatClip);

        GameEvents.RaiseBool("Overheated", true);
    }

    private void OnExitOverheat()
    {
        Debug.Log("[ComboOverheat] EXIT OVERHEAT");

        if (comboUiAnimator != null && !string.IsNullOrEmpty(overheatRecoverTriggerName))
            comboUiAnimator.SetTrigger(overheatRecoverTriggerName);

        if (audioSource != null && overheatRecoverClip != null)
            audioSource.PlayOneShot(overheatRecoverClip);

        GameEvents.RaiseBool("Overheated", false);
    }

    private void UpdateUI()
    {
        if (overheatSlider != null)
        {
            float norm = overheatThreshold <= 0f ? 0f : heat / overheatThreshold;
            overheatSlider.value = norm;
        }
    }

    private void PlayTierUpVfx()
    {
        if (tierUpVfxPrefab == null) return;
        Instantiate(tierUpVfxPrefab, vfxSpawnPoint.position, Quaternion.identity);
    }

    private void PlayTierUpSfx()
    {
        if (audioSource != null && tierUpClip != null)
            audioSource.PlayOneShot(tierUpClip);
    }

    private void TriggerTierUpUIAnim()
    {
        if (comboUiAnimator != null && !string.IsNullOrEmpty(comboTierUpTriggerName))
            comboUiAnimator.SetTrigger(comboTierUpTriggerName);
    }
}
