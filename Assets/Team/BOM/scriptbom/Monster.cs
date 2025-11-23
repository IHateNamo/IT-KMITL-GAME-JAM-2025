using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; 

public class Monster : MonoBehaviour
{
    [Header("Base Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Base UI (Auto-Wired)")]
    public Slider healthBar;
    public TextMeshProUGUI hpText;

    protected GameManager gameManager;
    protected Vector3 originalScale;

    // We store the specific animation routine here so we don't stop EVERYTHING
    private Coroutine hitFeedbackCoroutine; 

    protected virtual void Awake()
    {
        gameManager = Object.FindFirstObjectByType<GameManager>();
        originalScale = transform.localScale;

        if (hpText == null)
        {
            GameObject textObj = GameObject.Find("HPText"); 
            if (textObj != null) 
                hpText = textObj.GetComponent<TextMeshProUGUI>();
        }

        if (healthBar == null)
        {
            GameObject sliderObj = GameObject.Find("HPBar");
            if (sliderObj == null) sliderObj = GameObject.Find("Slider");

            if (sliderObj != null) 
                healthBar = sliderObj.GetComponent<Slider>();
        }
    }

    protected virtual void OnEnable()
    {
        ResetMonster();
    }

    public virtual void ResetMonster()
    {
        currentHealth = maxHealth;
        UpdateUI();
        transform.localScale = originalScale;
        gameObject.SetActive(true);
    }

    public virtual void TakeDamage(float damage)
    {
        if (ComboOverheatSystem.Instance != null)
        {
            ComboOverheatSystem.Instance.RegisterClickHit(this, damage);
        }

        currentHealth -= damage;
        if (currentHealth < 0f) currentHealth = 0f;

        // --- FIX START ---
        // Old Code: StopAllCoroutines(); (This killed the Boss Timer!)
        
        // New Code: Only stop the hit feedback animation
        if (hitFeedbackCoroutine != null) 
        {
            StopCoroutine(hitFeedbackCoroutine);
        }
        hitFeedbackCoroutine = StartCoroutine(HitFeedback());
        // --- FIX END ---

        UpdateUI();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    protected virtual IEnumerator HitFeedback()
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

    protected virtual void UpdateUI()
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

    protected virtual void Die()
    {
        if (gameManager != null)
        {
            gameManager.OnMonsterDied(this);
        }

        gameObject.SetActive(false);
    }
}