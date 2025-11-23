using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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

    // Active set chosen by scene
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
            activeSpawnPoint = GameObject.FindGameObjectWithTag("PastSpawnPoint").transform;
            activeMinionPrefab = minionPrefab1;
            activeBossPrefab = bossPrefab1;
        }
        else if (scene == "FutureScene")
        {
            activeSpawnPoint = GameObject.FindGameObjectWithTag("FutureSpawnPoint").transform;
            activeMinionPrefab = minionPrefab2;
            activeBossPrefab = bossPrefab2;
        }
        else
        {
            Debug.LogWarning("Scene not recognized. Using PastScene defaults.");
            activeSpawnPoint = GameObject.FindGameObjectWithTag("PastSpawnPoint").transform;
            activeMinionPrefab = minionPrefab1;
            activeBossPrefab = bossPrefab1;
        }
    }

    // ----------------------------------------------------------
    // Monster Death
    // ----------------------------------------------------------
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
            AllKillCount++;
        }

        StartCoroutine(SpawnNextMonsterRoutine());
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
        if (activeMonster != null) activeMonster.gameObject.SetActive(false);

        if (activeMinionPrefab.gameObject.scene.name == null)
            activeMonster = Instantiate(activeMinionPrefab, activeSpawnPoint.position, Quaternion.identity);
        else
            activeMonster = activeMinionPrefab;

        activeMonster.maxHealth = 100 * Mathf.Pow(1.2f, level - 1);
        activeMonster.ResetMonster();
        isBossActive = false;
    }

    void SpawnBoss()
    {
        if (!canSpawn) return;
        if (activeMonster != null) activeMonster.gameObject.SetActive(false);

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
    }

    //Do not touch
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
        SetupSceneSpawning();
        currentKillCount = 0;
        isBossActive = false;

        // Force new spawn after scene change
        if (canSpawn)
            SpawnMinion();
    }
}