using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// เพื่อนร่วมสู้ (Companion) ที่ยิงมอนสเตอร์ให้อัตโนมัติ
/// ใช้ GameManager.activeMonster เป็นเป้าหมายหลัก
/// สร้าง VFX ให้บินไปหาเป้าหมายแล้วทำดาเมจ
/// มีระบบคอมโบซินเนอร์จี้ + อัลติคอลแลป + SFX
/// </summary>
public class Companion : MonoBehaviour
{
    [Header("Base Stats")]
    [Tooltip("เลเวลเริ่มต้นของเพื่อน (Companion)")]
    public int level = 1;

    [Tooltip("เลเวลสูงสุดของเพื่อน (Companion)")]
    public int maxLevel = 10;

    [Tooltip("ดาเมจพื้นฐานคิดเป็นกี่เท่าของดาเมจปัจจุบันของผู้เล่น (จาก UpgradeManager)")]
    [Range(0f, 2f)]
    public float baseDamageMultiplier = 0.3f;

    [Tooltip("เพิ่มดาเมจต่อเลเวล (+ จาก baseDamageMultiplier)")]
    [Range(0f, 1f)]
    public float damageMultiplierPerLevel = 0.05f;

    [Tooltip("จำนวนการโจมตีต่อวินาทีที่เลเวล 1")]
    public float baseAttacksPerSecond = 1f;

    [Tooltip("เพิ่มความเร็วโจมตี (% ต่อเลเวล) เช่น 0.1 = เพิ่ม 10% ต่อเลเวล")]
    [Range(0f, 1f)]
    public float attackSpeedPercentPerLevel = 0.1f;

    [Header("Upgrade Cost (optional)")]
    [Tooltip("ค่าใช้จ่ายพื้นฐานของการอัปเกรดเลเวลแรก")]
    public int baseUpgradeCost = 50;

    [Tooltip("ตัวคูณเพิ่มราคาอัปเกรดต่อเลเวล (เช่น 1.2 = แพงขึ้น 20% ทุกเลเวล)")]
    public float upgradeCostGrowth = 1.2f;

    [Header("VFX Settings (Normal Attack)")]
    [Tooltip("Prefab ของ VFX ที่ใช้ตอน Companion โจมตี (ต้องมี CompanionAttackVFX)")]
    public CompanionAttackVFX attackVfxPrefab;

    [Tooltip("ตำแหน่งที่ใช้ spawn VFX (ถ้าเว้นว่างจะใช้ตำแหน่งของ Companion)")]
    public Transform vfxSpawnPoint;

    [Tooltip("เวลาที่ VFX ใช้บินไปหาเป้าหมาย (วินาที)")]
    public float vfxTravelTime = 0.15f;

    [Header("Extra VFX (Combo / Ult)")]
    public CompanionAttackVFX comboHeavyVfxPrefab;
    public CompanionAttackVFX comboSuperVfxPrefab;
    public CompanionAttackVFX signatureVfxPrefab;
    public CompanionAttackVFX ultCoopVfxPrefab;

    [Header("Runtime State")]
    [SerializeField] private bool isActive = true;
    [SerializeField] private bool showDebugLog = true;

    [Header("References")]
    [Tooltip("ใช้สำหรับอ่านดาเมจปัจจุบันของผู้เล่น")]
    public UpgradeManager upgradeManager;

    [Tooltip("GameManager ที่ใช้จัดการ activeMonster")]
    public GameManager gameManager;

    [Tooltip("แอนิเมเตอร์ของ Companion (ไว้เล่นอนิเมชัน Idle / Attack / Combo)")]
    public Animator animator;

    [Tooltip("ชื่อ Trigger หรือ State ของอนิเมชันโจมตีปกติ")]
    public string attackTriggerName = "Attack";

    [Tooltip("ชื่อ State Idle ใน Animator (ถ้าอยากบังคับกลับไป Idle)")]
    public string idleStateName = "Idle";

    [Header("Animator States (Combo / Ult / Emotion)")]
    public string comboHeavyStateName = "ComboHeavy";
    public string comboSuperStateName = "ComboSuper";
    public string signatureStateName = "Signature";
    public string ultCoopStateName = "UltCoop";
    public string happyStateName = "Happy";
    public string berserkStateName = "Berserk";

    [Header("SFX")]
    public AudioSource audioSource;
    public AudioClip attackSfx;
    public AudioClip comboHeavySfx;
    public AudioClip comboSuperSfx;
    public AudioClip signatureSfx;
    public AudioClip ultCoopSfx;

    // ---- Limit HP ที่ Companion จะยอมยิง ----
    [Header("Attack Limit")]
    [Tooltip("หยุดโจมตีเมื่อ HP ของมอนสเตอร์ต่ำกว่าค่านี้ (เช่น 0.01 = 1%)")]
    [Range(0f, 1f)]
    public float minHpPercentToAttack = 0.01f;

