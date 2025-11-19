using UnityEngine;
using UnityEngine.Playables;

public class UltimateSkill : MonoBehaviour
{
    [Header("Duration")]
    public float ultDuration = 3f;

    [Header("Slash Settings")]
    public float minSlashDistance = 0.5f;
    public LayerMask slashHitLayers = ~0;
    [Range(0f, 1f)]
    public float damagePercentOfMaxHP = 0.5f;

    [Header("Visuals & Cutscene (optional)")]
    public Animator playerAnimator;
    public string ultTriggerName = "Ult";
    public PlayableDirector ultTimeline;

    private Camera mainCamera;
    private bool isActive;
    private bool isDragging;
    private Vector2 dragStartWorld;
    private float timer;
    private int _ultTriggerHash;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = Object.FindFirstObjectByType<Camera>();

        if (mainCamera == null)
            Debug.LogError("UltimateSkill: no Camera in scene");

        _ultTriggerHash = Animator.StringToHash(ultTriggerName);

        enabled = false; // off until ult starts
    }

    public void StartUltimateDuration()
    {
        isActive = true;
        timer = ultDuration;
        enabled = true;

        // === DEBUG: ult used ===
        Debug.Log($"UltimateSkill: ULT USED! Entering ult mode for {ultDuration} seconds.");  // <<<

        if (playerAnimator != null)
            playerAnimator.SetTrigger(_ultTriggerHash);

        if (ultTimeline != null)
        {
            ultTimeline.time = 0;
            ultTimeline.Play();
        }
    }

    private void Update()
    {
        if (!isActive || mainCamera == null)
            return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            EndUltimate();
            return;
        }

        HandleSlashInput();
    }

    private void HandleSlashInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragStartWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }
        else if (isDragging && Input.GetMouseButtonUp(0))
        {
            Vector2 dragEndWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            isDragging = false;

            float distance = Vector2.Distance(dragStartWorld, dragEndWorld);
            if (distance >= minSlashDistance)
            {
                PerformSlash(dragStartWorld, dragEndWorld);
            }
        }
    }

    private void PerformSlash(Vector2 start, Vector2 end)
    {
        RaycastHit2D[] hits = Physics2D.LinecastAll(start, end, slashHitLayers);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null) continue;

            Monster monster = hit.collider.GetComponent<Monster>();
            if (monster != null)
            {
                float damage = monster.maxHealth * damagePercentOfMaxHP;
                monster.TakeDamage(damage);
            }
        }

        Debug.Log($"UltimateSkill: SLASH {start} -> {end}");
    }

    private void EndUltimate()
    {
        isActive = false;
        enabled = false;
        Debug.Log("UltimateSkill: END ULT mode");
    }
}
