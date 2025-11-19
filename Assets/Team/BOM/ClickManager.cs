using UnityEngine;

public class ClickManager : MonoBehaviour
{
    public float clickDamage = 10f; // ดาเมจต่อการคลิก
    public LayerMask monsterLayer;  // กำหนด Layer ให้มอนสเตอร์

    [Header("Optional: Player Attack Animation")]
    [SerializeField] private Animator playerAnimator;
    private static readonly int AttackParam = Animator.StringToHash("Attack");

    void Update()
    {
        // ตรวจจับการกดเมาส์ซ้าย หรือ นิ้วแตะหน้าจอ
        if (Input.GetMouseButtonDown(0))
        {
            DetectClick();
        }
    }

    private void DetectClick()
    {
        // ยิง Raycast จากตำแหน่งเมาส์ ไปในโลกเกม
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, monsterLayer);

        if (hit.collider != null)
        {
            // ถ้าชนกับอะไรที่มีสคริปต์ Monster
            Monster monster = hit.collider.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(clickDamage);

                // เติมเกจอัลติ
                if (PlayerCombatAndUlt.Instance != null)
                {
                    PlayerCombatAndUlt.Instance.OnSuccessfulHit(clickDamage);
                }

                // เล่นอนิเมชันตีธรรมดา
                if (playerAnimator != null)
                {
                    playerAnimator.SetTrigger(AttackParam);
                }

                Debug.Log("Hit Monster!");
            }
        }
    }
}
