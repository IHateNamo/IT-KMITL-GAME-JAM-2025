using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    [Header("Managers")]
    public UpgradeManager upgradeManager;
    public UltUpgradeManager ultUpgradeManager; // ← เปลี่ยนชื่อ
    
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
    
    [Header("Player Resources")]
    public float playerGold = 5000f;
    public TextMeshProUGUI goldText;
    
    void Start()
    {
        upgradeButton.onClick.AddListener(OnUpgradeButtonClick);
        ultimateUpgradeButton.onClick.AddListener(OnUltimateUpgradeButtonClick);
    }
    
    void Update()
    {
        UpdateUI();
    }
    
    void UpdateUI()
    {
        // อัพเดท Click Damage UI
        if (upgradeManager != null)
        {
            levelText.text = $"ClickDMG Level: {upgradeManager.GetCurrentLevel()}";
            damageText.text = $"ClickDMG: {upgradeManager.GetCurrentDamage():F0}";
            
            float nextCost = upgradeManager.GetNextLevelCost();
            float nextDamage = upgradeManager.GetNextLevelDamage();
            
            if (nextCost >= 0)
            {
                costText.text = $"Upgrade\nCost: {nextCost:F0} Gold\nNext: {nextDamage:F0}";
                upgradeButton.interactable = (playerGold >= nextCost);
            }
            else
            {
                costText.text = "MAX LEVEL";
                upgradeButton.interactable = false;
            }
        }
        
        // อัพเดท Ultimate UI
        if (ultUpgradeManager != null) // ← เปลี่ยนชื่อ
        {
            ultimateLevelText.text = $"Ultimate Level: {ultUpgradeManager.GetCurrentLevel()}";
            
            ultimateStatsText.text = $"Duration: {ultUpgradeManager.GetCurrentDuration()}s\n" +
                                    $"DPS: {ultUpgradeManager.GetCurrentDPS()}% Max HP\n" +
                                    $"Final Hit: {ultUpgradeManager.GetCurrentFinalHit()}% Max HP\n" +
                                    $"Clicks To Ult: {ultUpgradeManager.GetCurrentClicksToUlt()}";

            float ultCost = ultUpgradeManager.GetNextLevelCost();
            
            if (ultCost >= 0)
            {
                ultimateCostText.text = $"Upgrade Ultimate\nCost: {ultCost:F0} Gold\n{ultUpgradeManager.GetNextLevelStats()}";
                ultimateUpgradeButton.interactable = (playerGold >= ultCost);
            }
            else
            {
                ultimateCostText.text = "MAX LEVEL";
                ultimateUpgradeButton.interactable = false;
            }
        }
        
        // แสดงเงิน
        if (goldText != null)
        {
            goldText.text = $"Gold: {playerGold:F0}";
        }
    }
    
    public void OnUpgradeButtonClick()
    {
        float cost = upgradeManager.GetNextLevelCost();
        
        if (cost < 0) return;
        
        if (upgradeManager.UpgradeDamage(playerGold))
        {
            playerGold -= cost;
            Debug.Log($"✅ อัพเกรด Click Damage สำเร็จ! เหลือเงิน: {playerGold}");
        }
    }
    
    public void OnUltimateUpgradeButtonClick()
    {
        float cost = ultUpgradeManager.GetNextLevelCost(); // ← เปลี่ยนชื่อ
        
        if (cost < 0)
        {
            Debug.Log("Ultimate ถึง Max Level แล้ว!");
            return;
        }
        
        if (ultUpgradeManager.UpgradeUltimate(playerGold)) // ← เปลี่ยนชื่อ
        {
            playerGold -= cost;
            Debug.Log($"✅ อัพเกรด Ultimate สำเร็จ! เหลือเงิน: {playerGold}");
        }
    }
    
    public void AddGold(float amount)
    {
        playerGold += amount;
    }
}
