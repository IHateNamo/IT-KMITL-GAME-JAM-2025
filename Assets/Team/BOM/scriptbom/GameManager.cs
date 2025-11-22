using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Spawning Settings")]
    public Monster activeMonster; 
    
    [Header("Prefabs")]
    public MinionMonster minionPrefab; 
    public BossMonster bossPrefab;     
    public Transform spawnPoint;

    [Header("Game Progression")]
    public int level = 1;
    public int minionsToKillForBoss = 5; // Adjust this in Inspector
    
    [SerializeField] private int currentKillCount = 0;
    private bool isBossActive = false;

    private void Start()
    {
        SpawnMinion();
    }

    public void OnMonsterDied(Monster monster)
    {
        if (monster is BossMonster)
        {
            Debug.Log("Boss Defeated! Level Up!");
            level++;
            currentKillCount = 0; 
            isBossActive = false;
        }
        else
        {
            Debug.Log("Minion Defeated.");
            currentKillCount++;
        }

        StartCoroutine(SpawnNextMonsterRoutine());
    }

    IEnumerator SpawnNextMonsterRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        // Check if we reached the kill count for Boss
        if (!isBossActive && currentKillCount >= minionsToKillForBoss)
        {
            SpawnBoss();
        }
        else
        {
            SpawnMinion();
        }
    }

    void SpawnMinion()
    {
        if (activeMonster != null) activeMonster.gameObject.SetActive(false);

        // Simple instantiation logic
        if (minionPrefab.gameObject.scene.name == null) 
        {
             activeMonster = Instantiate(minionPrefab, spawnPoint.position, Quaternion.identity);
        }
        else
        {
             activeMonster = minionPrefab;
        }

        // Stats Logic
        activeMonster.maxHealth = 100 * Mathf.Pow(1.2f, level - 1);
        activeMonster.ResetMonster();
        
        isBossActive = false;
    }

    void SpawnBoss()
    {
        if (activeMonster != null) activeMonster.gameObject.SetActive(false);

        if (bossPrefab.gameObject.scene.name == null)
        {
             activeMonster = Instantiate(bossPrefab, spawnPoint.position, Quaternion.identity);
        }
        else
        {
             activeMonster = bossPrefab;
        }

        // Boss Logic: 5x HP
        activeMonster.maxHealth = (100 * Mathf.Pow(1.2f, level - 1)) * 5; 
        
        BossMonster bossScript = activeMonster as BossMonster;
        if(bossScript != null)
        {
            bossScript.maxBreakGauge = 50 + (level * 10);
        }

        activeMonster.ResetMonster();
        isBossActive = true;
    }
}