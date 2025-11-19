using UnityEngine;

public class ClickManager : MonoBehaviour
{
    public float clickDamage = 10f;      // ดาเมจต่อการคลิก
    public LayerMask monsterLayer;       // กำหนด Layer ให้มอนสเตอร์

    private Camera mainCam;              // cache กล้องหลัก

    private void Awake()
    {
        mainCam = Camera.main;

        if (mainCam == null)
        {
            Debug.LogError("ClickManager: ไม่เจอ Camera ที่ Tag = MainCamera ในฉาก!");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DetectClick();
        }
    }

    private void DetectClick()
    {
        // ถ้ายังไม่มีกล้องก็ไม่ต้องทำอะไร
        if (mainCam == null)
        {
            Debug.LogError("ClickManager: mainCam เป็น null (เช็ค Tag MainCamera ด้วย)");
            return;
        }

        // แปลงตำแหน่งเมาส์มาเป็นตำแหน่งในโลกเกม
        Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        // ยิง Raycast เฉพาะ Layer ของมอนสเตอร์
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, monsterLayer);

        if (hit.collider == null)
        {
            // กดโดนที่ว่าง ๆ
            // Debug.Log("ClickManager: ไม่โดนอะไร");
            return;
        }

        // ถ้าชนกับอะไรที่มีสคริปต์ Monster
        Monster monster = hit.collider.GetComponent<Monster>();
        if (monster == null)
        {
            Debug.Log("ClickManager: โดน " + hit.collider.name + " แต่ไม่มีคอมโพเนนต์ Monster");
            return;
        }

        // โดนมอนสเตอร์จริง ๆ
        monster.TakeDamage(clickDamage);
        Debug.Log("Hit Monster! " + hit.collider.name);
    }
}
