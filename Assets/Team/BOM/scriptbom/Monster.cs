using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Monster : MonoBehaviour
{
    [Header("Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI")]
    public Slider healthBar;
    public TextMeshProUGUI hpText;

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
        if (ComboOverheatSystem.Instance != null)
        {
        ComboOverheatSystem.Instance.RegisterClickHit(this, damage);
        }

        currentHealth -= damage;
        if (currentHealth < 0f) currentHealth = 0f;

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

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = t / duration;
            transform.localScale = Vector3.Lerp(originalScale, small, a);
            yield return null;
        }

        t = 0f;
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
        if (gameManager != null)
        {
            gameManager.OnMonsterDied();
        }

        gameObject.SetActive(false);
    }
}
