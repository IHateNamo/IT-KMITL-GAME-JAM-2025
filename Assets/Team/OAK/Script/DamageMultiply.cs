using UnityEngine;

/// <summary>
/// แนบกับ Monster - จะเรียก UpgradeManager หลังจากโดนตี
/// </summary>
public class DamageCalculator : MonoBehaviour
{
    private UpgradeManager upgradeManager;
    private Monster monsterScript;
    
    private void Awake()
    {
        upgradeManager = FindFirstObjectByType<UpgradeManager>();
        monsterScript = GetComponent<Monster>();
        
        if (upgradeManager == null)
        {
            Debug.LogError("❌ DamageCalculator: ไม่พบ UpgradeManager!");
        }
    }
    
    private void OnEnable()
    {
        // Subscribe to damage event
        if (monsterScript != null)
        {
            // Hook into the update loop to detect damage
            InvokeRepeating(nameof(CheckDamage), 0f, 0.05f);
        }
    }
    
    private void OnDisable()
    {
        CancelInvoke(nameof(CheckDamage));
    }
    
    private float lastHealth = 0f;
    
    private void CheckDamage()
    {
        if (monsterScript == null) return;
        
        // ตรวจจับว่าถูกตีหรือยัง (เลือดลด)
        if (lastHealth > 0 && monsterScript.currentHealth < lastHealth)
        {
            // โดนตีแล้ว! แจ้ง UpgradeManager ให้สุ่มดาเมจใหม่
            if (upgradeManager != null)
            {
                upgradeManager.OnClickUsed();
            }
        }
        
        lastHealth = monsterScript.currentHealth;
    }
}