    // ---- Ult Collaboration ----
    [Header("Ultimate Collaboration")]
    [Tooltip("เปิด / ปิด ระบบอัลติร่วมกับผู้เล่น")]
    public bool enableUltCollab = true;

    [Tooltip("Timeline สำหรับคัตซีนอัลติร่วม (ไม่จำเป็นต้องใส่ก็ได้)")]
    public PlayableDirector coopUltTimeline;

    [Tooltip("ตัวคูณดาเมจพิเศษตอนทำ Ult ร่วม (x เท่าจากดาเมจปกติของ Companion)")]
    public float ultBonusDamageMultiplier = 3f;

    [Tooltip("ระยะเวลาเดินทางของ VFX ตอน Ult ร่วม (ถ้าตั้ง ≤ 0 จะใช้ vfxTravelTime ปกติ)")]
    public float ultVfxTravelTime = 0.25f;

    // ---- Combo Synergy ----
    [Header("Combo Synergy")]
    public bool enableComboSynergy = true;

    [Tooltip("ถึงคอมโบเท่านี้ครั้ง จะยิง Heavy Shot หนึ่งทีในคอมโบนั้น")]
    public int comboForHeavyShot = 20;

    [Tooltip("ถึงคอมโบเท่านี้ครั้ง จะยิง Super Shot หนึ่งทีในคอมโบนั้น")]
    public int comboForSuperShot = 50;

    [Tooltip("ถึงคอมโบเท่านี้ครั้ง จะยิง Signature Shot หนึ่งทีในคอมโบนั้น")]
    public int comboForSignatureShot = 100;

    [Tooltip("ตัวคูณดาเมจ Heavy / Super / Signature")]
    public float heavyShotMultiplier = 1.6f;
    public float superShotMultiplier = 2.3f;
    public float signatureShotMultiplier = 3.5f;

    private bool heavyShotUsedThisCombo;
    private bool superShotUsedThisCombo;
    private bool signatureShotUsedThisCombo;

    private float attackInterval;
    private float nextAttackTime;

    private enum ComboShotType
    {
        Heavy,
        Super,
        Signature
    }

