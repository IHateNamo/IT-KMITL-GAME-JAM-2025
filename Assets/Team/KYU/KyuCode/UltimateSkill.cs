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

    [Tooltip("ทำดาเมจกี่ % ของเลือดสูงสุดต่อการโดนหนึ่งครั้ง")]
    [Range(0f, 1f)]
    public float damagePercentOfMaxHP = 0.01f;   // 1% of MAX HP per hit

    [Header("Visuals & Cutscene (optional)")]
    public Animator playerAnimator;
    public string ultTriggerName = "Ult";
    public PlayableDirector ultTimeline;

    private Camera mainCamera;

    private bool isActive;          // อยู่ในโหมดอัลติไหม
    private bool isDragging;        // ตอนนี้กำลังลากอยู่ไหม
    private Vector2 lastSlashPos;   // จุดก่อนหน้าสำหรับ segment
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

        // ปิดไว้ก่อน จนกว่าจะเริ่มอัลติ
        enabled = false;
    }

    /// <summary>
    /// เรียกจาก UltimateProgression เมื่อเกจเต็ม
    /// </summary>
    public void StartUltimateDuration()
    {
        isActive = true;
        timer = ultDuration;
        enabled = true;

        Debug.Log($"UltimateSkill: ULT USED! Entering ult mode for {ultDuration} seconds.");

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
        RaycastHit2D[] hits = Physics2D.LinecastAll(start, end, slashHitLayers);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null) continue;

            Monster monster = hit.collider.GetComponent<Monster>();
            if (monster == null) continue;

            // ดาเมจ = % ของ "เลือดสูงสุด" ทุกครั้งที่โดน
            float damage = monster.maxHealth * damagePercentOfMaxHP;
            monster.TakeDamage(damage);
        }

        Debug.Log($"UltimateSkill: SLASH {start} -> {end}");
    }

    private void EndUltimate()
    {
        isActive  = false;
        enabled   = false;
        isDragging = false;

        Debug.Log("UltimateSkill: END ULT mode");
    }
}
