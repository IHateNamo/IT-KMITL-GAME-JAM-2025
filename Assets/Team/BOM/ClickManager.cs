using UnityEngine;

public class ClickManager : MonoBehaviour
{
    [Header("Progression")]
    public int currentClicks = 0;
    public int clicksToUlt = 100; 
    
    [Header("References")]
    public UltimateSkill ultimateSkill; 
    public LayerMask monsterLayer;
    
    // แก้ตรงนี้: จาก normalDamage เป็น clickDamage เพื่อให้ StatusCalulate เรียกใช้ได้
    public float clickDamage = 10f; 

    // Cache Camera
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null) mainCamera = Object.FindFirstObjectByType<Camera>();
    }

    void Update()
    {
        if (ultimateSkill != null && ultimateSkill.isActiveAndEnabled) return;

        if (Input.GetMouseButtonDown(0))
        {
            DetectClick();
        }
    }

    private void DetectClick()
    {
        if (mainCamera == null) return;

        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, monsterLayer);

        if (hit.collider != null)
        {
            Monster monster = hit.collider.GetComponent<Monster>();
            if (monster != null)
            {
                // 1. ตีธรรมดา (ใช้ clickDamage)
                monster.TakeDamage(clickDamage);
                Debug.Log("Hit Monster! Click: " + currentClicks);

                // 2. นับจำนวนคลิก
                currentClicks++;

                // 3. เช็คว่าครบ 100 หรือยัง
                if (currentClicks >= clicksToUlt)
                {
                    ActivateUltimate();
                    currentClicks = 0; 
                }
            }
        }
    }

    void ActivateUltimate()
    {
        Debug.Log("⚡ ULTIMATE READY! ⚡");
        if (ultimateSkill != null)
        {
            ultimateSkill.StartUltimateDuration();
        }
    }
}