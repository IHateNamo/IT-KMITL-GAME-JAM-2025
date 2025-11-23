using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables; 
using System.Collections;    
using System.Collections.Generic;

[System.Serializable]
public struct BossPhase
{
    public string phaseName;
    [Range(0f, 1f)] public float healthPercentageTrigger; 
    public PlayableAsset timelineAnimation; 
}

public class BossMonster : Monster
{
    [Header("Boss Specifics")]
    public float maxBreakGauge = 50f;
    public float currentBreakGauge;
    public float breakRecoveryRate = 2f; 
    public float breakDamageMultiplier = 2.0f; 
    
    [Tooltip("How long the boss stays stunned when broken")]
    public float breakStunDuration = 3.0f; 

    [Header("Boss UI (Auto-Wired)")]
    public Slider breakGaugeSlider;

    [Header("Phases & Timeline")]
    public PlayableDirector director; 
    public List<BossPhase> phases;
    
    private int currentPhaseIndex = 0;
    private bool isBroken = false;

    protected override void Awake()
    {
        // Runs the Monster Awake first (wires up HP and Text)
        base.Awake();

        if (director == null) director = GetComponent<PlayableDirector>();

        // Auto-Wire Break Gauge
        if (breakGaugeSlider == null)
        {
            GameObject breakObj = GameObject.Find("BreakGauge"); 
            if (breakObj != null) 
                breakGaugeSlider = breakObj.GetComponent<Slider>();
        }   
    }

    private void Update()
    {
        if (!isBroken && currentBreakGauge < maxBreakGauge)
        {
            currentBreakGauge += breakRecoveryRate * Time.deltaTime;
            UpdateBossUI();
        }
    }

    public override void ResetMonster()
    {
        base.ResetMonster();
        
        currentBreakGauge = maxBreakGauge;
        currentPhaseIndex = 0;
        isBroken = false;
        UpdateBossUI();

        Debug.Log("Boss Spawned!");
    }

    public override void TakeDamage(float damage)
    {
        float actualDamage = damage;

        if (isBroken)
        {
            actualDamage *= breakDamageMultiplier; 
        }
        else
        {
            currentBreakGauge -= damage;
            if (currentBreakGauge <= 0)
            {
                StartCoroutine(BreakStateRoutine());
            }
        }

        base.TakeDamage(actualDamage);
        
        UpdateBossUI();
        CheckPhaseTransition();
    }

    private void CheckPhaseTransition()
    {
        if (phases == null || phases.Count == 0) return;
        if (currentPhaseIndex >= phases.Count) return;

        BossPhase nextPhase = phases[currentPhaseIndex];
        float healthPercent = currentHealth / maxHealth;

        if (healthPercent <= nextPhase.healthPercentageTrigger)
        {
            EnterPhase(nextPhase);
            currentPhaseIndex++;
        }
    }

    private void EnterPhase(BossPhase phase)
    {
        Debug.Log($"Entering Boss Phase: {phase.phaseName}");

        if (director != null && phase.timelineAnimation != null)
        {
            director.Play(phase.timelineAnimation);
        }
        
        currentBreakGauge = maxBreakGauge; 
    }

    private IEnumerator BreakStateRoutine()
    {
        isBroken = true;
        currentBreakGauge = 0;
        
        Debug.Log(">>> BOSS BREAK!! Stunned! <<<");

        // Countdown Logic
        float timer = breakStunDuration;
        while (timer > 0)
        {
            Debug.Log($"Boss recovering in: {timer} seconds...");
            yield return new WaitForSeconds(1.0f); // Wait 1 second
            timer -= 1.0f;
        }

        isBroken = false;
        currentBreakGauge = maxBreakGauge; 
        Debug.Log(">>> Boss Recovered from Break. Shield Active! <<<");
    }

    private void UpdateBossUI()
    {
        if (breakGaugeSlider != null)
        {
            breakGaugeSlider.maxValue = maxBreakGauge;
            breakGaugeSlider.value = currentBreakGauge;
        }
    }
}