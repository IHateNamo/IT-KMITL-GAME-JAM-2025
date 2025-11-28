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

    // ---- NEW: minimum HP% for companion to keep attacking ----
    [Header("Attack Limit")]
    [Tooltip("หยุดโจมตีเมื่อ HP ของมอนสเตอร์ต่ำกว่าค่านี้ (เช่น 0.01 = 1%)")]
    [Range(0f, 1f)]
    public float minHpPercentToAttack = 0.01f;

    private float attackInterval;
    private float nextAttackTime;

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

        if (attackVfxPrefab != null)
        {
            Vector3 spawnPos = transform.position;
            if (vfxSpawnPoint != null)
                spawnPos = vfxSpawnPoint.position;

            CompanionAttackVFX vfx = Instantiate(attackVfxPrefab, spawnPos, Quaternion.identity);
            vfx.Initialize(target, damage, vfxTravelTime);

            if (showDebugLog)
            {
                Debug.Log("Companion: Spawn VFX -> target " + target.name + ", dmg " + damage.ToString("F1") + ", Lv." + level);
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
                    Debug.Log("Companion: Direct BYPASS dmg " + damage.ToString("F1") + " (no VFX) Lv." + level);
                }
            }
            else
            {
                target.TakeDamage(damage);
                if (showDebugLog)
                {
                    Debug.LogWarning("Companion: Direct TakeDamage " + damage.ToString("F1") + " (no VFX, no bypass) Lv." + level);
                }
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
}
