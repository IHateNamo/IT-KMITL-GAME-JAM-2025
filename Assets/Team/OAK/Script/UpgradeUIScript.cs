using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    [Header("Managers")]
    public UpgradeManager upgradeManager;
    public UltUpgradeManager ultUpgradeManager;
    public AutoClickUpgradeManager autoClickUpgradeManager;
    
    [Header("Click Damage UI")]
    public Button upgradeButton;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI costText;
    
    [Header("Ultimate Skill UI")]
    public Button ultimateUpgradeButton;
    public TextMeshProUGUI ultimateLevelText;
    public TextMeshProUGUI ultimateStatsText;
    public TextMeshProUGUI ultimateCostText;
    
    [Header("Auto Click UI")]
    public Button autoClickUpgradeButton;
    public TextMeshProUGUI autoClickLevelText;
    public TextMeshProUGUI autoClickStatsText;
    public TextMeshProUGUI autoClickCostText;
    
    [Header("Player Resources")]
    public float playerGold = 5000f;
    public TextMeshProUGUI goldText;
    
    void Start()
    {
        // เชื่อมปุ่มกับฟังก์ชัน - ต้องมีฟังก์ชันเหล่านี้
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeButtonClick);
        
        if (ultimateUpgradeButton != null)
            ultimateUpgradeButton.onClick.AddListener(OnUltimateUpgradeButtonClick);
        
        if (autoClickUpgradeButton != null)
            autoClickUpgradeButton.onClick.AddListener(OnAutoClickUpgradeButtonClick);
    }
    
    void Update()
    {
        UpdateUI();
    }
    
    void UpdateUI()
    {
        // Click Damage UI
        if (upgradeManager != null)
        {
            if (levelText != null)
                levelText.text = $"ClickDMG Level: {upgradeManager.GetCurrentLevel()}";
            
            if (damageText != null)
                damageText.text = $"ClickDMG: {upgradeManager.GetCurrentDamage():F0}";
            
            float nextCost = upgradeManager.GetNextLevelCost();
            float nextDamage = upgradeManager.GetNextLevelDamage();
            
            if (costText != null)
            {
                if (nextCost >= 0)
                {
                    costText.text = $"Upgrade\nCost: {nextCost:F0} Gold\nNext: {nextDamage:F0}";
                    if (upgradeButton != null)
                        upgradeButton.interactable = (playerGold >= nextCost);
                }
                else
                {
                    costText.text = "MAX LEVEL";
                    if (upgradeButton != null)
                        upgradeButton.interactable = false;
                }
            }
        }
        
        // Ultimate UI
        if (ultUpgradeManager != null)
        {
            if (ultimateLevelText != null)
                ultimateLevelText.text = $"Ultimate Level: {ultUpgradeManager.GetCurrentLevel()}";
            
            if (ultimateStatsText != null)
            {
                ultimateStatsText.text = $"Duration: {ultUpgradeManager.GetCurrentDuration()}s\n" +
                                        $"DPS: {ultUpgradeManager.GetCurrentDPS()}%\n" +
                                        $"Final Hit: {ultUpgradeManager.GetCurrentFinalHit()}%\n" +
                                        $"Clicks To Ult: {ultUpgradeManager.GetCurrentClicksToUlt()}";
            }
            
            float ultCost = ultUpgradeManager.GetNextLevelCost();
            
            if (ultimateCostText != null)
            {
                if (ultCost >= 0)
                {
                    ultimateCostText.text = $"Upgrade\nCost: {ultCost:F0} Gold\n{ultUpgradeManager.GetNextLevelStats()}";
                    if (ultimateUpgradeButton != null)
                        ultimateUpgradeButton.interactable = (playerGold >= ultCost);
                }
                else
                {
                    ultimateCostText.text = "MAX LEVEL";
                    if (ultimateUpgradeButton != null)
                        ultimateUpgradeButton.interactable = false;
                }
            }
        }
        
        // Auto Click UI
        if (autoClickUpgradeManager != null)
        {
            if (autoClickLevelText != null)
            {
                if (autoClickUpgradeManager.IsUnlocked())
                {
                    autoClickLevelText.text = $"Auto Click Level: {autoClickUpgradeManager.GetCurrentLevel()}";
                }
                else
                {
                    autoClickLevelText.text = "Auto Click: LOCKED";
                }
            }
            
            if (autoClickStatsText != null)
            {
                if (autoClickUpgradeManager.IsUnlocked())
                {
                    autoClickStatsText.text = $"CPS: {autoClickUpgradeManager.GetCurrentCPS()}\n" +
                                             $"Damage: {autoClickUpgradeManager.GetCurrentDamagePercent():F0}%";
                }
                else
                {
                    autoClickStatsText.text = autoClickUpgradeManager.GetUnlockText();
                }
            }
            
            float autoClickCost = autoClickUpgradeManager.GetNextLevelCost();
            
            if (autoClickCostText != null)
            {
                if (autoClickCost >= 0)
                {
                    autoClickCostText.text = $"Upgrade\nCost: {autoClickCost:F0} Gold\n{autoClickUpgradeManager.GetNextLevelStats()}";
                    if (autoClickUpgradeButton != null)
                        autoClickUpgradeButton.interactable = (playerGold >= autoClickCost);
                }
                else
                {
                    autoClickCostText.text = "MAX LEVEL";
                    if (autoClickUpgradeButton != null)
                        autoClickUpgradeButton.interactable = false;
                }
            }
        }
        
        // แสดงเงิน
        if (goldText != null)
        {
            goldText.text = $"Gold: {playerGold:F0}";
        }
    }
    
    // ← ฟังก์ชันที่ Error บอกว่าหายไป
    public void OnUpgradeButtonClick()
    {
        if (upgradeManager == null) return;
        
        float cost = upgradeManager.GetNextLevelCost();
        
        if (cost < 0)
        {
            Debug.Log("ถึง Max Level แล้ว!");
            return;
        }
        
        if (upgradeManager.UpgradeDamage(playerGold))
        {
            playerGold -= cost;
            Debug.Log($"✅ อัพเกรด Click Damage สำเร็จ! เหลือเงิน: {playerGold}");
        }
    }
    
    public void OnUltimateUpgradeButtonClick()
    {
        if (ultUpgradeManager == null) return;
        
        float cost = ultUpgradeManager.GetNextLevelCost();
        
        if (cost < 0)
        {
            Debug.Log("Ultimate ถึง Max Level แล้ว!");
            return;
        }
        
        if (ultUpgradeManager.UpgradeUltimate(playerGold))
        {
            playerGold -= cost;
            Debug.Log($"✅ อัพเกรด Ultimate สำเร็จ! เหลือเงิน: {playerGold}");
        }
    }
    
    public void OnAutoClickUpgradeButtonClick()
    {
        if (autoClickUpgradeManager == null) return;
        
        float cost = autoClickUpgradeManager.GetNextLevelCost();
        
        if (cost < 0)
        {
            Debug.Log("Auto Click ถึง Max Level แล้ว!");
            return;
        }
        
        if (autoClickUpgradeManager.UpgradeAutoClick(playerGold))
        {
            playerGold -= cost;
            Debug.Log($"✅ อัพเกรด Auto Click สำเร็จ! เหลือเงิน: {playerGold}");
        }
    }
    
    public void AddGold(float amount)
    {
        playerGold += amount;
    }
}
