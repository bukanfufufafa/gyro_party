using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct StageConfig
{
    public string stageName;
    [Tooltip("Durasi stage berjalan (detik)")] public float duration;
    [Tooltip("Jeda minimal antar spawn")] public float minSpawnDelay;
    [Tooltip("Jeda maksimal antar spawn")] public float maxSpawnDelay;
    [Tooltip("Jumlah minimal obstacle dalam 1 waktu spawn")] public int minObstacles;
    [Tooltip("Jumlah maksimal obstacle dalam 1 waktu spawn")] public int maxObstacles;
}

public class LevelManager : MonoBehaviour
{

    [Header("References")]
    [Tooltip("Masukkan Parent GameObject dari Pesawat agar posisi Z sinkron.")]
    [SerializeField] private Transform playerParent;

    [Header("Obstacle Settings")]
    [SerializeField] private List<GameObject> obstaclePrefabs;
    [Tooltip("Jarak Z di depan pesawat untuk spawn obstacle")]
    [SerializeField] private float spawnDistanceZ = 100f;
    [Tooltip("Batas jarak di belakang pesawat sebelum obstacle dihancurkan")]
    [SerializeField] private float despawnDistanceBehind = 15f;
    
    [Header("Spawn Points (Koordinat X, Y)")]
    [SerializeField] private Vector2 topLeft = new Vector2(-5f, 5f);
    [SerializeField] private Vector2 topRight = new Vector2(5f, 5f);
    [SerializeField] private Vector2 bottomLeft = new Vector2(-5f, -5f);
    [SerializeField] private Vector2 bottomRight = new Vector2(5f, -5f);

    [Header("Stage Settings")]
    [SerializeField] private float initialDelay = 4f;
    [SerializeField] private List<StageConfig> stages = new List<StageConfig>()
    {
        new StageConfig { stageName = "Stage 1", duration = 8f, minSpawnDelay = 3f, maxSpawnDelay = 3f, minObstacles = 1, maxObstacles = 1 },
        new StageConfig { stageName = "Stage 2", duration = 10f, minSpawnDelay = 2f, maxSpawnDelay = 3f, minObstacles = 1, maxObstacles = 1 },
        new StageConfig { stageName = "Stage 3", duration = 6f, minSpawnDelay = 2f, maxSpawnDelay = 2f, minObstacles = 1, maxObstacles = 2 },
        new StageConfig { stageName = "Stage 4", duration = 6f, minSpawnDelay = 1f, maxSpawnDelay = 1.5f, minObstacles = 1, maxObstacles = 2 }
    };

    [Header("Events (Callbacks)")]
    public UnityEvent OnGameStarted;
    public UnityEvent OnGameOver;
    
    [Tooltip("Dipanggil saat seluruh stage selesai (Game Win).")]
    public UnityEvent OnGameCompleted; 
    
    [Tooltip("Mengirim nilai 0.0 hingga 1.0 secara real-time. Cocok untuk UI Slider.")]
    public UnityEvent<float> OnProgressUpdated;

    private bool isGameOver = false;
    private bool hasStarted = false; 
    private List<GameObject> activeObstacles = new List<GameObject>();

    private Coroutine stageCoroutine;
    private Coroutine cleanupCoroutine;

    // Variabel internal untuk kalkulasi progress
    private float totalGameDuration = 0f;
    private float currentElapsedTime = 0f;


    /// <summary>
    /// Memulai permainan, menjalankan pesawat, dan memulai siklus stage.
    /// </summary>
    public void StartGame()
    {
        if (hasStarted) return;
        hasStarted = true;
        isGameOver = false;
        currentElapsedTime = 0f;

        // Kalkulasi total waktu game
        totalGameDuration = initialDelay;
        foreach (var stage in stages)
        {
            totalGameDuration += stage.duration;
        }

        Debug.Log($"Game Dimulai! Total durasi game: {totalGameDuration} detik.");

        if (playerParent != null)
        {
            ForwardMover mover = playerParent.GetComponent<ForwardMover>();
            if (mover != null) mover.StartMovement();
        }

        DecorationSpawner decSpawner = GetComponent<DecorationSpawner>();
        if (decSpawner != null) decSpawner.StartSpawning();

        stageCoroutine = StartCoroutine(StageRoutine());
        cleanupCoroutine = StartCoroutine(CleanupRoutine());

        OnGameStarted?.Invoke();
        OnProgressUpdated?.Invoke(0f);
    }

