using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// เพื่อนร่วมสู้ (Companion) ที่ยิงมอนสเตอร์ให้อัตโนมัติ
/// ใช้ GameManager.activeMonster เป็นเป้าหมายหลัก
/// มี: คอมโบซินเนอร์จี้ + อัลติคอลแลป + SFX/VFX + Friendship Level + UI
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
        // หา UpgradeManager ถ้าไม่ได้ลากจาก Inspector
        if (upgradeManager == null)
        {
            upgradeManager = FindFirstObjectByType<UpgradeManager>();
            if (upgradeManager == null)
            {
                Debug.LogWarning("Companion: ไม่พบ UpgradeManager ในซีน");
            }
        }

        // หา GameManager ถ้าไม่ได้ลากจาก Inspector
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogWarning("Companion: ไม่พบ GameManager ในซีน");
            }
        }

        // หา AudioSource ถ้ายังไม่ได้ใส่
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // คำนวณความถี่ในการยิงครั้งแรก
        RecalculateAttackInterval();

        // อัปเดต UI Friendship รอบแรก
        UpdateFriendshipUI();
    }

    // ------------------------------------------------------
    // OnEnable: reset timer, update UI
    // ------------------------------------------------------
    private void OnEnable()
    {
        nextAttackTime = Time.time;
        UpdateFriendshipUI();
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

        // หาร % HP ของมอน
        float maxHP = Mathf.Max(1f, target.maxHealth);
        float hpPercent = target.currentHealth / maxHP;

        // ถ้า HP ต่ำกว่า limit (เช่น 1%) ให้หยุดยิง และให้ผู้เล่นไปเก็บเอง
        if (hpPercent <= minHpPercentToAttack)
        {
            if (showDebugLog)
            {
                Debug.Log($"Companion: Stop attacking, target HP is below {minHpPercentToAttack * 100f:F2}%");
            }
            return;
        }

        // ยิงเมื่อถึงเวลา
        if (Time.time >= nextAttackTime)
        {
            PerformAttack(target);
            nextAttackTime = Time.time + attackInterval;
        }
    }

    // ======================================================
    // DAMAGE + FRIENDSHIP (คำนวณดาเมจของเพื่อน)
    // ======================================================

    /// <summary>
    /// คำนวณดาเมจของ Companion โดยอิงจากดาเมจผู้เล่น + เลเวลเพื่อน + ความสนิท
    /// </summary>
    private float CalculateDamage()
    {
        // ดาเมจเบสจากผู้เล่น (อ่านจาก UpgradeManager)
        float playerDamage = 1f;
        if (upgradeManager != null)
        {
            playerDamage = upgradeManager.GetCurrentDamage();
        }

        // Bonus จากเลเวลของ Companion เอง
        float levelBonus = damageMultiplierPerLevel * (level - 1);
        float baseMult = baseDamageMultiplier + levelBonus;

        if (baseMult < 0f)
            baseMult = 0f;

        // ⭐ Bonus จาก Friendship Level
        int effectiveFriendLv = Mathf.Clamp(friendshipLevel, 1, maxFriendshipLevel);
        float friendshipBonusPercent = friendshipDamageBonusPerLevel * (effectiveFriendLv - 1);
        float friendshipMultiplier = 1f + friendshipBonusPercent;

        // รวม multiplier ทั้งหมด
        float finalMultiplier = baseMult * friendshipMultiplier;

        // ดาเมจสุดท้าย = ดาเมจผู้เล่น * multiplier
        float damage = playerDamage * finalMultiplier;
        return damage;
    }

    /// <summary>
    /// คำนวณระยะห่างระหว่างการยิง (ใช้ APS) ทุกครั้งที่เลเวลเปลี่ยน
    /// </summary>
    private void RecalculateAttackInterval()
    {
        float bonusPercent = attackSpeedPercentPerLevel * (level - 1);
        float speedMultiplier = 1f + bonusPercent;

        float finalAPS = Mathf.Max(0.1f, baseAttacksPerSecond * speedMultiplier);
        attackInterval = 1f / finalAPS;
    }

    /// <summary>
    /// ยิงหนึ่งครั้งไปที่มอน: เช็ค HP, คำนวณดาเมจ, เล่นอนิเมชัน, Spawn VFX
    /// </summary>
    private void PerformAttack(Monster target)
    {
        if (target == null || target.currentHealth <= 0f)
            return;

        // กันไม่ให้เรียกตรง ๆ แล้วไปยิงตอน HP ต่ำกว่า 1%
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

        // เล่นอนิเมชันโจมตี + SFX
        PlayAttackAnimation();
        PlaySfx(attackSfx);

        // สร้าง VFX หรือยิงดาเมจตรงถ้าไม่มี VFX
        SpawnAttackVfx(target, attackVfxPrefab, damage, vfxTravelTime, logPrefix: "Normal");
    }

    /// <summary>
    /// ฟังก์ชันกลางสำหรับสร้าง VFX ที่บินไปหา Monster แล้วทำดาเมจ
    /// ถ้าไม่มี prefab จะยิงดาเมจตรง (bypass หรือ TakeDamage)
    /// </summary>
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
            // ตำแหน่ง spawn (ใช้ตัวเพื่อนหรือจุดยิงเฉพาะ)
            Vector3 spawnPos = transform.position;
            if (vfxSpawnPoint != null)
                spawnPos = vfxSpawnPoint.position;

            // สร้าง VFX และส่งเป้าหมาย + ดาเมจให้มัน
            CompanionAttackVFX vfx = Instantiate(prefab, spawnPos, Quaternion.identity);
            vfx.Initialize(target, damage, travelTime > 0f ? travelTime : vfxTravelTime);

            if (showDebugLog)
            {
                Debug.Log($"Companion [{logPrefix}]: Spawn VFX -> target {target.name}, dmg {damage:F1}, Lv.{level}, F-Lv.{friendshipLevel}");
            }
        }
        else
        {
            // ถ้าไม่มี VFX ใช้ระบบ damage bypass หรือ TakeDamage ตรง ๆ
            MonsterDamageBypass bypass = target.GetComponent<MonsterDamageBypass>();
            if (bypass != null)
            {
                bypass.ApplyDirectDamage(damage);
                if (showDebugLog)
                {
                    Debug.Log($"Companion [{logPrefix}]: Direct BYPASS dmg {damage:F1} (no VFX) Lv.{level}, F-Lv.{friendshipLevel}");
                }
            }
            else
            {
                target.TakeDamage(damage);
                if (showDebugLog)
                {
                    Debug.LogWarning($"Companion [{logPrefix}]: Direct TakeDamage {damage:F1} (no VFX, no bypass) Lv.{level}, F-Lv.{friendshipLevel}");
                }
            }
        }
    }

    // ======================================================
    // ANIMATION & SFX HELPERS
    // ======================================================

    /// <summary>
    /// เล่นอนิเมชันโจมตีปกติ (ใช้ trigger หรือ state)
    /// </summary>
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

    /// <summary>
    /// เล่น state ตรง ๆ ใน Animator ถ้าตั้งชื่อไว้
    /// </summary>
    private void PlayAnimatorStateIfValid(string stateName)
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(stateName)) return;

        animator.Play(stateName, 0, 0f);
    }

    /// <summary>
    /// เล่นเสียงหนึ่งครั้งด้วย AudioSource.PlayOneShot
    /// </summary>
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

    /// <summary>
    /// คืนค่า cost ที่ต้องใช้สำหรับอัปเลเวลครั้งถัดไป
    /// </summary>
    public int GetNextUpgradeCost()
    {
        if (!CanUpgrade())
            return 0;

        int nextLevel = level + 1;
        float cost = baseUpgradeCost * Mathf.Pow(upgradeCostGrowth, nextLevel - 1);
        return Mathf.CeilToInt(cost);
    }

    /// <summary>
    /// เช็คว่าตอนนี้ยังอัปเลเวลเพื่อนได้ไหม (ถึง maxLevel หรือยัง)
    /// </summary>
    public bool CanUpgrade()
    {
        return level < maxLevel;
    }

    /// <summary>
    /// อัปเลเวลเพื่อน 1 เลเวล + อัปเดต speed ยิง
    /// </summary>
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

    // ======================================================
    // PUBLIC CONTROLS (เปิด/ปิด Companion)
    // ======================================================

    /// <summary>
    /// เปิด/ปิด การทำงานของ Companion (เช่น ตอนอยู่ใน cutscene)
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;

        if (showDebugLog)
        {
            Debug.Log("Companion: Active = " + isActive);
        }

        if (!isActive)
        {
            // ถ้าปิดให้กลับไป Idle
            if (animator != null && !string.IsNullOrEmpty(idleStateName))
            {
                animator.Play(idleStateName, 0, 0f);
            }
        }
        else
        {
            // ถ้าเปิดใหม่ reset timer การยิง
            nextAttackTime = Time.time;
        }
    }

    // ======================================================
    // ULT COLLAB (ถูกเรียกตอนผู้เล่นกดอัลติ)
    // ======================================================

    /// <summary>
    /// เรียกจากสคริปต์ UltimateSkill ตอนที่ผู้เล่นกดอัลติ (OnUltStart)
    /// Companion จะเล่น Timeline + ยิงสกิลแรง ๆ
    /// </summary>
    public void OnPlayerUltStarted()
    {
        if (!enableUltCollab) return;
        if (gameManager == null) return;

        Monster target = gameManager.activeMonster;
        if (target == null || target.currentHealth <= 0f)
            return;

        // เคารพ limit % HP เหมือนเดิม
        float maxHP = Mathf.Max(1f, target.maxHealth);
        float hpPercent = target.currentHealth / maxHP;
        if (hpPercent <= minHpPercentToAttack)
            return;

        // เล่น Timeline ถ้ามี
        if (coopUltTimeline != null)
        {
            coopUltTimeline.Play();
        }

        // เล่นอนิเมชัน + เสียง
        PlayAnimatorStateIfValid(ultCoopStateName);
        PlaySfx(ultCoopSfx);

        // คำนวณดาเมจคูณ ultBonusDamageMultiplier
        float dmg = CalculateDamage() * ultBonusDamageMultiplier;
        float travel = ultVfxTravelTime > 0f ? ultVfxTravelTime : vfxTravelTime;

        // ยิง VFX อัลติ ถ้าไม่มีใช้ VFX ปกติแทน
        SpawnAttackVfx(
            target,
            ultCoopVfxPrefab != null ? ultCoopVfxPrefab : attackVfxPrefab,
            dmg,
            travel,
            "UltCollab");
    }

    // ======================================================
    // COMBO SYNERGY (เชื่อมกับระบบคอมโบของผู้เล่น)
    // ======================================================

    /// <summary>
    /// ให้ระบบคอมโบเรียกทุกครั้งที่ค่า combo เปลี่ยน
    /// ใส่ใน ComboSystem: companion.OnComboChanged(currentCombo);
    /// </summary>
    public void OnComboChanged(int combo)
    {
        if (!enableComboSynergy) return;
        if (!isActive) return;
        if (!gameObject.activeInHierarchy) return;

        // ถ้าคอมโบหลุด ให้รีเซ็ต flag ว่ายิงพิเศษไปแล้วหรือยัง
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

        // เช็คจากสกิลแรงสุดไปอ่อนสุด (จะได้ไม่เผลอยิง Heavy ก่อน Signature)
        if (combo >= comboForSignatureShot && !signatureShotUsedThisCombo)
        {
            signatureShotUsedThisCombo = true;
            TriggerComboShot(target, ComboShotType.Signature);

            // ⭐ S Combo → บัดดี้ได้ EXP ชุดใหญ่
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

    /// <summary>
    /// ยิงสกิลคอมโบ (Heavy / Super / Signature) พร้อมอนิเมชัน + SFX + VFX
    /// </summary>
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

        // เล่นอนิเมชัน + เสียง
        PlayAnimatorStateIfValid(animState);
        PlaySfx(sfx);

        // Spawn VFX ตามประเภท
        SpawnAttackVfx(target, prefab, dmg, vfxTravelTime, logPrefix);
    }

    // ======================================================
    // FRIENDSHIP LOGIC + UI (เพิ่ม Exp + อัปเดต UI)
    // ======================================================

    /// <summary>
    /// เพิ่ม Exp ความสนิท (เรียกตอนฆ่าบอส, คอมโบใหญ่, เควสเสร็จ ฯลฯ)
    /// แล้วเช็คว่าขึ้นเลเวลหรือยัง + อัปเดต UI
    /// </summary>
    public void AddFriendshipExp(int amount)
    {
        // ถ้าเพื่อนตัวนี้ไม่ active (ปิดผ่าน SetActive() หรือ gameObject ปิดใน hierarchy)
        // จะไม่รับ EXP และไม่เลเวลอัป
        if (!isActive) return;
        if (!gameObject.activeInHierarchy) return;

        if (amount <= 0) return;
        if (friendshipLevel >= maxFriendshipLevel) return;

        currentFriendshipExp += amount;

        bool leveledUp = false;

        // ถ้า Exp เกิน ให้เลื่อนเลเวลขึ้นได้หลาย Lv ต่อเนื่องถ้าพอ
        while (currentFriendshipExp >= expPerLevel && friendshipLevel < maxFriendshipLevel)
        {
            currentFriendshipExp -= expPerLevel;
            friendshipLevel++;
            leveledUp = true;
        }

        if (leveledUp && showDebugLog)
        {
            Debug.Log($"[{friendName}] Friendship Level Up! Lv.{friendshipLevel}");
        }

        UpdateFriendshipUI();
    }

    /// <summary>
    /// อัปเดต UI ของความสนิท (ชื่อ, Lv, และ Slider Exp)
    /// </summary>
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

    /// <summary>
    /// เรียกตอนฆ่ามอน 1 ตัว เพื่อให้บัดดี้ได้ EXP
    /// (ให้ GameManager หรือ Monster เรียกฟังก์ชันนี้)
    /// </summary>
    public void OnMonsterKilled()
    {
        AddFriendshipExp(expOnMonsterKill);
    }

    // ======================================================
    // EMOTION HELPERS (ไว้เรียกจากเหตุการณ์ต่าง ๆ)
    // ======================================================

    /// <summary>
    /// ใช้ตอนผู้เล่นทำอะไรเท่ ๆ เช่น คอมโบต่อเนื่อง, ชนะบอส
    /// </summary>
    public void ReactHappy()
    {
        PlayAnimatorStateIfValid(happyStateName);
    }

    /// <summary>
    /// ใช้ตอน Boss เข้า Break แล้วให้เพื่อนบ้าพลัง
    /// </summary>
    public void EnterBerserkMode()
    {
        PlayAnimatorStateIfValid(berserkStateName);
    }
}
