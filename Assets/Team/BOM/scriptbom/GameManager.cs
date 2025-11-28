using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// จัดการการเกิดมอน, Kill count, Boss spawn และคุยกับ Companion (แจก EXP ตอนมอนตาย)
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Spawning Settings")]
    public Monster activeMonster;

    [Header("Prefabs - Past Scene")]
    public MinionMonster minionPrefab1;
    public BossMonster bossPrefab1;

    [Header("Prefabs - Future Scene")]
    public MinionMonster minionPrefab2;
    public BossMonster bossPrefab2;

    [Header("Game Progression")]
    public int level = 1;
    public int minionsToKillForBoss = 5;

    [SerializeField] private int currentKillCount = 0;
    private bool isBossActive = false;

    [Header("Kill Count")]
    public int AllKillCount = 0;
    public bool canSpawn = true;

    // -------------------------------
    // COMPANIONS (Buddy System link)
    // -------------------------------
    [Header("Companions (Buddy System)")]
    [Tooltip("ลาก Companion ทั้งหมดที่อยากให้ได้ EXP ตอนมอนตายมาวางที่นี่")]
    public Companion[] companions;

    // ใช้ prefab ตาม scene ปัจจุบัน
    private MinionMonster activeMinionPrefab;
    private BossMonster activeBossPrefab;
    private Transform activeSpawnPoint;

    private void Start()
    {
        SetupSceneSpawning();
        SpawnMinion();
    }

    // ----------------------------------------------------------
    // Scene Setup (Using Tags)
    // ----------------------------------------------------------
    void SetupSceneSpawning()
    {
        string scene = SceneManager.GetActiveScene().name;

        if (scene == "PastScene")
        {
            GameObject sp = GameObject.FindGameObjectWithTag("PastSpawnPoint");
            if (sp != null) activeSpawnPoint = sp.transform;

            activeMinionPrefab = minionPrefab1;
            activeBossPrefab = bossPrefab1;
        }
        else if (scene == "FutureScene")
        {
            GameObject sp = GameObject.FindGameObjectWithTag("FutureSpawnPoint");
            if (sp != null) activeSpawnPoint = sp.transform;

            activeMinionPrefab = minionPrefab2;
            activeBossPrefab = bossPrefab2;
        }
        else
        {
            Debug.LogWarning("GameManager: Scene not recognized. Using PastScene defaults.");
            GameObject sp = GameObject.FindGameObjectWithTag("PastSpawnPoint");
            if (sp != null) activeSpawnPoint = sp.transform;

            activeMinionPrefab = minionPrefab1;
            activeBossPrefab = bossPrefab1;
        }

        if (activeSpawnPoint == null)
        {
            Debug.LogError("GameManager: activeSpawnPoint is null, check SpawnPoint tags.");
        }
    }

    // ----------------------------------------------------------
    // Monster Death
    // ----------------------------------------------------------
    public void OnMonsterDied(Monster monster)
    {
        if (monster == null)
        {
            Debug.LogWarning("GameManager: OnMonsterDied called with null Monster");
            return;
        }

        if (monster is BossMonster)
        {
            Debug.Log("GameManager: Boss Defeated! Level Up!");
            level++;
            currentKillCount = 0;
            isBossActive = false;
        }
        else
        {
            currentKillCount++;
            AllKillCount++;
            Debug.Log($"GameManager: Minion Defeated. WaveKill={currentKillCount}/{minionsToKillForBoss}, AllKill={AllKillCount}");
        }

        // ⭐ แจ้ง Companion ว่ามอนตายแล้ว → ได้ Friendship EXP เฉพาะตัวที่ Active
        GrantCompanionsKillExp();

        StartCoroutine(SpawnNextMonsterRoutine());
    }

    /// <summary>
    /// ให้ Companion ทุกตัวที่ active ได้ EXP จากการฆ่ามอน 1 ตัว
    /// </summary>
    private void GrantCompanionsKillExp()
    {
        if (companions == null || companions.Length == 0)
        {
            Debug.Log("GameManager: No companions assigned, skip EXP grant.");
            return;
        }

        for (int i = 0; i < companions.Length; i++)
        {
            Companion c = companions[i];
            if (c == null) continue;

            // ให้ EXP เฉพาะตัวที่เปิดอยู่จริง ๆ
            if (!c.isActiveAndEnabled)
            {
                Debug.Log($"GameManager: Companion[{c.name}] skipped EXP (isActiveAndEnabled == false)");
                continue;
            }

            if (!c.gameObject.activeInHierarchy)
            {
                Debug.Log($"GameManager: Companion[{c.name}] skipped EXP (not activeInHierarchy)");
                continue;
            }

            Debug.Log($"GameManager: Grant kill EXP to Companion[{c.name}]");
            c.OnMonsterKilled();
        }
    }

    IEnumerator SpawnNextMonsterRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        if (!isBossActive && currentKillCount >= minionsToKillForBoss)
            SpawnBoss();
        else
            SpawnMinion();
    }

    // ----------------------------------------------------------
    // Spawning
    // ----------------------------------------------------------
    void SpawnMinion()
    {
        if (!canSpawn) return;
        if (activeSpawnPoint == null) return;

        if (activeMonster != null)
            activeMonster.gameObject.SetActive(false);

        if (activeMinionPrefab == null)
        {
            Debug.LogError("GameManager: activeMinionPrefab is null");
            return;
        }

        // prefab ในซีน vs prefab จาก Project
        if (activeMinionPrefab.gameObject.scene.name == null)
            activeMonster = Instantiate(activeMinionPrefab, activeSpawnPoint.position, Quaternion.identity);
        else
            activeMonster = activeMinionPrefab;

        activeMonster.maxHealth = 100 * Mathf.Pow(1.2f, level - 1);
        activeMonster.ResetMonster();
        isBossActive = false;

        Debug.Log($"GameManager: Spawn Minion (Level {level}, HP {activeMonster.maxHealth})");
    }

    void SpawnBoss()
    {
        if (!canSpawn) return;
        if (activeSpawnPoint == null) return;

        if (activeMonster != null)
            activeMonster.gameObject.SetActive(false);

        if (activeBossPrefab == null)
        {
            Debug.LogError("GameManager: activeBossPrefab is null");
            return;
        }

        if (activeBossPrefab.gameObject.scene.name == null)
            activeMonster = Instantiate(activeBossPrefab, activeSpawnPoint.position, Quaternion.identity);
        else
            activeMonster = activeBossPrefab;

        activeMonster.maxHealth = (100 * Mathf.Pow(1.2f, level - 1)) * 5;

        BossMonster bossScript = activeMonster as BossMonster;
        if (bossScript != null)
        {
            bossScript.maxBreakGauge = 50 + (level * 10);
        }

        activeMonster.ResetMonster();
        isBossActive = true;

        Debug.Log($"GameManager: Spawn BOSS (Level {level}, HP {activeMonster.maxHealth})");
    }

    // ----------------------------------------------------------
    // Scene Events (Do not touch area, just added small debug)
    // ----------------------------------------------------------
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"GameManager: Scene Loaded -> {scene.name}");
        SetupSceneSpawning();
        currentKillCount = 0;
        isBossActive = false;

        if (canSpawn)
            SpawnMinion();
    }
}
