using UnityEngine;

public class ClickManager : MonoBehaviour
{
    [Header("Combat")]
    public LayerMask monsterLayer;
    public float clickDamage = 10f;

    [Header("Progression")]
    public UltimateProgression ultimateProgression;

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
    }

    private void Update()
    {
        if (mainCamera == null) return;

        if (ultimateProgression != null && ultimateProgression.IsUltimateActive)
            return; // disable normal clicks while ult mode

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

        if (ultimateProgression != null)
        {
            ultimateProgression.RegisterClick();
        }
    }
}
