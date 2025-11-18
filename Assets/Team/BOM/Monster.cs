using UnityEngine;
using UnityEngine.UI; // สำหรับ Slider
using TMPro; // สำหรับ TextMeshPro

public class Monster : MonoBehaviour
{
    [Header("Settings")]
    public float maxHealth = 100f; // เลือดเต็ม
    public float currentHealth;

    [Header("UI References")]
    public Slider healthBar;      // ลาก Slider มาใส่
    public TextMeshProUGUI hpText; // ลาก TextMeshPro มาใส่

    // เก็บ Reference ของ GameManager
    private GameManager gameManager;

    void Start()
    {
        // หา GameManager ในฉากเตรียมไว้
        gameManager = FindFirstObjectByType<GameManager>();
        
        // รีเซ็ตเลือดตอนเกิด
        ResetMonster();
    }

    public void ResetMonster()
    {
        currentHealth = maxHealth;
        UpdateUI();
        gameObject.SetActive(true); // เปิดตัวมอนสเตอร์
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        // เอฟเฟกต์ตัวเด้งนิดนึงตอนโดนตี (Juice)
        transform.localScale = Vector3.one * 0.9f; 
        Invoke("ResetScale", 0.1f);

        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void ResetScale()
    {
        transform.localScale = Vector3.one;
    }

    void UpdateUI()
    {
        // อัปเดตหลอดเลือด
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        // อัปเดตตัวเลข
        if (hpText != null)
        {
            hpText.text = $"{Mathf.Ceil(currentHealth)} / {maxHealth}";
        }
    }

    void Die()
    {
        // แจ้ง GameManager ว่ามอนสเตอร์ตายแล้ว
        gameManager.OnMonsterDied();
        gameObject.SetActive(false); // ซ่อนมอนสเตอร์ชั่วคราว
    }
}