using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UltimateSkill : MonoBehaviour
{
    [Header("Settings")]
    public float ultDuration = 5.0f;     // ระยะเวลา 5 วิ
    public float timeScaleAmount = 0.3f; // สโลว์เวลา
    public float ultDamage = 50f;        // ดาเมจตอนฟัน
    public LayerMask monsterLayer;       // Layer เดียวกับ ClickManager
    public float sampleDistance = 0.1f;

    // ตัวแปรภายใน
    private bool isDragging = false;
    private Vector3 lastPos;
    private Camera mainCamera;
    private HashSet<GameObject> hitEnemiesInStroke = new HashSet<GameObject>();

    void Start()
    {
        // หา Camera
        mainCamera = Camera.main;
        if (mainCamera == null) mainCamera = Object.FindFirstObjectByType<Camera>();

        // *สำคัญ* ปิดตัวเองตอนเริ่มเกม 
        // (เพื่อให้ ClickManager เช็ค isActiveAndEnabled = false แล้วนับคลิกได้)
        this.enabled = false;
    }

    // ฟังก์ชันนี้ถูก ClickManager เรียกเมื่อครบ 100 คลิก
    public void StartUltimateDuration()
    {
        this.enabled = true; // เปิดใช้งาน (ClickManager จะหยุดนับคลิกทันที)
        StartCoroutine(UltimateRoutine());
    }

    IEnumerator UltimateRoutine()
    {
        // เริ่ม Slow Motion
        Time.timeScale = timeScaleAmount;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        Debug.Log("⚡ SLICING MODE START! ⚡");

        // รอ 5 วินาที (Realtime)
        yield return new WaitForSecondsRealtime(ultDuration);

        // จบเวลา: คืนค่าปกติ
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
        
        isDragging = false;
        Debug.Log("⌛ End Ultimate.");

        // *สำคัญ* ปิดตัวเองเมื่อจบ 
        // (ClickManager จะกลับมาทำงานและเริ่มนับ 1 ใหม่)
        this.enabled = false;
    }

    void Update()
    {
        // ถ้าเข้ามาทำงานใน Update ได้ แปลว่า enabled = true แล้ว (กำลังใช้อัลติ)
        
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastPos = GetMouseWorld();
            hitEnemiesInStroke.Clear();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            // Logic การลากเส้นฟัน (เหมือนเดิม)
            Vector3 cur = GetMouseWorld();
            float dist = Vector3.Distance(lastPos, cur);

            if (dist > 0f)
            {
                int steps = Mathf.CeilToInt(dist / sampleDistance);
                for (int i = 0; i < steps; i++)
                {
                    Vector3 a = Vector3.Lerp(lastPos, cur, (float)i / steps);
                    Vector3 b = Vector3.Lerp(lastPos, cur, (float)(i + 1) / steps);

                    RaycastHit2D hit = Physics2D.Linecast(a, b, monsterLayer);
                    if (hit.collider != null)
                    {
                        GameObject obj = hit.collider.gameObject;
                        // ฟันตัวเดิมได้แค่ครั้งเดียวต่อการลาก 1 ครั้ง
                        if (!hitEnemiesInStroke.Contains(obj))
                        {
                            Monster monster = obj.GetComponent<Monster>();
                            if (monster != null)
                            {
                                monster.TakeDamage(ultDamage);
                            }
                            hitEnemiesInStroke.Add(obj);
                        }
                    }
                }
                lastPos = cur;
            }
        }
    }

    private Vector3 GetMouseWorld()
    {
        if (mainCamera == null) return Vector3.zero;
        Vector3 p = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        p.z = 0;
        return p;
    }
}