    private void Awake()
    {
        if (upgradeManager == null)
        {
            upgradeManager = FindFirstObjectByType<UpgradeManager>();
            if (upgradeManager == null)
            {
                Debug.LogWarning("Companion: ไม่พบ UpgradeManager ในซีน");
            }
        }

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogWarning("Companion: ไม่พบ GameManager ในซีน");
            }
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        RecalculateAttackInterval();
    }

    private void OnEnable()
    {
        nextAttackTime = Time.time;
    }

    private void Update()
    {
        if (!isActive) return;
        if (gameManager == null) return;

        Monster target = gameManager.activeMonster;
        if (target == null || target.currentHealth <= 0f)
            return;

        // === CHECK HP PERCENT BEFORE ATTACK ===
        float maxHP = Mathf.Max(1f, target.maxHealth); // กัน maxHealth = 0
        float hpPercent = target.currentHealth / maxHP;

        // ถ้า HP <= 1% (หรือค่าที่ตั้งใน minHpPercentToAttack) จะไม่ยิงแล้ว
        if (hpPercent <= minHpPercentToAttack)
        {
            if (showDebugLog)
            {
                Debug.Log($"Companion: Stop attacking, target HP is below {minHpPercentToAttack * 100f:F2}%");
            }
            return;
        }

        if (Time.time >= nextAttackTime)
        {
            PerformAttack(target);
            nextAttackTime = Time.time + attackInterval;
        }
    }

    #region Attack & Damage

    private float CalculateDamage()
    {
        float playerDamage = 1f;
        if (upgradeManager != null)
        {
            playerDamage = upgradeManager.GetCurrentDamage();
        }

        float levelBonus = damageMultiplierPerLevel * (level - 1);
        float finalMultiplier = baseDamageMultiplier + levelBonus;

        if (finalMultiplier < 0f)
            finalMultiplier = 0f;

        float damage = playerDamage * finalMultiplier;
        return damage;
    }

    private void RecalculateAttackInterval()
    {
        float bonusPercent = attackSpeedPercentPerLevel * (level - 1);
        float speedMultiplier = 1f + bonusPercent;

        float finalAPS = Mathf.Max(0.1f, baseAttacksPerSecond * speedMultiplier);
        attackInterval = 1f / finalAPS;
    }

    private void PerformAttack(Monster target)
    {
        if (target == null || target.currentHealth <= 0f)
            return;

        // ป้องกันกรณีอื่น ๆ ที่เรียก PerformAttack ตรง ๆ
        float maxHP = Mathf.Max(1f, target.maxHealth);
        float hpPercent = target.currentHealth / maxHP;
        if (hpPercent <= minHpPercentToAttack)
        {
            if (showDebugLog)
            {
                Debug.Log($"Companion: PerformAttack canceled, target HP is below {minHpPercentToAttack * 100f:F2}%");
            }
            return;
        }

        float damage = CalculateDamage();

        PlayAttackAnimation();
        PlaySfx(attackSfx);

        // ปกติใช้ VFX ปกติ
        SpawnAttackVfx(target, attackVfxPrefab, damage, vfxTravelTime, logPrefix: "Normal");
    }

    private void SpawnAttackVfx(
        Monster target,
        CompanionAttackVFX prefab,
        float damage,
        float travelTime,
        string logPrefix)
    {
        if (target == null) return;

        if (prefab != null)
        {
            Vector3 spawnPos = transform.position;
            if (vfxSpawnPoint != null)
                spawnPos = vfxSpawnPoint.position;

            CompanionAttackVFX vfx = Instantiate(prefab, spawnPos, Quaternion.identity);
            vfx.Initialize(target, damage, travelTime > 0f ? travelTime : vfxTravelTime);

            if (showDebugLog)
            {
                Debug.Log($"Companion [{logPrefix}]: Spawn VFX -> target {target.name}, dmg {damage:F1}, Lv.{level}");
            }
        }
        else
        {
            // Fallback: ยิงดาเมจตรง
            MonsterDamageBypass bypass = target.GetComponent<MonsterDamageBypass>();
            if (bypass != null)
            {
                bypass.ApplyDirectDamage(damage);
                if (showDebugLog)
                {
                    Debug.Log($"Companion [{logPrefix}]: Direct BYPASS dmg {damage:F1} (no VFX) Lv.{level}");
                }
            }
            else
            {
                target.TakeDamage(damage);
                if (showDebugLog)
                {
                    Debug.LogWarning($"Companion [{logPrefix}]: Direct TakeDamage {damage:F1} (no VFX, no bypass) Lv.{level}");
                }
            }
        }
    }

    #endregion

    #region Animation & SFX Helpers

    private void PlayAttackAnimation()
    {
        if (animator == null) return;

        if (!string.IsNullOrEmpty(attackTriggerName))
        {
            animator.SetTrigger(attackTriggerName);
        }
        else if (!string.IsNullOrEmpty(idleStateName))
        {
            animator.Play(idleStateName, 0, 0f);
        }
    }

    private void PlayAnimatorStateIfValid(string stateName)
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(stateName)) return;

        animator.Play(stateName, 0, 0f);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null) return;

        audioSource.PlayOneShot(clip);
    }

    #endregion

    #region Upgrade Logic

    public int GetNextUpgradeCost()
    {
        if (!CanUpgrade())
            return 0;

        int nextLevel = level + 1;
        float cost = baseUpgradeCost * Mathf.Pow(upgradeCostGrowth, nextLevel - 1);
        return Mathf.CeilToInt(cost);
    }

    public bool CanUpgrade()
    {
        return level < maxLevel;
    }

    public void Upgrade()
    {
        if (!CanUpgrade())
        {
            if (showDebugLog)
                Debug.Log("Companion: ถึงเลเวลสูงสุดแล้ว");
            return;
        }

        level++;
        RecalculateAttackInterval();

        if (showDebugLog)
        {
            float mult = baseDamageMultiplier + damageMultiplierPerLevel * (level - 1);
            Debug.Log("Companion Upgrade => Lv." + level + ", Damage Multiplier Now ≈ " + mult.ToString("F2"));
        }
    }

    #endregion

    #region Public Controls

    public void SetActive(bool active)
    {
        isActive = active;

        if (showDebugLog)
        {
            Debug.Log("Companion: Active = " + isActive);
        }

        if (!isActive)
        {
            if (animator != null && !string.IsNullOrEmpty(idleStateName))
            {
                animator.Play(idleStateName, 0, 0f);
            }
        }
        else
        {
            nextAttackTime = Time.time;
        }
    }

    #endregion

    #region Ultimate Collaboration API

    /// <summary>
    /// เรียกจากสคริปต์ UltimateSkill ตอนที่ผู้เล่นกดอัลติ (OnUltStart)
    /// </summary>
    public void OnPlayerUltStarted()
    {
        if (!enableUltCollab) return;
        if (gameManager == null) return;

        Monster target = gameManager.activeMonster;
        if (target == null || target.currentHealth <= 0f)
            return;

        // เคารพ limit 1% เหมือนเดิม
        float maxHP = Mathf.Max(1f, target.maxHealth);
        float hpPercent = target.currentHealth / maxHP;
        if (hpPercent <= minHpPercentToAttack)
            return;

        // เล่น Timeline ถ้ามี
        if (coopUltTimeline != null)
        {
            coopUltTimeline.Play();
        }

        // เล่นแอนิเมชัน / SFX
        PlayAnimatorStateIfValid(ultCoopStateName);
        PlaySfx(ultCoopSfx);

        // ยิง VFX พิเศษ + ดาเมจคูณ
        float dmg = CalculateDamage() * ultBonusDamageMultiplier;

        float travel = ultVfxTravelTime > 0f ? ultVfxTravelTime : vfxTravelTime;
        SpawnAttackVfx(target, ultCoopVfxPrefab != null ? ultCoopVfxPrefab : attackVfxPrefab,
            dmg, travel, "UltCollab");
    }

    #endregion

    #region Combo Synergy API

    /// <summary>
    /// ให้ระบบคอมโบเรียกทุกครั้งที่ค่า combo เปลี่ยน
    /// ใส่ใน ComboSystem: companion.OnComboChanged(currentCombo);
    /// </summary>
    public void OnComboChanged(int combo)
    {
        if (!enableComboSynergy) return;

        // ถ้าคอมโบหลุด รีเซ็ตสถานะการยิงพิเศษ
        if (combo <= 0)
        {
            heavyShotUsedThisCombo = false;
            superShotUsedThisCombo = false;
            signatureShotUsedThisCombo = false;
            return;
        }

        if (gameManager == null) return;

        Monster target = gameManager.activeMonster;
        if (target == null || target.currentHealth <= 0f)
            return;

        float maxHP = Mathf.Max(1f, target.maxHealth);
        float hpPercent = target.currentHealth / maxHP;
        if (hpPercent <= minHpPercentToAttack)
            return;

        // เรียงจากแรงสุดไปอ่อนสุด (จะได้ไม่เผลอใช้ Heavy ก่อนทั้งที่มี Signature)
        if (combo >= comboForSignatureShot && !signatureShotUsedThisCombo)
        {
            signatureShotUsedThisCombo = true;
            TriggerComboShot(target, ComboShotType.Signature);
        }
        else if (combo >= comboForSuperShot && !superShotUsedThisCombo)
        {
            superShotUsedThisCombo = true;
            TriggerComboShot(target, ComboShotType.Super);
        }
        else if (combo >= comboForHeavyShot && !heavyShotUsedThisCombo)
        {
            heavyShotUsedThisCombo = true;
            TriggerComboShot(target, ComboShotType.Heavy);
        }
    }

    private void TriggerComboShot(Monster target, ComboShotType type)
    {
        if (target == null || target.currentHealth <= 0f)
            return;

        float baseDmg = CalculateDamage();

        CompanionAttackVFX prefab = attackVfxPrefab;
        string animState = null;
        AudioClip sfx = null;
        float multiplier = 1f;
        string logPrefix = "Combo";

        switch (type)
        {
            case ComboShotType.Heavy:
                prefab = comboHeavyVfxPrefab != null ? comboHeavyVfxPrefab : attackVfxPrefab;
                animState = comboHeavyStateName;
                sfx = comboHeavySfx;
                multiplier = heavyShotMultiplier;
                logPrefix = "ComboHeavy";
                break;

            case ComboShotType.Super:
                prefab = comboSuperVfxPrefab != null ? comboSuperVfxPrefab : attackVfxPrefab;
                animState = comboSuperStateName;
                sfx = comboSuperSfx;
                multiplier = superShotMultiplier;
                logPrefix = "ComboSuper";
                break;

            case ComboShotType.Signature:
                prefab = signatureVfxPrefab != null ? signatureVfxPrefab : attackVfxPrefab;
                animState = signatureStateName;
                sfx = signatureSfx;
                multiplier = signatureShotMultiplier;
                logPrefix = "ComboSignature";
                break;
        }

        float dmg = baseDmg * multiplier;

        // เล่นอนิเมชัน + SFX
        PlayAnimatorStateIfValid(animState);
        PlaySfx(sfx);

        // ยิง VFX พิเศษ
        SpawnAttackVfx(target, prefab, dmg, vfxTravelTime, logPrefix);
    }

    #endregion

    #region Emotion Helpers (ใช้ในอนาคตได้)

    /// <summary>
    /// เรียกเวลา player ทำอะไรเท่ ๆ เช่น ติดคริต่อเนื่อง
    /// </summary>
    public void ReactHappy()
    {
        PlayAnimatorStateIfValid(happyStateName);
    }

    /// <summary>
    /// เรียกตอน Boss เข้า Break, Companion บ้าพลัง
    /// </summary>
    public void EnterBerserkMode()
    {
        PlayAnimatorStateIfValid(berserkStateName);
    }

    #endregion
}
