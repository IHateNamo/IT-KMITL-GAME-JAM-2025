using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterDamageBypass : MonoBehaviour
{
    private Monster monster;
    
    [Header("UI References (Auto-Wired)")]
    public Slider healthBar;
    public TextMeshProUGUI hpText;

    protected GameManager gameManager;
    protected Vector3 originalScale;
    
    private void Awake()
    {
        monster = GetComponent<Monster>();
        
        if (monster == null)
        {
            Debug.LogError("MonsterDamageBypass: ไม่พบ Monster component!");
            return;
        }
        
        gameManager = FindFirstObjectByType<GameManager>();
        originalScale = transform.localScale;

        // Auto-wire UI
        if (hpText == null)
        {
            GameObject textObj = GameObject.Find("HPText");
            if (textObj != null) 
                hpText = textObj.GetComponent<TextMeshProUGUI>();
            else 
                Debug.LogWarning("MonsterDamageBypass: ไม่พบ 'HPText'");
        }

        if (healthBar == null)
        {
            GameObject sliderObj = GameObject.Find("HPBar");
            if (sliderObj == null) sliderObj = GameObject.Find("Slider");

            if (sliderObj != null) 
                healthBar = sliderObj.GetComponent<Slider>();
            else
                Debug.LogWarning("MonsterDamageBypass: ไม่พบ 'HPBar' หรือ 'Slider'");
        }
    }
    
    /// <summary>
    /// อัพเดท UI ใช้ค่าจาก Monster component
    /// </summary>
    protected virtual void UpdateUI()
    {
        if (monster == null) return;
        
        if (healthBar != null)
        {
            healthBar.maxValue = monster.maxHealth; // ← ใช้จาก monster
            healthBar.value = monster.currentHealth; // ← ใช้จาก monster
        }

        if (hpText != null)
        {
            hpText.text = $"{Mathf.Ceil(monster.currentHealth)} / {Mathf.Ceil(monster.maxHealth)}"; // ← ใช้จาก monster
        }
    }

    /// <summary>
    /// ทำดาเมจโดยไม่นับคอมโบ
    /// </summary>
    public void ApplyDirectDamage(float amount)
    {
        if (monster == null) return;
        
        // แก้ HP ของ Monster component
        monster.currentHealth -= amount;
        
        // อัพเดท UI ทันที
        UpdateUI();
        
        // เช็คว่าตายหรือยัง
        if (monster.currentHealth <= 0f)
        {
            monster.currentHealth = 0f;
            UpdateUI(); // อัพเดทอีกครั้งเพื่อแสดง 0
            gameObject.SetActive(false);
        }
    }
}
