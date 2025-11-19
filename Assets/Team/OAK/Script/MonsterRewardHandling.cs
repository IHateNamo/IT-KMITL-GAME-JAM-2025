using UnityEngine;

public class MonsterRewardHandler : MonoBehaviour
{
    [Header("References")]
    [Tooltip("ลาก GameObject ที่มี UpgradeUI script มาใส่")]
    [SerializeField] private UpgradeUI upgradeUI;
    
    [Header("Scaled Method Settings")]
    [SerializeField] private float scalingMultiplier = 2f;
    [SerializeField] private float scalingPower = 0.75f;
    
    [Header("Debug")]
    [SerializeField] private bool showCalculationLog = true;
    
    private Monster monsterScript;
    private bool hasGivenReward = false; // ป้องกันให้เงินซ้ำ
    
    private void Awake()
    {
        monsterScript = GetComponent<Monster>();
        
        // ถ้าไม่ได้ลากใส่ใน Inspector ก็ค้นหาอัตโนมัติ
        if (upgradeUI == null)
        {
            upgradeUI = FindFirstObjectByType<UpgradeUI>();
        }
        
        if (monsterScript == null)
        {
            Debug.LogError("❌ ไม่พบ Monster script!");
        }
        else
        {
            Debug.Log($"✅ พบ Monster - Max HP = {monsterScript.maxHealth}");
        }
        
        if (upgradeUI == null)
        {
            Debug.LogError("❌ ไม่พบ UpgradeUI!");
        }
        else
        {
            Debug.Log($"✅ เชื่อมต่อ UpgradeUI สำเร็จ (GameObject: {upgradeUI.gameObject.name})");
        }
    }
    
    // ถูกเรียกเมื่อ GameObject ถูก SetActive(true)
    private void OnEnable()
    {
        hasGivenReward = false; // รีเซ็ตสถานะเมื่อมอนสเตอร์ spawn ใหม่
        Debug.Log("🔄 มอนสเตอร์ spawn ใหม่ - รีเซ็ตสถานะรางวัล");
    }
    
    // ถูกเรียกเมื่อ GameObject ถูก SetActive(false) - ตรงนี้คือจุดสำคัญ!
    private void OnDisable()
    {
        Debug.Log("⚠️ OnDisable() ถูกเรียก!");
        
        // ตรวจสอบว่ามอนสเตอร์ตายจริง (HP = 0) หรือแค่ถูก disable
        if (monsterScript != null && monsterScript.currentHealth <= 0f && !hasGivenReward)
        {
            Debug.Log("🎯 ยืนยัน: มอนสเตอร์ตาย (HP = 0)!");
            GiveReward();
            hasGivenReward = true;
        }
        else
        {
            Debug.Log($"ℹ️ OnDisable แต่ไม่ใช่การตาย (HP: {monsterScript?.currentHealth}, ให้รางวัลแล้ว: {hasGivenReward})");
        }
    }
    
    private void GiveReward()
    {
        if (upgradeUI == null)
        {
            Debug.LogError("❌ ไม่สามารถให้เงินได้: UpgradeUI = null");
            return;
        }
        
        if (monsterScript == null)
        {
            Debug.LogError("❌ ไม่สามารถให้เงินได้: Monster = null");
            return;
        }
        
        float maxHP = monsterScript.maxHealth;
        float calculatedGold = Mathf.Pow(maxHP, scalingPower) * scalingMultiplier;
        calculatedGold = Mathf.Round(calculatedGold);
        
        Debug.Log($"💰 คำนวณเงิน: HP {maxHP} → {calculatedGold} Gold");
        
        upgradeUI.AddGold(calculatedGold);
        
        Debug.Log($"✅ ให้เงินสำเร็จ! เงินปัจจุบัน = {upgradeUI.playerGold}");
    }
}
