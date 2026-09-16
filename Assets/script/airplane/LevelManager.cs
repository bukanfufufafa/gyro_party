using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct StageConfig
{
    public string stageName;
    public float duration;
    public float minSpawnDelay;
    public float maxSpawnDelay;
    public int minObstacles;
    public int maxObstacles;
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform playerParent;

    [Header("Obstacle Settings")]
    [SerializeField] private List<GameObject> obstaclePrefabs;
    [SerializeField] private float spawnDistanceZ = 100f;
    [SerializeField] private float despawnDistanceBehind = 15f;

    [Header("Spawn Points (Koordinat X, Y)")]
    [SerializeField] private Vector2 topLeft = new Vector2(-5f, 5f);
    [SerializeField] private Vector2 topRight = new Vector2(5f, 5f);
    [SerializeField] private Vector2 bottomLeft = new Vector2(-5f, -5f);
    [SerializeField] private Vector2 bottomRight = new Vector2(5f, -5f);

    [Header("Stage Settings")]
    [SerializeField] private float initialDelay = 4f;
    [SerializeField]
    private List<StageConfig> stages = new List<StageConfig>()
    {
        new StageConfig { stageName = "Stage 1", duration = 8f, minSpawnDelay = 3f, maxSpawnDelay = 3f, minObstacles = 1, maxObstacles = 1 },
        new StageConfig { stageName = "Stage 2", duration = 10f, minSpawnDelay = 2f, maxSpawnDelay = 3f, minObstacles = 1, maxObstacles = 1 },
        new StageConfig { stageName = "Stage 3", duration = 6f, minSpawnDelay = 2f, maxSpawnDelay = 2f, minObstacles = 1, maxObstacles = 2 },
        new StageConfig { stageName = "Stage 4", duration = 6f, minSpawnDelay = 1f, maxSpawnDelay = 1.5f, minObstacles = 1, maxObstacles = 2 }
    };

    [Header("Events")]
    public UnityEvent OnGameOver;
    // Tambahan event jika butuh trigger UI (opsional)
    public UnityEvent OnGameStarted;

    private bool isGameOver = false;
    private bool hasStarted = false; // Flag tambahan agar StartGame tidak bisa dipanggil 2x
    private List<GameObject> activeObstacles = new List<GameObject>();

    // Tambahan: Referensi coroutine
    private Coroutine stageCoroutine;
    private Coroutine cleanupCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // UBAH: Fungsi Start() dihapus / dikosongkan.

    /// <summary>
    /// Panggil fungsi ini dari UI Button (Play) atau script Main Menu Anda.
    /// </summary>
    public void StartGame()
    {
        if (hasStarted) return;
        hasStarted = true;

        Debug.Log("Game Dimulai!");

        // 1. Jalankan pergerakan pesawat (ForwardMover)
        if (playerParent != null)
        {
            ForwardMover mover = playerParent.GetComponent<ForwardMover>();
            if (mover != null) mover.StartMovement();
        }

        // 2. Jalankan spawner dekorasi (Asumsi terpasang di GameObject yang sama)
        DecorationSpawner decSpawner = GetComponent<DecorationSpawner>();
        if (decSpawner != null) decSpawner.StartSpawning();

        // 3. Mulai siklus Stage & pembersihan memori Obstacle
        StartCoroutine(StageRoutine());
        StartCoroutine(CleanupRoutine());

        // Trigger event (jika UI mau bereaksi, misal menghilangkan menu utama)
        OnGameStarted?.Invoke();
    }

    private IEnumerator StageRoutine()
    {
        // Jeda awal 4 detik
        yield return new WaitForSeconds(initialDelay);

        // Eksekusi tiap stage berurutan
        for (int i = 0; i < stages.Count; i++)
        {
            if (isGameOver) yield break;

            StageConfig currentStage = stages[i];
            Debug.Log($"Memulai: {currentStage.stageName}");

            float stageTimer = 0f;
            float nextSpawnTimer = 0f;

            // Loop selama durasi stage belum habis
            while (stageTimer < currentStage.duration && !isGameOver)
            {
                stageTimer += Time.deltaTime;
                nextSpawnTimer -= Time.deltaTime;

                if (nextSpawnTimer <= 0f)
                {
                    SpawnObstacles(currentStage);
                    nextSpawnTimer = Random.Range(currentStage.minSpawnDelay, currentStage.maxSpawnDelay);
                }

                yield return null;
            }
        }

        Debug.Log("Semua Stage Selesai! Anda bisa me-loop stage terakhir atau menang di sini.");
    }

    private void SpawnObstacles(StageConfig config)
    {
        if (obstaclePrefabs.Count == 0 || playerParent == null) return;

        int spawnCount = Random.Range(config.minObstacles, config.maxObstacles + 1);

        // Buat list posisi yang tersedia agar 2 obstacle tidak menumpuk di 1 titik
        List<Vector2> availablePositions = new List<Vector2> { topLeft, topRight, bottomLeft, bottomRight };

        for (int i = 0; i < spawnCount; i++)
        {
            if (availablePositions.Count == 0) break;

            // Pilih posisi acak
            int posIndex = Random.Range(0, availablePositions.Count);
            Vector2 selectedOffset = availablePositions[posIndex];
            availablePositions.RemoveAt(posIndex); // Hapus agar tidak terpilih lagi di loop yang sama

            // Tentukan posisi 3D
            Vector3 spawnPos = new Vector3(
                playerParent.position.x + selectedOffset.x,
                playerParent.position.y + selectedOffset.y,
                playerParent.position.z + spawnDistanceZ
            );

            // Pilih prefab acak dan Spawn
            GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)];
            GameObject obs = Instantiate(prefab, spawnPos, Quaternion.identity);

            activeObstacles.Add(obs);
        }
    }

    // Coroutine khusus untuk membersihkan memory (dipanggil rutin setiap 1 detik, lebih hemat performa dari Update)
    private IEnumerator CleanupRoutine()
    {
        while (!isGameOver)
        {
            for (int i = activeObstacles.Count - 1; i >= 0; i--)
            {
                if (activeObstacles[i] == null)
                {
                    activeObstacles.RemoveAt(i);
                    continue;
                }

                // Jika posisi Z obstacle lebih kecil dari (posisi Z pesawat - jarak despawn)
                if (activeObstacles[i].transform.position.z < playerParent.position.z - despawnDistanceBehind)
                {
                    Destroy(activeObstacles[i]);
                    activeObstacles.RemoveAt(i);
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// FUNGSI BARU: Menghentikan seluruh proses game secara bersih.
    /// Bisa dipanggil saat pause, pindah scene, atau game over.
    /// </summary>
    public void StopGame()
    {
        if (!hasStarted) return;
        hasStarted = false; // Reset state agar bisa dipanggil StartGame() lagi jika ingin dibuat sistem Resume

        Debug.Log("Game Dihentikan (Stopped)!");

        // 1. Hentikan laju pesawat
        if (playerParent != null)
        {
            ForwardMover mover = playerParent.GetComponent<ForwardMover>();
            if (mover != null) mover.StopMovement();
        }

        // 2. Hentikan spawner dekorasi
        DecorationSpawner decSpawner = GetComponent<DecorationSpawner>();
        if (decSpawner != null) decSpawner.StopSpawning();

        // 3. Hentikan coroutine Level Manager
        if (stageCoroutine != null) StopCoroutine(stageCoroutine);
        if (cleanupCoroutine != null) StopCoroutine(cleanupCoroutine);
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("Game Over!");
        OnGameOver?.Invoke();

        // Gunakan StopGame untuk mematikan semua sistem karena logic-nya sama
        StopGame();
    }
}