using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public Monster activeMonster; // ลากตัว Monster ในฉากมาใส่
    public int level = 1;
    
    // สูตรคำนวณเลือดมอนสเตอร์ (เพิ่มขึ้นทีละ 1.5 เท่า หรือตามสูตรที่ชอบ)
    public void OnMonsterDied()
    {
        Debug.Log("Monster Died! Preparing next level...");
        
        // เพิ่ม Level
        level++;

        // รอแป๊บหนึ่งแล้วเสกตัวใหม่ (Coroutines)
        StartCoroutine(SpawnNextMonster());
    }

    IEnumerator SpawnNextMonster()
    {
        yield return new WaitForSeconds(0.5f); // รอ 0.5 วินาที

        // เพิ่มเลือดมอนสเตอร์ตามเลเวล (สูตรสมมติ)
        activeMonster.maxHealth = 100 * Mathf.Pow(1.2f, level - 1);
        
        // รีเซ็ตมอนสเตอร์
        activeMonster.ResetMonster();
    }
}