using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UltimateProgressionView : MonoBehaviour
{
    [Header("Core Reference")]
    public UltimateProgression progression;

    [Header("UI To Animate")]
    public RectTransform sliderRect;      // RectTransform of the Slider (or its fill)
    public RectTransform textRect;        // RectTransform of the TMP text

    [Header("VFX")]
    public ParticleSystem onClickVFX;     // Small effect per click
    public ParticleSystem onFullVFX;      // Big effect when ULT is ready / fired

    [Header("DOTween Settings")]
    [Tooltip("Scale punch when clicking (normal charge)")]
    public float clickPunchScale = 1.08f;
    public float clickPunchDuration = 0.15f;

    [Tooltip("Scale punch when bar is full / ULT fires")]
    public float fullPunchScale = 1.2f;
    public float fullPunchDuration = 0.3f;

    private void Awake()
    {
        if (progression == null)
            progression = GetComponent<UltimateProgression>();
    }

    /// <summary>
    /// Call this from other scripts / Button instead of calling progression.RegisterClick() directly.
    /// </summary>
    public void RegisterClickFromOutside()
    {
        if (progression == null)
        {
            Debug.LogWarning("UltimateProgressionView: progression not assigned");
            return;
        }

        // Remember state BEFORE click
        int previousClicks = progression.currentClicks;

        // Use the existing logic in your original script (we don't touch that file)
        progression.RegisterClick();

        // If previous < max and NOW we got reset to 0 -> that means bar was full and ult fired
        bool ultJustTriggered = previousClicks > 0 &&
                                previousClicks < progression.clicksToUlt &&
                                progression.currentClicks == 0;

        if (ultJustTriggered)
        {
            PlayFullBarAnimation();
        }
        else
        {
            PlayClickAnimation();
        }
    }

    private void PlayClickAnimation()
    {
        // Small punch for normal clicks
        if (sliderRect != null)
        {
            sliderRect.DOKill(true);
            sliderRect.localScale = Vector3.one;
            sliderRect.DOPunchScale(
                Vector3.one * (clickPunchScale - 1f),
                clickPunchDuration,
                vibrato: 1,
                elasticity: 0.5f
            );
        }

        if (textRect != null)
        {
            textRect.DOKill(true);
            textRect.localScale = Vector3.one;
            textRect.DOPunchScale(
                Vector3.one * (clickPunchScale - 1f),
                clickPunchDuration,
                vibrato: 1,
                elasticity: 0.5f
            );
        }

        if (onClickVFX != null)
        {
            onClickVFX.Play();
        }
    }

    private void PlayFullBarAnimation()
    {
        // Bigger punch when ULT bar fills / ULT is consumed
        if (sliderRect != null)
        {
            sliderRect.DOKill(true);
            sliderRect.localScale = Vector3.one;
            sliderRect.DOPunchScale(
                Vector3.one * (fullPunchScale - 1f),
                fullPunchDuration,
                vibrato: 2,
                elasticity: 0.5f
            );
        }

        if (textRect != null)
        {
            textRect.DOKill(true);
            textRect.localScale = Vector3.one;
            textRect.DOPunchScale(
                Vector3.one * (fullPunchScale - 1f),
                fullPunchDuration,
                vibrato: 2,
                elasticity: 0.5f
            );
        }

        if (onFullVFX != null)
        {
            onFullVFX.Play();
        }
    }
}
