using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// เพื่อนร่วมสู้ (Companion) ที่ยิงมอนสเตอร์ให้อัตโนมัติ
/// - ยิงตาม GameManager.activeMonster
/// - มี Friendship Level, Combo Synergy, Ult Collab, VFX/SFX, UI
/// - จะไม่รับ EXP และไม่เลเวลอัปถ้าเพื่อนไม่ Active
/// </summary>
public class Companion : MonoBehaviour
{
    // -------------------------
    //  BASE STATS (ค่าพื้นฐานของเพื่อน)
    // -------------------------
    
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

    // ======================================================
    // FRIENDSHIP SYSTEM (ระบบความสนิทของเพื่อน)
    // ======================================================

    [Header("Friendship (Buddy System)")]
    [Tooltip("ชื่อเพื่อน เช่น Gear / Bolt / Nova")]
    public string friendName = "Gear";

    [Tooltip("เลเวลความสนิทเริ่มต้น")]
    public int friendshipLevel = 1;

    [Tooltip("เลเวลความสนิทสูงสุด")]
    public int maxFriendshipLevel = 10;

    [Tooltip("Exp ปัจจุบันของเลเวลนี้")]
    public int currentFriendshipExp = 0;

    [Tooltip("Exp ที่ต้องใช้ต่อ 1 เลเวล")]
    public int expPerLevel = 100;

    [Tooltip("ดาเมจบัฟต่อ 1 เลเวลความสนิท เช่น 0.05 = +5% ต่อ Lv")]
    [Range(0f, 1f)]
    public float friendshipDamageBonusPerLevel = 0.05f;

    // UI ที่เอาไว้แสดงชื่อ, เลเวล, และ Exp bar
    [Header("Friendship UI (Optional)")]
    [Tooltip("Text ชื่อเพื่อน (เช่น Gear)")]
    public TextMeshProUGUI friendshipNameText;

    [Tooltip("Text แสดง Level เช่น Lv.3")]
    public TextMeshProUGUI friendshipLevelText;

    [Tooltip("Slider แสดง Exp ความสนิท")]
    public Slider friendshipExpSlider;

    [Header("Friendship Rewards")]
    [Tooltip("EXP ที่ได้เมื่อฆ่ามอน 1 ตัว")]
    public int expOnMonsterKill = 20;

    [Tooltip("EXP ที่ได้เมื่อทำคอมโบระดับ S (Signature)")]
    public int expOnSCombo = 50;

    // ======================================================
    // VFX / SFX (สำหรับโจมตีธรรมดา / คอมโบ / อัลติ)
    // ======================================================

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
    [SerializeField] private bool isActive = true;     // ใช้เปิด/ปิดเพื่อน (ตรรกะภายใน)
    [SerializeField] private bool showDebugLog = true; // เปิด/ปิด debug log

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

    // state ต่าง ๆ ใน Animator สำหรับคอมโบ / อัลติ / อารมณ์
    [Header("Animator States (Combo / Ult / Emotion)")]
    public string comboHeavyStateName = "ComboHeavy";
    public string comboSuperStateName = "ComboSuper";
    public string signatureStateName = "Signature";
    public string ultCoopStateName = "UltCoop";
    public string happyStateName = "Happy";
    public string berserkStateName = "Berserk";

    // เสียงต่าง ๆ
    [Header("SFX")]
    public AudioSource audioSource;
    public AudioClip attackSfx;
    public AudioClip comboHeavySfx;
    public AudioClip comboSuperSfx;
    public AudioClip signatureSfx;
    public AudioClip ultCoopSfx;

    // ======================================================
    // HP LIMIT (ไม่โจมตีถ้าเลือดมอนต่ำกว่า % ที่กำหนด)
    // ======================================================

    [Header("Attack Limit")]
    [Tooltip("หยุดโจมตีเมื่อ HP ของมอนสเตอร์ต่ำกว่าค่านี้ (เช่น 0.01 = 1%)")]
    [Range(0f, 1f)]
    public float minHpPercentToAttack = 0.01f;

    // ======================================================
    // ULT COLLAB (อัลติร่วมกับผู้เล่น)
    // ======================================================

