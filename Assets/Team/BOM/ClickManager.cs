using UnityEngine;

public class ClickManager : MonoBehaviour
{
    public float clickDamage = 10f; // ดาเมจต่อการคลิก
    public LayerMask monsterLayer;  // กำหนด Layer ให้มอนสเตอร์

    void Update()
    {
        // ตรวจจับการกดเมาส์ซ้าย หรือ นิ้วแตะหน้าจอ
        if (Input.GetMouseButtonDown(0))
        {
            DetectClick();
        }
    }

    void DetectClick()
    {
        // ยิง Raycast จากตำแหน่งเมาส์/นิ้ว ไปในโลกเกม
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, monsterLayer);

        if (hit.collider != null)
        {
            // ถ้าชนกับอะไรที่มีสคริปต์ Monster
            Monster monster = hit.collider.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(clickDamage);
                Debug.Log("Hit Monster!");
            }
        }
    }
}