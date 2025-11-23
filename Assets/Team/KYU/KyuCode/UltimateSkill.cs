using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class UltimateSkill : MonoBehaviour
{
    public float RemainingTime => timer;

    [Header("Ultimate Settings")]
    [Tooltip("ระยะเวลาที่อยู่ในโหมดอัลติ")]
    public float ultDuration = 3f;

    [Tooltip("ความช้าของเวลา (0.3 = ช้าลงเหลือ 30%)")]
    public float slowMotionScale = 0.3f;

    [Header("Slash Settings")]
    public float minSlashSegmentDistance = 0.25f;
    public LayerMask slashHitLayers = ~0;
    [Range(0f, 1f)]
    public float damagePercentOfMaxHP = 0.01f;

    [Header("Finisher Settings")]
    [Range(0f, 1f)]
    public float finalHitPercentOfMaxHP = 0.2f;

    [Header("Visuals & Audio")]
    public Animator playerAnimator;
    public string ultTriggerName = "Ult";
    public PlayableDirector ultTimeline;

    public AudioSource sfxSource;
    public AudioClip ultStartSfx;
    public AudioClip slashSfx;
    public AudioClip finalHitSfx;
    public AudioClip ultEndSfx;

    public GameObject slashVfxPrefab;
    public GameObject hitVfxPrefab;
    public GameObject finalHitVfxPrefab;
    public float vfxLifetime = 1.0f;

    [Header("Cursor Trail (during ULT)")]
    [Tooltip("Prefab with TrailRenderer that follows the cursor while ULT is active")]
    public GameObject cursorTrailPrefab;

    [Header("Slash Timeline Prefab (optional)")]
    [Tooltip("Prefab with PlayableDirector + Timeline for each slash attack")]
    public GameObject slashTimelinePrefab;
    [Tooltip("Lifetime of spawned slash timeline object (>= timeline length)")]
    public float slashTimelineLifetime = 1.2f;

    // internal
    private Camera mainCamera;
    private bool isActive;
    private bool isDragging;
    private Vector2 lastSlashPos;
    private float timer;
    private int _ultTriggerHash;
    private readonly HashSet<Monster> monstersHitThisUlt = new HashSet<Monster>();
    private GameObject cursorTrailInstance;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null) mainCamera = Object.FindFirstObjectByType<Camera>();

        _ultTriggerHash = Animator.StringToHash(ultTriggerName);

        // disabled by default, will be enabled when ULT starts
        enabled = false;
    }

    // Called by UltimateProgression when ULT triggers
    public void StartUltimateDuration()
    {
        isActive = true;
        isDragging = false;
        timer = ultDuration;
        monstersHitThisUlt.Clear();
        enabled = true;

        Debug.Log("UltimateSkill: ULT STARTED (Slow Motion ON)");

        // Slow motion
        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // Player anim + cutscene timeline
        if (playerAnimator != null) playerAnimator.SetTrigger(_ultTriggerHash);
        if (ultTimeline != null)
        {
            ultTimeline.time = 0;
            ultTimeline.Play();
        }
        PlaySfx(ultStartSfx);

        // Spawn cursor trail
        if (cursorTrailPrefab != null && mainCamera != null)
        {
            Vector3 pos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0f;
            cursorTrailInstance = Instantiate(cursorTrailPrefab, pos, Quaternion.identity);
        }
    }

    private void Update()
    {
        if (!isActive || mainCamera == null) return;

        // countdown (unscaled because of slow motion)
        timer -= Time.unscaledDeltaTime;
        if (timer <= 0f)
        {
            EndUltimate();
            return;
        }

        UpdateCursorTrailPosition();
        HandleSlashInput();
    }

    private void UpdateCursorTrailPosition()
    {
        if (cursorTrailInstance == null || mainCamera == null) return;

        Vector3 pos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0f;
        cursorTrailInstance.transform.position = pos;
    }

    private void HandleSlashInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastSlashPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }
        else if (isDragging && Input.GetMouseButton(0))
        {
            Vector2 currentPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            float dist = Vector2.Distance(lastSlashPos, currentPos);

            if (dist >= minSlashSegmentDistance)
            {
                PerformSlash(lastSlashPos, currentPos);
                lastSlashPos = currentPos;
            }
        }
        else if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    private void PerformSlash(Vector2 start, Vector2 end)
    {
        Vector3 mid = (start + end) * 0.5f;

        // Simple VFX line slash
        SpawnVfx(slashVfxPrefab, mid, Quaternion.identity);
        PlaySfx(slashSfx);

        // OPTIONAL: Timeline-based slash cutscene
        if (slashTimelinePrefab != null)
        {
            GameObject slashTimelineObj = Instantiate(slashTimelinePrefab, mid, Quaternion.identity);
            if (slashTimelineLifetime > 0f)
                Destroy(slashTimelineObj, slashTimelineLifetime);
        }

        // Damage
        RaycastHit2D[] hits = Physics2D.LinecastAll(start, end, slashHitLayers);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null) continue;
            Monster monster = hit.collider.GetComponent<Monster>();
            if (monster == null) continue;

            if (!monstersHitThisUlt.Contains(monster))
            {
                monstersHitThisUlt.Add(monster);
            }

            float damage = monster.maxHealth * damagePercentOfMaxHP;
            monster.TakeDamage(damage);
            SpawnVfx(hitVfxPrefab, hit.point, Quaternion.identity);
        }
    }

    private void EndUltimate()
    {
        // Finisher
        if (finalHitPercentOfMaxHP > 0f && monstersHitThisUlt.Count > 0)
        {
            PlaySfx(finalHitSfx);
            foreach (Monster monster in monstersHitThisUlt)
            {
                if (monster == null) continue;

                float finisherDamage = monster.maxHealth * finalHitPercentOfMaxHP;
                monster.TakeDamage(finisherDamage);
                SpawnVfx(finalHitVfxPrefab, monster.transform.position, Quaternion.identity);
            }
        }

        monstersHitThisUlt.Clear();
        PlaySfx(ultEndSfx);

        // Reset time scale
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;

        // Destroy cursor trail
        if (cursorTrailInstance != null)
        {
            Destroy(cursorTrailInstance);
            cursorTrailInstance = null;
        }

        isActive = false;
        isDragging = false;
        enabled = false;

        Debug.Log("UltimateSkill: ULT ENDED");
    }

    // Helpers
    private void PlaySfx(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip);
    }

    private void SpawnVfx(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null) return;

        GameObject obj = Instantiate(prefab, pos, rot);
        if (vfxLifetime > 0f)
            Destroy(obj, vfxLifetime);
    }

    // Property for other scripts (e.g., UltimateProgression) to check
    public bool IsBusy => isActive;
}
