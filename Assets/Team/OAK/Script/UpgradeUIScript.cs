using UnityEngine;
using UnityEngine.UI;
using TMPro; // สำหรับ TextMeshPro

public class UpgradeUI : MonoBehaviour
{
    [Header("References")]
    public UpgradeManager upgradeManager;
    
    [Header("UI Elements")]
    public Button upgradeButton;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI costText;
    
    [Header("Player Resources")]
    public float playerGold = 5000f; // เชื่อมกับระบบเงินของคุณ
    public TextMeshProUGUI goldText; // แสดงเงินที่มี (optional)
    
    void Start()
    {
        // เชื่อมปุ่มกับฟังก์ชัน
        upgradeButton.onClick.AddListener(OnUpgradeButtonClick);
    }
    
    void Update()
    {
        UpdateUI();
    }
    
    void UpdateUI()
    {
        if (upgradeManager == null) return;
        
        // อัพเดทข้อมูลปัจจุบัน
        levelText.text = $"Level: {upgradeManager.GetCurrentLevel()}";
        damageText.text = $"Damage: {upgradeManager.GetCurrentDamage():F0}";
        
        // อัพเดทราคาและสถานะปุ่ม
        float nextCost = upgradeManager.GetNextLevelCost();
        float nextDamage = upgradeManager.GetNextLevelDamage();
        
        if (nextCost >= 0)
        {
            costText.text = $"Upgrade\nCost: {nextCost:F0} Gold\nNext Damage: {nextDamage:F0}";
            
            // เปิด/ปิดปุ่มตามเงินที่มี
            upgradeButton.interactable = (playerGold >= nextCost);
        }
        else
        {
            costText.text = "MAX LEVEL";
            upgradeButton.interactable = false;
        }
        
        // แสดงเงินที่มี (optional)
        if (goldText != null)
        {
            goldText.text = $"Gold: {playerGold:F0}";
        }
    }
    
    public void OnUpgradeButtonClick()
    {
        float cost = upgradeManager.GetNextLevelCost();
        
        if (cost < 0)
        {
            Debug.Log("ถึง Max Level แล้ว!");
            return;
        }
        
        // พยายามอัพเกรด
        if (upgradeManager.UpgradeDamage(playerGold))
        {
            playerGold -= cost; // หักเงิน
            Debug.Log($"✅ อัพเกรดสำเร็จ! เหลือเงิน: {playerGold}");
            
            // เล่นเสียงหรือเอฟเฟกต์ (optional)
            // AudioManager.PlaySound("Upgrade");
        }
        else
        {
            Debug.Log("❌ ไม่สามารถอัพเกรดได้ (เงินไม่พอ)");
        }
    }
    
    // ฟังก์ชันเพิ่มเงิน (สำหรับทดสอบ)
    public void AddGold(float amount)
    {
        playerGold += amount;
    }
}
