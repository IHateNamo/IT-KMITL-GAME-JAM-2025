using UnityEngine;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    [Header("References")]
    public ClickManager clickManager;
    
    [Header("Current Status")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private float baseClickDamage = 10f;
    
    [Header("Damage Variance")]
    [Tooltip("ดาเมจต่ำสุด (1.0 = 100%)")]
    [SerializeField] private float minDamageMultiplier = 1.0f;
    
    [Tooltip("ดาเมจสูงสุด (2.0 = 200%)")]
    [SerializeField] private float maxDamageMultiplier = 2.0f;
    
    [Tooltip("เปิด/ปิดระบบดาเมจสุ่ม")]
    [SerializeField] private bool enableDamageVariance = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDamageLog = true;
    
    private Dictionary<int, UpgradeLevel> upgradeLevels = new Dictionary<int, UpgradeLevel>();
    private float nextClickDamage = 10f; // เก็บดาเมจที่จะใช้ในครั้งถัดไป
    
    void Start()
    {
        LoadUpgradesFromCSV();
        PrepareNextClickDamage(); // สุ่มดาเมจครั้งแรก
    }
    
    void LoadUpgradesFromCSV()
    {
        TextAsset upgradeCSV = Resources.Load<TextAsset>("UpgradeData");
        
        if (upgradeCSV == null)
        {
            Debug.LogError("ไม่พบไฟล์ UpgradeData.csv ใน Resources folder!");
            return;
        }
        
        string[] lines = upgradeCSV.text.Split('\n');
        
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i].Trim())) continue;
            
            string[] values = lines[i].Split(',');
            
            UpgradeLevel level = new UpgradeLevel
            {
                level = int.Parse(values[0].Trim()),
                clickDamage = float.Parse(values[1].Trim()),
                cost = float.Parse(values[2].Trim())
            };
            
            upgradeLevels.Add(level.level, level);
        }
        
        if (upgradeLevels.ContainsKey(currentLevel))
        {
            baseClickDamage = upgradeLevels[currentLevel].clickDamage;
        }
        
        Debug.Log($"โหลดข้อมูลอัพเกรด {upgradeLevels.Count} เลเวลสำเร็จ");
    }
    
    public bool UpgradeDamage(float playerGold)
    {
        if (!upgradeLevels.ContainsKey(currentLevel + 1))
        {
            Debug.Log("ถึงเลเวลสูงสุดแล้ว!");
            return false;
        }
        
        UpgradeLevel nextLevel = upgradeLevels[currentLevel + 1];
        
        if (playerGold >= nextLevel.cost)
        {
            currentLevel++;
            baseClickDamage = upgradeLevels[currentLevel].clickDamage;
            PrepareNextClickDamage(); // สุ่มดาเมจใหม่หลังอัพเกรด
            
            Debug.Log($"=== อัพเกรดสำเร็จ ===");
            Debug.Log($"Level: {currentLevel}");
            Debug.Log($"Base Damage: {baseClickDamage}");
            Debug.Log($"Damage Range: {GetMinDamage():F1} - {GetMaxDamage():F1}");
            
            return true;
        }
        else
        {
            Debug.Log($"เงินไม่พอ! ต้องการ {nextLevel.cost} แต่มี {playerGold}");
            return false;
        }
    }
    
    // *** สร้างดาเมจสำหรับการคลิกครั้งถัดไป ***
    private void PrepareNextClickDamage()
    {
        if (enableDamageVariance)
        {
            float randomMultiplier = Random.Range(minDamageMultiplier, maxDamageMultiplier);
            nextClickDamage = baseClickDamage * randomMultiplier;
        }
        else
        {
            nextClickDamage = baseClickDamage;
        }
        
        // อัพเดทให้ ClickManager
        if (clickManager != null)
        {
            clickManager.clickDamage = nextClickDamage;
        }
    }
    
    // *** ฟังก์ชันให้ตัวอื่นเรียกหลังจากคลิก เพื่อสุ่มดาเมจครั้งใหม่ ***
    public void OnClickUsed()
    {
        if (showDamageLog)
        {
            Debug.Log($"🎲 Damage Used: {nextClickDamage:F1}");
        }
        
        // สุ่มดาเมจใหม่สำหรับการคลิกครั้งถัดไป
        PrepareNextClickDamage();
    }
    
    public int GetCurrentLevel() => currentLevel;
    public float GetCurrentDamage() => baseClickDamage;
    
    public float GetMinDamage() => baseClickDamage * minDamageMultiplier;
    public float GetMaxDamage() => baseClickDamage * maxDamageMultiplier;
    
    public string GetDamageRangeText()
    {
        if (enableDamageVariance)
        {
            return $"{GetMinDamage():F0} - {GetMaxDamage():F0}";
        }
        return $"{baseClickDamage:F0}";
    }
    
    public float GetNextLevelCost()
    {
        if (upgradeLevels.ContainsKey(currentLevel + 1))
            return upgradeLevels[currentLevel + 1].cost;
        return -1;
    }
    
    public float GetNextLevelDamage()
    {
        if (upgradeLevels.ContainsKey(currentLevel + 1))
            return upgradeLevels[currentLevel + 1].clickDamage;
        return -1;
    }
}

[System.Serializable]
public class UpgradeLevel
{
    public int level;
    public float clickDamage;
    public float cost;
}
