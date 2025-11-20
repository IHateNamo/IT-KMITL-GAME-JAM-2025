using UnityEngine;

public class ClickManager : MonoBehaviour
{
    [Header("Combat")]
    public LayerMask monsterLayer;
    public float clickDamage = 10f;

    [Header("Progression")]
    public UltimateProgression ultimateProgression;          // logic (unchanged)
    public UltimateProgressionView ultimateProgressionView;  // NEW: visuals + DOTween

    [Header("Camera (auto-find if empty)")]
    [SerializeField] private Camera mainCamera;

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

        // block normal clicks while ult mode active
        if (ultimateProgression != null && ultimateProgression.IsUltimateActive)
            return;

        if (Input.GetMouseButtonDown(0))
            DetectClick();
    }

    private void DetectClick()
    {
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, monsterLayer);

        if (hit.collider == null) return;

        Monster monster = hit.collider.GetComponent<Monster>();
        if (monster == null) return;

        monster.TakeDamage(clickDamage);
        Debug.Log("Hit Monster! damage = " + clickDamage);

        // 👉 Progress bar logic + DOTween + VFX are handled in another file
        if (ultimateProgressionView != null)
        {
            // This calls UltimateProgression.RegisterClick() inside,
            // AND plays UI animation & VFX
            ultimateProgressionView.RegisterClickFromOutside();
        }
        else if (ultimateProgression != null)
        {
            // Fallback: old behavior (no DOTween) if view is not assigned
            ultimateProgression.RegisterClick();
        }
    }
}