    [Header("Ultimate Collaboration")]
    [Tooltip("เปิด / ปิด ระบบอัลติร่วมกับผู้เล่น")]
    public bool enableUltCollab = true;

    [Tooltip("Timeline สำหรับคัตซีนอัลติร่วม (ไม่จำเป็นต้องใส่ก็ได้)")]
    public PlayableDirector coopUltTimeline;

    [Tooltip("ตัวคูณดาเมจพิเศษตอนทำ Ult ร่วม (x เท่าจากดาเมจปกติของ Companion)")]
    public float ultBonusDamageMultiplier = 3f;

    [Tooltip("ระยะเวลาเดินทางของ VFX ตอน Ult ร่วม (ถ้าตั้ง ≤ 0 จะใช้ vfxTravelTime ปกติ)")]
    public float ultVfxTravelTime = 0.25f;

    // ======================================================
    // COMBO SYNERGY (ยิงสกิลพิเศษตามคอมโบผู้เล่น)
    // ======================================================

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

    // flag ว่าในคอมโบชุดนี้ใช้สกิลไปแล้วหรือยัง (กันยิงซ้ำ)
    private bool heavyShotUsedThisCombo;
    private bool superShotUsedThisCombo;
    private bool signatureShotUsedThisCombo;

    // จัดการความถี่การยิง
    private float attackInterval;
    private float nextAttackTime;

    // enum ใช้ระบุประเภท combo shot แบบอ่านง่าย
    private enum ComboShotType
    {
        Heavy,
        Super,
        Signature
    }

    // ------------------------------------------------------
    // Awake: auto-find reference และคำนวณค่าเริ่มต้น
    // ------------------------------------------------------
    private void Awake()
    {
        if (upgradeManager == null)
        {
            upgradeManager = FindFirstObjectByType<UpgradeManager>();
            if (upgradeManager == null && showDebugLog)
            {
                Debug.LogWarning($"Companion[{friendName}]: ไม่พบ UpgradeManager ในซีน");
            }
        }

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager == null && showDebugLog)
            {
                Debug.LogWarning($"Companion[{friendName}]: ไม่พบ GameManager ในซีน");
            }
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        RecalculateAttackInterval();
        UpdateFriendshipUI();

