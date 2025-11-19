using UnityEngine;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    [Header("References")]
    public ClickManager clickManager; // ต้องลาก ClickManager GameObject มาใส่
    
    [Header("Current Status")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private float currentClickDamage = 10f;
    
    private Dictionary<int, UpgradeLevel> upgradeLevels = new Dictionary<int, UpgradeLevel>();
    
    void Start()
    {
        LoadUpgradesFromCSV();
        UpdateClickManagerDamage(); // ตั้งค่าเริ่มต้น
    }
    
    // เพิ่มฟังก์ชัน Update เพื่อ sync ค่าตลอดเวลา
    void Update()
    {
        // ส่งค่าไปที่ ClickManager ทุกเฟรม (เพื่อให้แน่ใจว่าค่าถูกต้อง)
        UpdateClickManagerDamage();
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
        
        // ตั้งค่าเริ่มต้นจาก Level 1 ใน CSV
        if (upgradeLevels.ContainsKey(currentLevel))
        {
            currentClickDamage = upgradeLevels[currentLevel].clickDamage;
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
            currentClickDamage = upgradeLevels[currentLevel].clickDamage;
            
            // บังคับอัพเดททันที
            UpdateClickManagerDamage();
            
            Debug.Log($"=== อัพเกรดสำเร็จ ===");
            Debug.Log($"Level: {currentLevel}");
            Debug.Log($"Click Damage ใหม่: {currentClickDamage}");
            Debug.Log($"ClickManager.clickDamage ตอนนี้: {clickManager.clickDamage}");
            
            return true;
        }
        else
        {
            Debug.Log($"เงินไม่พอ! ต้องการ {nextLevel.cost} แต่มี {playerGold}");
            return false;
        }
    }
    
    // *** ฟังก์ชันหลักที่ส่งค่าไปที่ ClickManager ***
    private void UpdateClickManagerDamage()
    {
        if (clickManager != null)
        {
            // บังคับให้ ClickManager.clickDamage = ค่าจาก CSV
            clickManager.clickDamage = currentClickDamage;
        }
        else
        {
            Debug.LogError("⚠️ ClickManager ไม่ได้ถูกเชื่อมต่อ!");
            Debug.LogError("กรุณาลาก GameObject ที่มี ClickManager มาใส่ใน Inspector!");
        }
    }
    
    public int GetCurrentLevel() => currentLevel;
    public float GetCurrentDamage() => currentClickDamage;
    
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
