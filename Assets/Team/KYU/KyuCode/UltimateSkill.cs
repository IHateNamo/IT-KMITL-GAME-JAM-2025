using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class UltimateSkill : MonoBehaviour
{
    // ========================================================================
    // ไม่มีการนับแต้มในนี้แล้ว (เพราะ UltimateProgression จัดการให้)
    // ========================================================================

    [Header("Ultimate Settings")]
    [Tooltip("ระยะเวลาที่อยู่ในโหมดอัลติ")]
    public float ultDuration = 3f;
    
    [Tooltip("ความช้าของเวลา (0.3 = ช้าลงเหลือ 30%)")]
    public float slowMotionScale = 0.3f; // ✅ ยังมี Slow Motion ตามที่ขอ

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

    // ตัวแปรภายใน
    private Camera mainCamera;
    private bool isActive;
    private bool isDragging;
    private Vector2 lastSlashPos;
    private float timer;
    private int _ultTriggerHash;
    private readonly HashSet<Monster> monstersHitThisUlt = new HashSet<Monster>();

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null) mainCamera = Object.FindFirstObjectByType<Camera>();

        _ultTriggerHash = Animator.StringToHash(ultTriggerName);

        // ปิดการทำงานไว้ก่อน รอให้ UltimateProgression สั่งเปิด
        enabled = false;
    }

    // ฟังก์ชันนี้จะถูกเรียกจาก UltimateProgression
    public void StartUltimateDuration()
    {
        isActive = true;
        isDragging = false;
        timer = ultDuration;
        monstersHitThisUlt.Clear();
        enabled = true; // เปิดให้ Update ทำงาน

        Debug.Log("UltimateSkill: ULT STARTED (Slow Motion ON)");

        // ✅ เปิด Slow Motion
        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // เล่น Animation / Timeline
        if (playerAnimator != null) playerAnimator.SetTrigger(_ultTriggerHash);
        if (ultTimeline != null) { ultTimeline.time = 0; ultTimeline.Play(); }
        PlaySfx(ultStartSfx);
    }

    private void Update()
    {
        if (!isActive || mainCamera == null) return;

        // นับถอยหลัง (ใช้ unscaledDeltaTime เพราะเวลาเดินช้า)
        timer -= Time.unscaledDeltaTime;

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
        SpawnVfx(slashVfxPrefab, (start + end) * 0.5f, Quaternion.identity); 
        PlaySfx(slashSfx);

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
                if (monster != null)
                {
                    float finisherDamage = monster.maxHealth * finalHitPercentOfMaxHP;
                    monster.TakeDamage(finisherDamage);
                    SpawnVfx(finalHitVfxPrefab, monster.transform.position, Quaternion.identity);
                }
            }
        }

        monstersHitThisUlt.Clear();
        PlaySfx(ultEndSfx);

        // ✅ ปิด Slow Motion (คืนค่าเวลา)
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;

        isActive = false;
        isDragging = false;
        enabled = false; // ปิด Update

        Debug.Log("UltimateSkill: ULT ENDED");
    }

    // Helper Functions
    private void PlaySfx(AudioClip clip)
    {
        if (clip != null && sfxSource != null) sfxSource.PlayOneShot(clip);
    }

    private void SpawnVfx(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab != null) Destroy(Instantiate(prefab, pos, rot), vfxLifetime);
    }

    // Property ให้ UltimateProgression เช็คสถานะ
    public bool isActiveAndEnabled => isActive;
}