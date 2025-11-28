using System.Collections;
using UnityEngine;

/// <summary>
/// เพื่อนร่วมสู้ (Companion) ที่ยิงมอนสเตอร์ให้อัตโนมัติ
/// ใช้ GameManager.activeMonster เป็นเป้าหมายหลัก
/// สร้าง VFX ให้บินไปหาเป้าหมายแล้วทำดาเมจ
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

    [Header("VFX Settings")]
    [Tooltip("Prefab ของ VFX ที่ใช้ตอน Companion โจมตี (ต้องมี CompanionAttackVFX)")]
    public CompanionAttackVFX attackVfxPrefab;

    [Tooltip("ตำแหน่งที่ใช้ spawn VFX (ถ้าเว้นว่างจะใช้ตำแหน่งของ Companion)")]
    public Transform vfxSpawnPoint;

    [Tooltip("เวลาที่ VFX ใช้บินไปหาเป้าหมาย (วินาที)")]
    public float vfxTravelTime = 0.15f;

    [Header("Runtime State")]
    [SerializeField] private bool isActive = true;
    [SerializeField] private bool showDebugLog = true;

    [Header("References")]
    [Tooltip("ใช้สำหรับอ่านดาเมจปัจจุบันของผู้เล่น")]
    public UpgradeManager upgradeManager;

    [Tooltip("GameManager ที่ใช้จัดการ activeMonster")]
    public GameManager gameManager;

    [Tooltip("แอนิเมเตอร์ของ Companion (ไว้เล่นอนิเมชัน Idle / Attack)")]
    public Animator animator;

    [Tooltip("ชื่อ Trigger หรือ State ของอนิเมชันโจมตี")]
    public string attackTriggerName = "Attack";

    [Tooltip("ชื่อ State Idle ใน Animator (ถ้าอยากบังคับกลับไป Idle)")]
    public string idleStateName = "Idle";

    private float attackInterval;
    private float nextAttackTime;

    private void Awake()
    {
        // Auto-find UpgradeManager ถ้าไม่เซ็ตใน Inspector
        if (upgradeManager == null)
        {
            upgradeManager = FindFirstObjectByType<UpgradeManager>();
            if (upgradeManager == null)
            {
                Debug.LogWarning("Companion: ไม่พบ UpgradeManager ในซีน");
            }
        }

        // Auto-find GameManager ถ้าไม่เซ็ต
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogWarning("Companion: ไม่พบ GameManager ในซีน");
            }
        }

        RecalculateAttackInterval();
    }

    private void OnEnable()
    {
        // รีเซ็ตเวลาโจมตีเมื่อ Companion ถูกเปิดใช้งาน
        nextAttackTime = Time.time;
    }

    private void Update()
    {
        if (!isActive) return;
        if (gameManager == null) return;

        Monster target = gameManager.activeMonster;
        if (target == null || target.currentHealth <= 0f)
            return;

        if (Time.time >= nextAttackTime)
        {
            PerformAttack(target);
            nextAttackTime = Time.time + attackInterval;
        }
    }

    #region Attack & Damage

    /// <summary>
    /// คำนวณดาเมจของ Companion ตามเลเวล และดาเมจของผู้เล่น (UpgradeManager)
    /// </summary>
    private float CalculateDamage()
    {
        float playerDamage = upgradeManager != null ? upgradeManager.GetCurrentDamage() : 1f;

        float levelBonus = damageMultiplierPerLevel * (level - 1);
        float finalMultiplier = baseDamageMultiplier + levelBonus;

        if (finalMultiplier < 0f)
            finalMultiplier = 0f;

        float damage = playerDamage * finalMultiplier;
        return damage;
    }

    /// <summary>
    /// คำนวณความเร็วโจมตีตามเลเวล แล้วกลับเป็น interval
    /// </summary>
    private void RecalculateAttackInterval()
    {
        float bonusPercent = attackSpeedPercentPerLevel * (level - 1); // 0.1 => +10% ต่อเลเวล
        float speedMultiplier = 1f + bonusPercent;

        float finalAPS = Mathf.Max(0.1f, baseAttacksPerSecond * speedMultiplier);
        attackInterval = 1f / finalAPS;
    }

    private void PerformAttack(Monster target)
    {
        if (target == null || target.currentHealth <= 0f)
            return;

        float damage = CalculateDamage();

        // เล่นอนิเมชัน Companion ก่อน
        PlayAttackAnimation();

        // ถ้ามี VFX prefab ให้ VFX เป็นคนทำดาเมจแทน
        if (attackVfxPrefab != null)
        {
            Vector3 spawnPos = vfxSpawnPoint != null ? vfxSpawnPoint.position : transform.position;
            CompanionAttackVFX vfx = Instantiate(attackVfxPrefab, spawnPos, Quaternion.identity);

            // ส่งเป้าหมาย + ดาเมจ + เวลาเดินทางไปให้ VFX จัดการเอง
            vfx.Initialize(target, damage, vfxTravelTime);

            if (showDebugLog)
            {
                Debug.Log($"🧭 Companion: Spawn VFX -> target {target.name}, dmg {damage:F1}, Lv.{level}");
            }
        }
        else
        {
            // Fallback: ถ้าไม่มี VFX ก็ยิงดาเมจตรง ๆ
            var bypass = target.GetComponent<MonsterDamageBypass>();
            if (bypass != null)
            {
                bypass.ApplyDirectDamage(damage);
                if (showDebugLog)
                    Debug.Log($"🧭 Companion: Direct BYPASS dmg {damage:F1} (no VFX) Lv.{level}");
            }
            else
            {
                target.TakeDamage(damage);
                if (showDebugLog)
                    Debug.LogWarning($"Companion: Direct TakeDamage {damage:F1} (no VFX, no bypass) Lv.{level}");
            }
        }
    }

    #endregion

    #region Animation

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
            Debug.Log($"✨ Companion Upgrade => Lv.{level}, " +
                      $"Damage Multiplier Now ≈ {baseDamageMultiplier + damageMultiplierPerLevel * (level - 1):F2}");
        }
    }

    #endregion

    #region Public Controls

    public void SetActive(bool active)
    {
        isActive = active;

        if (showDebugLog)
        {
            Debug.Log($"Companion: Active = {isActive}");
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
}
