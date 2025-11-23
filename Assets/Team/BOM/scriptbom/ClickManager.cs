using UnityEngine;

public class ClickManager : MonoBehaviour
{
    [Header("Combat")]
    public LayerMask monsterLayer;
    public float clickDamage = 10f;

    [Header("Progression")]
    public UltimateProgression ultimateProgression;          // logic
    public UltimateProgressionView ultimateProgressionView;  // UI + VFX

    [Header("Camera (auto-find if empty)")]
    [SerializeField] private Camera mainCamera;

    // ========================= PLAYER EFFECTS =========================
    [Header("Player Effects (NORMAL CLICK)")]
    [Tooltip("Main player Animator (old one)")]
    public Animator primaryAnimator;

    [Tooltip("Trigger name on main Animator")]
    public string primaryAttackTriggerName = "Attack";

    [Tooltip("SFX when clicking a monster")]
    public AudioSource playerSfxSource;
    public AudioClip playerClickSfx;

    [Header("VFX")]
    public GameObject clickVfxPrefab;
    public Transform clickVfxSpawnPoint;
    public float clickVfxLifetime = 1.0f;

    [Header("Attack Timeline Prefab (optional)")]
    [Tooltip("Prefab that contains PlayableDirector + Timeline for normal attack")]
    public GameObject attackTimelinePrefab;
    public Transform attackTimelineSpawnPoint;
    public float attackTimelineLifetime = 2.0f;

    // ==================== EXTRA ANIMATORS SUPPORT =====================

    [System.Serializable]
    public class AdditionalAnimator
    {
        [Tooltip("Extra animator that should also react when you click a monster.")]
        public Animator animator;

        [Tooltip("Trigger name for this animator. If empty, use primaryAttackTriggerName.")]
        public string triggerName;
    }

    [Header("Extra Animators (optional)")]
    [Tooltip("All these animators will be triggered together with the main one.")]
    public AdditionalAnimator[] extraAnimators;

    // ==================================================================

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                mainCamera = Object.FindFirstObjectByType<Camera>();
        }

        if (mainCamera == null)
            Debug.LogError("ClickManager: no Camera in scene");

        // Auto-link progression from view if not set manually
        if (ultimateProgression == null && ultimateProgressionView != null)
        {
            ultimateProgression = ultimateProgressionView.progression;
        }
    }

    private void Update()
    {
        if (mainCamera == null) return;

        // block normal clicks while ult mode is active
        if (ultimateProgression != null && ultimateProgression.IsUltimateActive)
            return;

        if (Input.GetMouseButtonDown(0))
            DetectClick();
    }

    private void DetectClick()
    {
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(
            mousePos,
            Vector2.zero,
            Mathf.Infinity,
            monsterLayer
        );

        if (hit.collider == null) return;

        Monster monster = hit.collider.GetComponent<Monster>();
        if (monster == null) return;

        // 1) DAMAGE
        monster.TakeDamage(clickDamage);

        // 2) ULT PROGRESSION
        if (ultimateProgressionView != null)
        {
            ultimateProgressionView.RegisterClickFromOutside();
        }
        else if (ultimateProgression != null)
        {
            ultimateProgression.RegisterClick();
        }

        // 3) FEEDBACK (ANIM + SFX + VFX + TIMELINE)
        PlayPlayerEffects();
    }

    private void PlayPlayerEffects()
    {
        // --- MAIN ANIMATOR (old behaviour) ---
        if (primaryAnimator != null && !string.IsNullOrEmpty(primaryAttackTriggerName))
        {
            int trig = Animator.StringToHash(primaryAttackTriggerName);
            primaryAnimator.SetTrigger(trig);
        }

        // --- EXTRA ANIMATORS (new) ---
        if (extraAnimators != null)
        {
            foreach (var entry in extraAnimators)
            {
                if (entry == null || entry.animator == null) continue;

                string trigName = string.IsNullOrEmpty(entry.triggerName)
                    ? primaryAttackTriggerName       // use old trigger
                    : entry.triggerName;

                if (!string.IsNullOrEmpty(trigName))
                {
                    int trigHash = Animator.StringToHash(trigName);
                    entry.animator.SetTrigger(trigHash);
                }
            }
        }

        // --- SFX ---
        if (playerSfxSource != null && playerClickSfx != null)
        {
            playerSfxSource.PlayOneShot(playerClickSfx);
        }

        // --- VFX ---
        if (clickVfxPrefab != null)
        {
            Transform spawnT = clickVfxSpawnPoint != null
                ? clickVfxSpawnPoint
                : (primaryAnimator != null ? primaryAnimator.transform : transform);

            GameObject vfx = Instantiate(clickVfxPrefab, spawnT.position, spawnT.rotation);
            if (clickVfxLifetime > 0f)
                Destroy(vfx, clickVfxLifetime);
        }

        // --- ATTACK TIMELINE PREFAB ---
        if (attackTimelinePrefab != null)
        {
            Transform spawnT = attackTimelineSpawnPoint != null
                ? attackTimelineSpawnPoint
                : transform;

            GameObject timelineObj = Instantiate(attackTimelinePrefab, spawnT.position, spawnT.rotation);

            // PlayableDirector inside the prefab should have Play On Awake enabled
            if (attackTimelineLifetime > 0f)
                Destroy(timelineObj, attackTimelineLifetime);
        }
    }
}
