using UnityEngine;
using System.Collections.Generic;

public class AutoClickUpgradeManager : MonoBehaviour
{
    [Header("References")]
    public AutoClicker autoClicker;
    
    [Header("Current Status")]
    [SerializeField] private int currentLevel = 0; // 0 = ยังไม่ปลดล็อค
    
    private Dictionary<int, AutoClickUpgradeLevel> upgradeLevels = new Dictionary<int, AutoClickUpgradeLevel>();
    
    void Start()
    {
        LoadUpgradesFromCSV();
        
        // ถ้า level > 0 = ปลดล็อคแล้ว
        if (currentLevel > 0)
        {
            ApplyUpgrade(currentLevel);
        }
    }
    
    void LoadUpgradesFromCSV()
    {
        TextAsset upgradeCSV = Resources.Load<TextAsset>("AutoClickUpgradeData");
        
        if (upgradeCSV == null)
        {
            Debug.LogError("ไม่พบไฟล์ AutoClickUpgradeData.csv ใน Resources folder!");
            return;
        }
        
        string[] lines = upgradeCSV.text.Split('\n');
        
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i].Trim())) continue;
            
            string[] values = lines[i].Split(',');
            
            AutoClickUpgradeLevel level = new AutoClickUpgradeLevel
            {
                level = int.Parse(values[0].Trim()),
                clicksPerSecond = float.Parse(values[1].Trim()),
                damageMultiplier = float.Parse(values[2].Trim()),
                cost = float.Parse(values[3].Trim())
            };
            
            upgradeLevels.Add(level.level, level);
        }
        
        Debug.Log($"โหลดข้อมูล Auto Click Upgrade {upgradeLevels.Count} เลเวลสำเร็จ");
    }
    
    public bool UpgradeAutoClick(float playerGold)
    {
        int nextLevel = currentLevel + 1;
        
        if (!upgradeLevels.ContainsKey(nextLevel))
        {
            Debug.Log("Auto Click ถึงเลเวลสูงสุดแล้ว!");
            return false;
        }
        
        AutoClickUpgradeLevel nextLevelData = upgradeLevels[nextLevel];
        
        if (playerGold >= nextLevelData.cost)
        {
            currentLevel++;
            ApplyUpgrade(currentLevel);
            
            // ถ้าเป็นการปลดล็อคครั้งแรก (level 1)
            if (currentLevel == 1 && autoClicker != null)
            {
                autoClicker.StartAutoClick();
                Debug.Log("🎉 ปลดล็อค Auto Click!");
            }
            
            Debug.Log($"=== Auto Click อัพเกรดสำเร็จ ===");
            Debug.Log($"Level: {currentLevel}");
            Debug.Log($"CPS: {nextLevelData.clicksPerSecond}");
            Debug.Log($"Damage: {nextLevelData.damageMultiplier * 100}%");
            
            return true;
        }
        else
        {
            Debug.Log($"เงินไม่พอ! ต้องการ {nextLevelData.cost} แต่มี {playerGold}");
            return false;
        }
    }
    
    private void ApplyUpgrade(int level)
    {
        if (!upgradeLevels.ContainsKey(level))
            return;
        
        AutoClickUpgradeLevel data = upgradeLevels[level];
        
        if (autoClicker != null)
        {
            autoClicker.UpdateAutoClickStats(data.clicksPerSecond, data.damageMultiplier);
            Debug.Log($"✅ Auto Click อัพเดทเป็น Level {level}");
        }
        else
        {
            Debug.LogError("❌ AutoClicker reference is null!");
        }
    }
    
    // Getters สำหรับ UI
    public int GetCurrentLevel() => currentLevel;
    
    public bool IsUnlocked() => currentLevel > 0;
    
    public float GetCurrentCPS()
    {
        if (currentLevel > 0 && upgradeLevels.ContainsKey(currentLevel))
            return upgradeLevels[currentLevel].clicksPerSecond;
        return 0f;
    }
    
    public float GetCurrentDamagePercent()
    {
        if (currentLevel > 0 && upgradeLevels.ContainsKey(currentLevel))
            return upgradeLevels[currentLevel].damageMultiplier * 100f;
        return 0f;
    }
    
    public float GetNextLevelCost()
    {
        if (upgradeLevels.ContainsKey(currentLevel + 1))
            return upgradeLevels[currentLevel + 1].cost;
        return -1;
    }
    
    public string GetNextLevelStats()
    {
        if (upgradeLevels.ContainsKey(currentLevel + 1))
        {
            AutoClickUpgradeLevel next = upgradeLevels[currentLevel + 1];
            return $"CPS: {next.clicksPerSecond} | Damage: {next.damageMultiplier * 100:F0}%";
        }
        return "MAX";
    }
    
    public string GetUnlockText()
    {
        if (currentLevel == 0 && upgradeLevels.ContainsKey(1))
        {
            AutoClickUpgradeLevel first = upgradeLevels[1];
            return $"Unlock Auto Click\nCPS: {first.clicksPerSecond} | Damage: {first.damageMultiplier * 100:F0}%";
        }
        return "Unlocked";
    }
}

[System.Serializable]
public class AutoClickUpgradeLevel
{
    public int level;
    public float clicksPerSecond;
    public float damageMultiplier;
    public float cost;
}
