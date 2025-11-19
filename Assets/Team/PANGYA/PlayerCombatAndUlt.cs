using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Playables; // for Timeline (optional)

public class PlayerCombatAndUlt : MonoBehaviour
{
    [Header("Ult Gauge")]
    [SerializeField] private float maxUlt = 100f;
    [SerializeField] private Slider ultSlider;          // UI bar
    [SerializeField] private TextMeshProUGUI ultText;   // e.g. "50 / 100"
    [SerializeField] private float chargePerDamage = 1f;

    [Header("Slash Settings")]
    [SerializeField] private float minSlashDistance = 0.5f;   // world units
    [SerializeField] private LayerMask slashHitLayers = ~0;   // default: everything

    [Header("Cutscene (Optional)")]
    [SerializeField] private PlayableDirector ultTimeline;    // Uma Musume style Timeline
    [SerializeField] private Animator animator;               // same Animator as Player
    [SerializeField] private string ultTriggerName = "Ult";   // must match Animator parameter

    private float currentUlt;
    private bool isDragging;
    private Vector2 dragStartWorld;

    private int _ultTriggerHash;

    // Simple singleton so ClickManager can notify us
    private static PlayerCombatAndUlt _instance;
    public static PlayerCombatAndUlt Instance => _instance;

    public bool IsUltReady => currentUlt >= maxUlt - 0.01f;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }

        _instance = this;

        if (animator == null)
            animator = GetComponent<Animator>();

        _ultTriggerHash = Animator.StringToHash(ultTriggerName);
    }

    private void Start()
    {
        currentUlt = 0f;
        UpdateUI();
    }

    /// <summary>
    /// Called by ClickManager when you successfully hit a monster.
    /// </summary>
    public void OnSuccessfulHit(float damage)
    {
        AddCharge(damage * chargePerDamage);
    }

    private void AddCharge(float amount)
    {
        currentUlt = Mathf.Clamp(currentUlt + amount, 0f, maxUlt);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (ultSlider != null)
        {
            ultSlider.maxValue = maxUlt;
            ultSlider.value = currentUlt;
        }

        if (ultText != null)
        {
            ultText.text = $"{Mathf.RoundToInt(currentUlt)} / {Mathf.RoundToInt(maxUlt)}";
        }
    }

    private void Update()
    {
        // Only listen for ult-drag when bar is full
        if (!IsUltReady)
            return;

        // Start drag
        if (Input.GetMouseButtonDown(0))
        {
            dragStartWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }
        // End drag
        else if (isDragging && Input.GetMouseButtonUp(0))
        {
            Vector2 dragEndWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isDragging = false;

            float distance = Vector2.Distance(dragStartWorld, dragEndWorld);
            if (distance >= minSlashDistance)
            {
                UseUltSlash(dragStartWorld, dragEndWorld);
            }
        }
    }

    private void UseUltSlash(Vector2 start, Vector2 end)
    {
        // Consume ult
        currentUlt = 0f;
        UpdateUI();

        // Linecast along slash path and hit every collider
        RaycastHit2D[] hits = Physics2D.LinecastAll(start, end, slashHitLayers);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null) continue;

            Monster monster = hit.collider.GetComponent<Monster>();
            if (monster != null)
            {
                // Example: big damage = 50% of its max HP
                float ultDamage = monster.maxHealth * 0.5f;
                monster.TakeDamage(ultDamage);
            }
        }

        // Play ult animation
        if (animator != null)
        {
            animator.SetTrigger(_ultTriggerHash);
        }

        // Optional Timeline cutscene (camera, VFX, etc.)
        if (ultTimeline != null)
        {
            ultTimeline.Play();
        }
    }

    // Call this from the end of the ult animation via Animation Event if you need to do something
    public void OnUltAnimationFinished()
    {
        // Put post-ult logic here if you want (e.g., reset camera).
    }
}
