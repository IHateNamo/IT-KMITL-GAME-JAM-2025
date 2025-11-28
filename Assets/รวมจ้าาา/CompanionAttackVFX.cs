using System.Collections;
using UnityEngine;

/// <summary>
/// VFX ที่ใช้ตอน Companion โจมตี
/// จะบินจากจุด spawn ไปหา Monster แล้วทำดาเมจ
/// ใช้ MonsterDamageBypass ก่อน ถ้าไม่มีค่อย fallback ไปที่ TakeDamage
/// </summary>
public class CompanionAttackVFX : MonoBehaviour
{
    [Tooltip("curve การเคลื่อนที่ (0 = เริ่ม, 1 = ถึงเป้าหมาย)")]
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("ลบตัวเองอัตโนมัติกรณีผิดพลาดไม่ถึงเป้าหมาย (วินาที)")]
    public float safetyLifetime = 2f;

    private Monster target;
    private float damage;
    private float travelTime;

    private bool initialized = false;

    public void Initialize(Monster target, float damage, float travelTime)
    {
        this.target = target;
        this.damage = damage;
        this.travelTime = Mathf.Max(0.01f, travelTime);
        initialized = true;

        StartCoroutine(MoveAndHitRoutine());
    }

    private IEnumerator MoveAndHitRoutine()
    {
        if (!initialized)
        {
            Debug.LogWarning("CompanionAttackVFX: Initialize() ยังไม่ถูกเรียก");
            yield break;
        }

        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = target != null ? target.transform.position : startPos;

        // เคลื่อนที่ไปหา target
        while (elapsed < travelTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / travelTime);
            float curved = moveCurve.Evaluate(t);

            if (target != null)
            {
                targetPos = target.transform.position;
            }

            transform.position = Vector3.Lerp(startPos, targetPos, curved);

            yield return null;
        }

        // ถึงเป้าหมาย -> ทำดาเมจ
        if (target != null && target.currentHealth > 0f)
        {
            var bypass = target.GetComponent<MonsterDamageBypass>();
            if (bypass != null)
            {
                bypass.ApplyDirectDamage(damage);
            }
            else
            {
                target.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }

    private void Start()
    {
        // กันลืมลบในกรณีที่ target หายไปแล้ว
        Destroy(gameObject, safetyLifetime);
    }
}
