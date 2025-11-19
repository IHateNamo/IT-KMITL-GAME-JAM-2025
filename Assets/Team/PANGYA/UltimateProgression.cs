using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UltimateProgression : MonoBehaviour
{
    [Header("Progression")]
    public int currentClicks = 0;
    public int clicksToUlt = 100;

    [Header("UI (optional)")]
    public Slider ultSlider;
    public TextMeshProUGUI ultText;

    [Header("References")]
    public UltimateSkill ultimateSkill;

    public bool IsUltimateActive
    {
        get
        {
            return ultimateSkill != null && ultimateSkill.isActiveAndEnabled;
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    public void RegisterClick()
    {
        if (IsUltimateActive)
            return;

        currentClicks++;
        Debug.Log($"UltimateProgression: Click {currentClicks}/{clicksToUlt}");

        if (currentClicks >= clicksToUlt)
        {
            Debug.Log("UltimateProgression: ULT BAR FULLY CHARGED!");
            ActivateUltimate();
            currentClicks = 0;
        }

        UpdateUI();
    }

    private void ActivateUltimate()
    {
        if (ultimateSkill != null)
        {
            ultimateSkill.StartUltimateDuration();
        }
        else
        {
            Debug.LogWarning("UltimateProgression: ultimateSkill not assigned");
        }
    }

    private void UpdateUI()
    {
        if (ultSlider != null)
        {
            ultSlider.maxValue = clicksToUlt;
            ultSlider.value = currentClicks;
        }

        if (ultText != null)
        {
            ultText.text = $"{currentClicks} / {clicksToUlt}";
        }
    }
}
