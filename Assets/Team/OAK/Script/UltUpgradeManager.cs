using UnityEngine;
using System.Collections.Generic;

public class UltUpgradeManager : MonoBehaviour
{
    [Header("References")]
    public UltimateSkill ultimateSkill;
    public UltimateProgression ultimateProgression;
    
    [Header("Current Status")]
    [SerializeField] private int currentLevel = 1;
    
    private Dictionary<int, UltUpgradeLevel> upgradeLevels = new Dictionary<int, UltUpgradeLevel>();
    
    void Start()
    {
        LoadUpgradesFromCSV();
        ApplyUpgrade(currentLevel);
    }
    
    void LoadUpgradesFromCSV()
    {
        TextAsset upgradeCSV = Resources.Load<TextAsset>("UltimateUpgradeData");
        
        if (upgradeCSV == null)
        {
            Debug.LogError("ไม่พบไฟล์ UltimateUpgradeData.csv ใน Resources folder!");
            return;
        }
        
        string[] lines = upgradeCSV.text.Split('\n');
        
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i].Trim())) continue;
            
            string[] values = lines[i].Split(',');
            
            UltUpgradeLevel level = new UltUpgradeLevel
            {
                level = int.Parse(values[0].Trim()),
                ultDuration = float.Parse(values[1].Trim()),
                damagePercentOfMaxHP = float.Parse(values[2].Trim()),
                finalHitPercentOfMaxHP = float.Parse(values[3].Trim()),
                clicksToUlt = int.Parse(values[4].Trim()),
                cost = float.Parse(values[5].Trim())
            };
            
            upgradeLevels.Add(level.level, level);
        }
        
        Debug.Log($"โหลดข้อมูลอัพเกรด Ultimate {upgradeLevels.Count} เลเวลสำเร็จ");
    }
    
    public bool UpgradeUltimate(float playerGold)
    {
        if (!upgradeLevels.ContainsKey(currentLevel + 1))
        {
            Debug.Log("Ultimate ถึงเลเวลสูงสุดแล้ว!");
            return false;
        }
        
        UltUpgradeLevel nextLevel = upgradeLevels[currentLevel + 1];
        
        if (playerGold >= nextLevel.cost)
        {
            currentLevel++;
            ApplyUpgrade(currentLevel);
            
            Debug.Log($"=== Ultimate อัพเกรดสำเร็จ ===");
            Debug.Log($"Level: {currentLevel}");
            Debug.Log($"Duration: {nextLevel.ultDuration}s");
            Debug.Log($"DPS: {nextLevel.damagePercentOfMaxHP * 100}%");
            Debug.Log($"Final Hit: {nextLevel.finalHitPercentOfMaxHP * 100}%");
            Debug.Log($"Clicks To Ult: {nextLevel.clicksToUlt}");
            
            return true;
        }
        else
        {
            Debug.Log($"เงินไม่พอ! ต้องการ {nextLevel.cost} แต่มี {playerGold}");
            return false;
        }
    }
    
    private void ApplyUpgrade(int level)
    {
        if (!upgradeLevels.ContainsKey(level))
            return;
        
        UltUpgradeLevel data = upgradeLevels[level];
        
        // อัพเดท UltimateSkill
        if (ultimateSkill != null)
        {
            ultimateSkill.ultDuration = data.ultDuration;
            ultimateSkill.damagePercentOfMaxHP = data.damagePercentOfMaxHP;
            ultimateSkill.finalHitPercentOfMaxHP = data.finalHitPercentOfMaxHP;
            Debug.Log($"✅ อัพเดท UltimateSkill สำเร็จ");
        }
        else
        {
            Debug.LogError("❌ UltimateSkill reference is null!");
        }
        
        // อัพเดท UltimateProgression (ใช้ clicksToUlt)
        if (ultimateProgression != null)
        {
            ultimateProgression.clicksToUlt = data.clicksToUlt; // ← แก้ตรงนี้
            Debug.Log($"✅ อัพเดท Clicks To Ult = {data.clicksToUlt}");
        }
        else
        {
            Debug.LogError("❌ UltimateProgression reference is null!");
        }
        
        Debug.Log($"✅ Ultimate อัพเดทเป็น Level {level} สำเร็จ");
    }
    
    // Getters สำหรับ UI
    public int GetCurrentLevel()
    {
        return currentLevel;
    }
    
    public float GetCurrentDuration()
    {
        if (upgradeLevels.ContainsKey(currentLevel))
            return upgradeLevels[currentLevel].ultDuration;
        return 0f;
    }
    
    public float GetCurrentDPS()
    {
        if (upgradeLevels.ContainsKey(currentLevel))
            return upgradeLevels[currentLevel].damagePercentOfMaxHP * 100f;
        return 0f;
    }
    
    public float GetCurrentFinalHit()
    {
        if (upgradeLevels.ContainsKey(currentLevel))
            return upgradeLevels[currentLevel].finalHitPercentOfMaxHP * 100f;
        return 0f;
    }
    
    public int GetCurrentClicksToUlt()
    {
        if (upgradeLevels.ContainsKey(currentLevel))
            return upgradeLevels[currentLevel].clicksToUlt;
        return 999;
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
            UltUpgradeLevel next = upgradeLevels[currentLevel + 1];
            return $"Dur: {next.ultDuration}s | DPS: {next.damagePercentOfMaxHP * 100f:F1}% | Final: {next.finalHitPercentOfMaxHP * 100f:F0}% | Clicks: {next.clicksToUlt}";
        }
        return "MAX";
    }
}

[System.Serializable]
public class UltUpgradeLevel
{
    public int level;
    public float ultDuration;
    public float damagePercentOfMaxHP;
    public float finalHitPercentOfMaxHP;
    public int clicksToUlt;
    public float cost;
}
