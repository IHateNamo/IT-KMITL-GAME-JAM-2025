using UnityEngine;
using UnityEngine.UI;   // สำหรับ Slider
using TMPro;           // สำหรับ TextMeshPro
using System.Collections;

public class Monster : MonoBehaviour
{
    [Header("Settings")]
    public float maxHealth = 100f; // เลือดเต็ม
    public float currentHealth;

    [Header("UI References")]
    public Slider healthBar;        // ลาก Slider มาใส่
    public TextMeshProUGUI hpText;  // ลาก TextMeshPro มาใส่

    private GameManager gameManager;
    private Vector3 originalScale;

    private void Awake()
    {
        gameManager   = FindObjectOfType<GameManager>();
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        ResetMonster();
    }

    public void ResetMonster()
    {
        currentHealth = maxHealth;
        UpdateUI();
        transform.localScale = originalScale;
        gameObject.SetActive(true);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0f) currentHealth = 0f;

        // เอฟเฟกต์ตัวเด้งนิดนึงตอนโดนตี (Juice)
        StopAllCoroutines();
        StartCoroutine(HitFeedback());

        UpdateUI();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private IEnumerator HitFeedback()
    {
        float duration = 0.1f;
        float t = 0f;
        Vector3 small = originalScale * 0.9f;

        // scale down
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = t / duration;
            transform.localScale = Vector3.Lerp(originalScale, small, a);
            yield return null;
        }

        t = 0f;
        // scale back
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = t / duration;
            transform.localScale = Vector3.Lerp(small, originalScale, a);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    private void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (hpText != null)
        {
            hpText.text = $"{Mathf.Ceil(currentHealth)} / {Mathf.Ceil(maxHealth)}";
        }
    }

    private void Die()
    {
        // แจ้ง GameManager ว่ามอนสเตอร์ตายแล้ว
        if (gameManager != null)
        {
            gameManager.OnMonsterDied();
        }

        gameObject.SetActive(false); // ซ่อนมอนสเตอร์ชั่วคราว
    }
}
