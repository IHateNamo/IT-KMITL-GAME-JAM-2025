using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class UltimateSkill : MonoBehaviour
{
    [Header("Duration")]
    [Tooltip("ระยะเวลาที่อยู่ในโหมดอัลติ (วินาที)")]
    public float ultDuration = 3f;

    [Header("Slash Settings")]
    [Tooltip("ระยะขั้นต่ำของ segment ต่อครั้ง (ยิ่งเล็กยิ่งถี่และลื่น)")]
    public float minSlashSegmentDistance = 0.25f;

    [Tooltip("เลเยอร์ที่อัลติจะโดน (ส่วนใหญ่ใช้กับมอนสเตอร์)")]
    public LayerMask slashHitLayers = ~0;

    [Tooltip("ทำดาเมจกี่ % ของเลือดสูงสุดต่อการโดนหนึ่งครั้ง (แต่ละ slash)")]
    [Range(0f, 1f)]
    public float damagePercentOfMaxHP = 0.01f;   // 1% of MAX HP per hit

    [Header("Finisher Settings")]
    [Tooltip("เปอร์เซ็นต์ดาเมจของเลือดสูงสุด ที่ใส่เพิ่มทีเดียวตอนจบอัลติ ให้มอนสเตอร์ที่เคยโดนฟันอย่างน้อย 1 ครั้ง")]
    [Range(0f, 1f)]
    public float finalHitPercentOfMaxHP = 0.2f;  // 20% of MAX HP once at the end

    [Header("Visuals & Cutscene (optional)")]
    public Animator playerAnimator;
    public string ultTriggerName = "Ult";
    public PlayableDirector ultTimeline;

    // ---------- SFX ----------
    [Header("SFX")]
    [Tooltip("AudioSource ที่ใช้เล่นเสียงอัลติ (ติดไว้ที่ Player ก็ได้)")]
    public AudioSource sfxSource;

    [Tooltip("เสียงตอนเริ่มกดใช้ Ult")]
    public AudioClip ultStartSfx;

    [Tooltip("เสียงตอนลาก Slash แต่ละเส้น")]
    public AudioClip slashSfx;

    [Tooltip("เสียงตอน Final Hit ใส่ดาเมจ 20%")]
    public AudioClip finalHitSfx;

    [Tooltip("เสียงตอนจบ Ult (ถ้ามี)")]
    public AudioClip ultEndSfx;

    // ---------- VFX ----------
    [Header("VFX - Aura / State")]
    [Tooltip("เอฟเฟกต์ออร่าเวลาที่เข้าโหมด Ult (เช่น Particle รอบตัว)")]
    public GameObject ultAuraPrefab;

    [Tooltip("จุดที่ให้ Spawn Aura (ถ้าเว้นว่างจะใช้ transform ของ Player)")]
    public Transform ultAuraAttachPoint;

    [Header("VFX - Slash")]
    [Tooltip("เอฟเฟกต์ของเส้น Slash (spawn ที่ midpoint ของ segment)")]
    public GameObject slashVfxPrefab;

    [Tooltip("เอฟเฟกต์ตอนโดนตัวมอนสเตอร์ (spawn ตรงตำแหน่งที่โดน)")]
    public GameObject hitVfxPrefab;

    [Header("VFX - Finisher")]
    [Tooltip("เอฟเฟกต์ตอน Final Hit บนตัวมอนสเตอร์แต่ละตัว")]
    public GameObject finalHitVfxPrefab;

    [Tooltip("เวลาที่ให้ VFX อยู่ก่อน Destroy (วินาที)")]
    public float vfxLifetime = 1.0f;

    private Camera mainCamera;

    private bool isActive;          // อยู่ในโหมดอัลติไหม
    private bool isDragging;        // ตอนนี้กำลังลากอยู่ไหม
    private Vector2 lastSlashPos;   // จุดก่อนหน้าสำหรับ segment
    private float timer;

    private int _ultTriggerHash;

    // เก็บมอนสเตอร์ที่เคยโดนฟันในอัลติรอบนี้ เพื่อใช้ทำ final hit ตอนจบ
    private readonly HashSet<Monster> monstersHitThisUlt = new HashSet<Monster>();

    // เก็บออร่า Ult ปัจจุบัน เพื่อ Destroy ตอนจบ
    private GameObject currentUltAuraInstance;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = Object.FindFirstObjectByType<Camera>();

        if (mainCamera == null)
            Debug.LogError("UltimateSkill: no Camera in scene");

        _ultTriggerHash = Animator.StringToHash(ultTriggerName);

        // ปิดไว้ก่อน จนกว่าจะเริ่มอัลติ
        enabled = false;
    }

    /// <summary>
    /// เรียกจาก UltimateProgression เมื่อเกจเต็ม
    /// </summary>
    public void StartUltimateDuration()
    {
        isActive = true;
        isDragging = false;
        timer = ultDuration;
        monstersHitThisUlt.Clear(); // รีเซ็ตมอนสเตอร์ที่เคยโดนจากรอบก่อน
        enabled = true;

        Debug.Log($"UltimateSkill: ULT USED! Entering ult mode for {ultDuration} seconds.");

        // Animator / Timeline
        if (playerAnimator != null)
            playerAnimator.SetTrigger(_ultTriggerHash);

        if (ultTimeline != null)
        {
            ultTimeline.time = 0;
            ultTimeline.Play();
        }

        // SFX: เริ่ม Ult
        PlaySfx(ultStartSfx);

        // VFX: Aura รอบตัวตอนเข้าโหมด Ult
        SpawnUltAura();
    }

    private void Update()
    {
        if (!isActive || mainCamera == null)
            return;

        // หมดเวลาอัลติ
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            EndUltimate();
            return;
        }

        HandleSlashInput();
    }

    /// <summary>
    /// Fruit Ninja style:
    /// - กดเมาส์ซ้ายค้าง + ลาก = ทำ segment slash ต่อเนื่อง
    /// - แต่ละ segment สามารถตีโดนมอนสเตอร์เดิมซ้ำได้ (multi-hit)
    /// - แต่ละ hit = 1% ของ Max HP (ตั้งไว้ที่ damagePercentOfMaxHP)
    /// </summary>
    private void HandleSlashInput()
    {
        // เริ่ม stroke ใหม่
        if (Input.GetMouseButtonDown(0))
        {
            lastSlashPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }
        // ระหว่างลาก
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
        // ปล่อยเมาส์ = จบ stroke
        else if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    private void PerformSlash(Vector2 start, Vector2 end)
    {
        // VFX ของเส้น Slash (กลาง segment)
        SpawnSlashVfx(start, end);

        // SFX ของ Slash หนึ่ง segment
        PlaySfx(slashSfx);

        RaycastHit2D[] hits = Physics2D.LinecastAll(start, end, slashHitLayers);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            Monster monster = hit.collider.GetComponent<Monster>();
            if (monster == null)
                continue;

            // เก็บมอนสเตอร์ไว้สำหรับ final hit ตอนจบ
            if (!monstersHitThisUlt.Contains(monster))
            {
                monstersHitThisUlt.Add(monster);
            }

            // ดาเมจจากแต่ละ slash = % ของ "เลือดสูงสุด"
            float damage = monster.maxHealth * damagePercentOfMaxHP;
            monster.TakeDamage(damage);

            // VFX ตอนโดนตัวมอนสเตอร์
            SpawnHitVfx(hit, monster);
        }

        Debug.Log($"UltimateSkill: SLASH {start} -> {end}");
    }

    private void EndUltimate()
    {
        // ตอนจบอัลติ: Final Hit ใส่เพิ่มทีเดียว 20% (หรือแล้วแต่ finalHitPercentOfMaxHP)
        if (finalHitPercentOfMaxHP > 0f && monstersHitThisUlt.Count > 0)
        {
            // SFX Final Hit (เล่นครั้งเดียวตอนฟาดรวม)
            PlaySfx(finalHitSfx);

            foreach (Monster monster in monstersHitThisUlt)
            {
                if (monster == null)
                    continue; // เผื่อมอนสเตอร์ตาย/โดนลบไปแล้ว

                float finisherDamage = monster.maxHealth * finalHitPercentOfMaxHP;
                monster.TakeDamage(finisherDamage);

                // VFX Final Hit บนตัวมอนสเตอร์แต่ละตัว
                SpawnFinalHitVfx(monster);

                Debug.Log($"UltimateSkill: FINISHER hit on {monster.name}, damage = {finisherDamage}");
            }
        }

        monstersHitThisUlt.Clear();

        // VFX: ปิดออร่า Ult
        DestroyUltAura();

        // SFX: จบ Ult
        PlaySfx(ultEndSfx);

        isActive   = false;
        enabled    = false;
        isDragging = false;

        Debug.Log("UltimateSkill: END ULT mode (finisher applied)");
    }

    // ---------- Helper: SFX ----------

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip);
    }

    // ---------- Helper: VFX ----------

    private void SpawnUltAura()
    {
        if (ultAuraPrefab == null)
            return;

        Transform parent = ultAuraAttachPoint != null ? ultAuraAttachPoint : transform;

        currentUltAuraInstance = Instantiate(ultAuraPrefab, parent.position, Quaternion.identity, parent);
    }

    private void DestroyUltAura()
    {
        if (currentUltAuraInstance != null)
        {
            Destroy(currentUltAuraInstance);
            currentUltAuraInstance = null;
        }
    }

    private void SpawnSlashVfx(Vector2 start, Vector2 end)
    {
        if (slashVfxPrefab == null)
            return;

        Vector2 mid = (start + end) * 0.5f;
        Vector2 dir = (end - start).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        GameObject vfx = Instantiate(slashVfxPrefab, mid, Quaternion.Euler(0f, 0f, angle));
        if (vfxLifetime > 0f)
        {
            Destroy(vfx, vfxLifetime);
        }
    }

    private void SpawnHitVfx(RaycastHit2D hit, Monster monster)
    {
        if (hitVfxPrefab == null)
            return;

        Vector3 spawnPos = hit.point != Vector2.zero ? (Vector3)hit.point : monster.transform.position;

        GameObject vfx = Instantiate(hitVfxPrefab, spawnPos, Quaternion.identity);
        if (vfxLifetime > 0f)
        {
            Destroy(vfx, vfxLifetime);
        }
    }

    private void SpawnFinalHitVfx(Monster monster)
    {
        if (finalHitVfxPrefab == null || monster == null)
            return;

        GameObject vfx = Instantiate(finalHitVfxPrefab, monster.transform.position, Quaternion.identity);
        if (vfxLifetime > 0f)
        {
            Destroy(vfx, vfxLifetime);
        }
    }
}