    /// <summary>
    /// Menghentikan seluruh proses game secara bersih (bisa untuk Pause, GameOver, atau Win).
    /// </summary>
    public void StopGame()
    {
        if (!hasStarted) return;
        hasStarted = false; 

        if (playerParent != null)
        {
            ForwardMover mover = playerParent.GetComponent<ForwardMover>();
            if (mover != null) mover.StopMovement();
        }

        DecorationSpawner decSpawner = GetComponent<DecorationSpawner>();
        if (decSpawner != null) decSpawner.StopSpawning();

        if (stageCoroutine != null) StopCoroutine(stageCoroutine);
        if (cleanupCoroutine != null) StopCoroutine(cleanupCoroutine);
    }

    private IEnumerator StageRoutine()
    {
        // 1. Fase Jeda Awal
        float delayTimer = 0f;
        while (delayTimer < initialDelay && !isGameOver && hasStarted)
        {
            delayTimer += Time.deltaTime;
            currentElapsedTime += Time.deltaTime;
            
            OnProgressUpdated?.Invoke(currentElapsedTime / totalGameDuration);
            yield return null;
        }

        // 2. Eksekusi tiap stage berurutan
        for (int i = 0; i < stages.Count; i++)
        {
            if (isGameOver || !hasStarted) yield break;

            StageConfig currentStage = stages[i];
            float stageTimer = 0f;
            float nextSpawnTimer = 0f;

            while (stageTimer < currentStage.duration && !isGameOver && hasStarted)
            {
                stageTimer += Time.deltaTime;
                currentElapsedTime += Time.deltaTime;
                nextSpawnTimer -= Time.deltaTime;

                if (nextSpawnTimer <= 0f)
                {
                    SpawnObstacles(currentStage);
                    nextSpawnTimer = Random.Range(currentStage.minSpawnDelay, currentStage.maxSpawnDelay);
                }

                // Kalkulasi progress (0.0 s/d 1.0)
                float progress = Mathf.Clamp01(currentElapsedTime / totalGameDuration);
                OnProgressUpdated?.Invoke(progress);

                yield return null;
            }
        }
        
        // 3. Pengecekan Akhir (Level Selesai)
        if (!isGameOver && hasStarted)
        {
            OnProgressUpdated?.Invoke(1f); 
            Debug.Log("Semua Stage Selesai! Level Completed.");
            
            OnGameCompleted?.Invoke(); 
            StopGame(); 
        }
    }

    private void SpawnObstacles(StageConfig config)
    {
        if (obstaclePrefabs.Count == 0 || playerParent == null) return;

        int spawnCount = Random.Range(config.minObstacles, config.maxObstacles + 1);
        List<Vector2> availablePositions = new List<Vector2> { topLeft, topRight, bottomLeft, bottomRight };
        
        for (int i = 0; i < spawnCount; i++)
        {
            if (availablePositions.Count == 0) break;

            int posIndex = Random.Range(0, availablePositions.Count);
            Vector2 selectedOffset = availablePositions[posIndex];
            availablePositions.RemoveAt(posIndex); 

            Vector3 spawnPos = new Vector3(
                playerParent.position.x + selectedOffset.x,
                playerParent.position.y + selectedOffset.y,
                playerParent.position.z + spawnDistanceZ
            );

            GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)];
            GameObject obs = Instantiate(prefab, spawnPos, Quaternion.identity);
            
            activeObstacles.Add(obs);
        }
    }

    private IEnumerator CleanupRoutine()
    {
        while (true)
        {
            for (int i = activeObstacles.Count - 1; i >= 0; i--)
            {
                if (activeObstacles[i] == null)
                {
                    activeObstacles.RemoveAt(i);
                    continue;
                }

                if (activeObstacles[i].transform.position.z < playerParent.position.z - despawnDistanceBehind)
                {
                    Destroy(activeObstacles[i]);
                    activeObstacles.RemoveAt(i);
                }
            }
            yield return new WaitForSeconds(1f); 
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        
        Debug.Log("Game Over!");
        OnGameOver?.Invoke();
        StopGame(); 
    }
}