        if (showDebugLog)
        {
            Debug.Log($"Companion[{friendName}] Awake() | Lv.{level}, F-Lv.{friendshipLevel}, isActive={isActive}");
        }
    }

    // ------------------------------------------------------
    // OnEnable: reset timer, update UI
    // ------------------------------------------------------
    private void OnEnable()
    {
        nextAttackTime = Time.time;
        UpdateFriendshipUI();

        if (showDebugLog)
        {
            Debug.Log($"Companion[{friendName}] OnEnable() | activeInHierarchy={gameObject.activeInHierarchy}");
        }
    }

    // ------------------------------------------------------
    // Update: ยิงมอนเมื่อถึงเวลา + เช็ค HP %
    // ------------------------------------------------------
    private void Update()
    {
        if (!isActive) return;
        if (gameManager == null) return;

        Monster target = gameManager.activeMonster;
        if (target == null || target.currentHealth <= 0f)
            return;

        float maxHP = Mathf.Max(1f, target.maxHealth);
        float hpPercent = target.currentHealth / maxHP;

        // ถ้า HP ต่ำกว่า limit ให้หยุดยิง
        if (hpPercent <= minHpPercentToAttack)
        {
            if (showDebugLog)
            {
                Debug.Log($"Companion[{friendName}]: Stop attacking, target HP {hpPercent * 100f:F1}% <= {minHpPercentToAttack * 100f:F1}%");
            }
            return;
        }

        if (Time.time >= nextAttackTime)
        {
            PerformAttack(target);
            nextAttackTime = Time.time + attackInterval;
        }
    }

    // ======================================================
    // DAMAGE + FRIENDSHIP (คำนวณดาเมจของเพื่อน)
    // ======================================================

    private float CalculateDamage()
    {
        float playerDamage = 1f;
        if (upgradeManager != null)
        {
            playerDamage = upgradeManager.GetCurrentDamage();
        }

        float levelBonus = damageMultiplierPerLevel * (level - 1);
        float baseMult = baseDamageMultiplier + levelBonus;
        if (baseMult < 0f) baseMult = 0f;

        int effectiveFriendLv = Mathf.Clamp(friendshipLevel, 1, maxFriendshipLevel);
        float friendshipBonusPercent = friendshipDamageBonusPerLevel * (effectiveFriendLv - 1);
        float friendshipMultiplier = 1f + friendshipBonusPercent;

        float finalMultiplier = baseMult * friendshipMultiplier;
        float damage = playerDamage * finalMultiplier;

        return damage;
    }

    private void RecalculateAttackInterval()
    {
        float bonusPercent = attackSpeedPercentPerLevel * (level - 1);
        float speedMultiplier = 1f + bonusPercent;

        float finalAPS = Mathf.Max(0.1f, baseAttacksPerSecond * speedMultiplier);
        attackInterval = 1f / finalAPS;

        if (showDebugLog)
        {
            Debug.Log($"Companion[{friendName}] RecalculateAttackInterval => APS={finalAPS:F2}, interval={attackInterval:F3}s");
        }
    }

    private void PerformAttack(Monster target)
    {
        if (target == null || target.currentHealth <= 0f)
            return;

        float maxHP = Mathf.Max(1f, target.maxHealth);
        float hpPercent = target.currentHealth / maxHP;
        if (hpPercent <= minHpPercentToAttack)
        {
            if (showDebugLog)
            {
                Debug.Log($"Companion[{friendName}]: PerformAttack canceled, target HP {hpPercent * 100f:F1}% <= {minHpPercentToAttack * 100f:F1}%");
            }
            return;
        }

        float damage = CalculateDamage();

        PlayAttackAnimation();
        PlaySfx(attackSfx);

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
                Debug.Log($"Companion[{friendName}][{logPrefix}] Spawn VFX -> {target.name}, dmg {damage:F1}, Lv.{level}, F-Lv.{friendshipLevel}");
            }
        }
        else
        {
            MonsterDamageBypass bypass = target.GetComponent<MonsterDamageBypass>();
            if (bypass != null)
            {
                bypass.ApplyDirectDamage(damage);
                if (showDebugLog)
                {
                    Debug.Log($"Companion[{friendName}][{logPrefix}] Direct BYPASS dmg {damage:F1} (no VFX)");
                }
            }
            else
            {
                target.TakeDamage(damage);
                if (showDebugLog)
                {
                    Debug.LogWarning($"Companion[{friendName}][{logPrefix}] Direct TakeDamage {damage:F1} (no VFX, no bypass)");
                }
            }
        }
    }

    // ======================================================
    // ANIMATION & SFX HELPERS
    // ======================================================

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

    // ======================================================
    // UPGRADE LOGIC (อัปเลเวลเพื่อน)
    // ======================================================

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
                Debug.Log($"Companion[{friendName}]: ถึงเลเวลสูงสุดแล้ว (Lv.{level})");
            return;
        }

        level++;
        RecalculateAttackInterval();

        if (showDebugLog)
        {
            float mult = baseDamageMultiplier + damageMultiplierPerLevel * (level - 1);
            Debug.Log($"Companion[{friendName}] Upgrade => Lv.{level}, Damage Multiplier ≈ {mult:F2}");
        }
    }

    // ======================================================
    // PUBLIC CONTROLS (เปิด/ปิด Companion)
    // ======================================================

    public void SetActive(bool active)
    {
        isActive = active;

        if (showDebugLog)
        {
            Debug.Log($"Companion[{friendName}]: SetActive({active})");
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

    // ======================================================
    // ULT COLLAB (ถูกเรียกตอนผู้เล่นกดอัลติ)
    // ======================================================

    public void OnPlayerUltStarted()
    {
        if (!enableUltCollab) return;
        if (!isActive) return;
        if (!gameObject.activeInHierarchy) return;
        if (gameManager == null) return;

        Monster target = gameManager.activeMonster;
        if (target == null || target.currentHealth <= 0f)
            return;

        float maxHP = Mathf.Max(1f, target.maxHealth);
        float hpPercent = target.currentHealth / maxHP;
        if (hpPercent <= minHpPercentToAttack)
            return;

        if (coopUltTimeline != null)
        {
            coopUltTimeline.Play();
        }

        PlayAnimatorStateIfValid(ultCoopStateName);
        PlaySfx(ultCoopSfx);

        float dmg = CalculateDamage() * ultBonusDamageMultiplier;
        float travel = ultVfxTravelTime > 0f ? ultVfxTravelTime : vfxTravelTime;

        SpawnAttackVfx(
            target,
            ultCoopVfxPrefab != null ? ultCoopVfxPrefab : attackVfxPrefab,
            dmg,
            travel,
            "UltCollab");
    }

    // ======================================================
    // COMBO SYNERGY
    // ======================================================

    public void OnComboChanged(int combo)
    {
        if (!enableComboSynergy) return;
        if (!isActive) return;
        if (!gameObject.activeInHierarchy) return;

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

        if (combo >= comboForSignatureShot && !signatureShotUsedThisCombo)
        {
            signatureShotUsedThisCombo = true;
            TriggerComboShot(target, ComboShotType.Signature);
            AddFriendshipExp(expOnSCombo);
        }
        else if (combo >= comboForSuperShot && !superShotUsedThisCombo)
        {
            superShotUsedThisCombo = true;
            TriggerComboShot(target, ComboShotType.Super);
            AddFriendshipExp(5);
        }
        else if (combo >= comboForHeavyShot && !heavyShotUsedThisCombo)
        {
            heavyShotUsedThisCombo = true;
            TriggerComboShot(target, ComboShotType.Heavy);
            AddFriendshipExp(3);
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

        PlayAnimatorStateIfValid(animState);
        PlaySfx(sfx);

        SpawnAttackVfx(target, prefab, dmg, vfxTravelTime, logPrefix);
    }

    // ======================================================
    // FRIENDSHIP LOGIC + UI
    // ======================================================

    public void AddFriendshipExp(int amount)
    {
        // 🔒 ถ้าเพื่อนไม่ active → ห้ามรับ EXP
        if (!isActive)
        {
            if (showDebugLog)
                Debug.Log($"Companion[{friendName}] AddFriendshipExp({amount}) blocked: isActive == false");
            return;
        }

        if (!gameObject.activeInHierarchy)
        {
            if (showDebugLog)
                Debug.Log($"Companion[{friendName}] AddFriendshipExp({amount}) blocked: gameObject not activeInHierarchy");
            return;
        }

        if (amount <= 0) return;
        if (friendshipLevel >= maxFriendshipLevel) return;

        if (showDebugLog)
        {
            Debug.Log($"Companion[{friendName}] Gain Friendship EXP +{amount} (before {currentFriendshipExp}/{expPerLevel}, Lv.{friendshipLevel})");
        }

        currentFriendshipExp += amount;

        bool leveledUp = false;

        while (currentFriendshipExp >= expPerLevel && friendshipLevel < maxFriendshipLevel)
        {
            currentFriendshipExp -= expPerLevel;
            friendshipLevel++;
            leveledUp = true;
        }

        if (leveledUp && showDebugLog)
        {
            Debug.Log($"Companion[{friendName}] Friendship Level Up! Lv.{friendshipLevel} (EXP now {currentFriendshipExp}/{expPerLevel})");
        }

        UpdateFriendshipUI();
    }

    private void UpdateFriendshipUI()
    {
        if (friendshipNameText != null)
            friendshipNameText.text = friendName;

        if (friendshipLevelText != null)
            friendshipLevelText.text = $"Lv.{friendshipLevel}";

        if (friendshipExpSlider != null)
        {
            friendshipExpSlider.minValue = 0;
            friendshipExpSlider.maxValue = expPerLevel;
            friendshipExpSlider.value = Mathf.Clamp(currentFriendshipExp, 0, expPerLevel);
        }
    }

    public void OnMonsterKilled()
    {
        if (showDebugLog)
        {
            Debug.Log($"Companion[{friendName}] OnMonsterKilled() called");
        }
        AddFriendshipExp(expOnMonsterKill);
    }

    // ======================================================
    // EMOTION HELPERS
    // ======================================================

    public void ReactHappy()
    {
        PlayAnimatorStateIfValid(happyStateName);
    }

    public void EnterBerserkMode()
    {
        PlayAnimatorStateIfValid(berserkStateName);
    }
}
