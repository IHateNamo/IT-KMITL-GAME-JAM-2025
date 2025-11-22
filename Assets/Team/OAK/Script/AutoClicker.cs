using UnityEngine;
using System.Collections;

public class AutoClicker : MonoBehaviour
{
    [Header("Auto Click Settings")]
    public float clicksPerSecond = 1f;
    
    [Range(0f, 2f)]
    public float damageMultiplier = 0.5f;
    
    public bool isAutoClickEnabled = false;
    
    [Header("References")]
    public UpgradeManager upgradeManager;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLog = true;
    
    private float autoClickInterval;
    private Monster currentTarget;
    
    private void Start()
    {
        UpdateAutoClickInterval();
        
        if (isAutoClickEnabled)
        {
            StartAutoClick();
        }
    }
    
    private void UpdateAutoClickInterval()
    {
        autoClickInterval = clicksPerSecond > 0 ? 1f / clicksPerSecond : 1f;
    }
    
    public void StartAutoClick()
    {
        if (!isAutoClickEnabled)
        {
            isAutoClickEnabled = true;
            UpdateAutoClickInterval();
            StartCoroutine(AutoClickCoroutine());
            Debug.Log("✅ Auto Click เริ่มทำงาน (ไม่นับคอมโบ)");
        }
    }
    
    public void StopAutoClick()
    {
        isAutoClickEnabled = false;
        StopAllCoroutines();
        Debug.Log("⏸️ Auto Click หยุด");
    }
    
    private IEnumerator AutoClickCoroutine()
    {
        while (isAutoClickEnabled)
        {
            FindTarget();
            
            if (currentTarget != null && currentTarget.currentHealth > 0)
            {
                PerformAutoClick();
            }
            
            yield return new WaitForSeconds(autoClickInterval);
        }
    }
    
    private void FindTarget()
    {
        if (currentTarget != null && currentTarget.currentHealth > 0)
            return;
        
        currentTarget = FindFirstObjectByType<Monster>();
    }
    
    /// <summary>
    /// ทำดาเมจโดยไม่นับคอมโบ
    /// </summary>
    private void PerformAutoClick()
    {
        if (currentTarget == null || upgradeManager == null)
            return;
        
        float baseDamage = upgradeManager.GetCurrentDamage();
        float autoDamage = baseDamage * damageMultiplier;
        
        // ลอง bypass ก่อน
        MonsterDamageBypass bypass = currentTarget.GetComponent<MonsterDamageBypass>();
        
        if (bypass != null)
        {
            bypass.ApplyDirectDamage(autoDamage);
            
            if (showDebugLog)
            {
                Debug.Log($"🤖 Auto Click: {autoDamage:F1} dmg [BYPASS - ไม่นับคอมโบ]");
            }
        }
        else
        {
            // Fallback (จะนับคอมโบ)
            currentTarget.TakeDamage(autoDamage);
            
            if (showDebugLog)
            {
                Debug.LogWarning($"⚠️ Auto Click: {autoDamage:F1} dmg [ไม่มี bypass - ยังนับคอมโบ!]");
            }
        }
    }
    
    public void UpdateAutoClickStats(float newClicksPerSecond, float newDamageMultiplier)
    {
        clicksPerSecond = newClicksPerSecond;
        damageMultiplier = newDamageMultiplier;
        UpdateAutoClickInterval();
        
        if (isAutoClickEnabled)
        {
            StopAllCoroutines();
            StartCoroutine(AutoClickCoroutine());
        }
        
        Debug.Log($"✅ Auto Click Update: {clicksPerSecond} CPS, {damageMultiplier * 100}% dmg");
    }
}
