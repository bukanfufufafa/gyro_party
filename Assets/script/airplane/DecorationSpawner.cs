using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerParent;

    [Header("Decoration Settings")]
    [SerializeField] private List<GameObject> decorationPrefabs;
    [SerializeField] private float spawnDistanceZ = 120f;
    [SerializeField] private float despawnDistanceBehind = 20f;

    [Header("Spawn Rate")]
    [SerializeField] private float minSpawnDelay = 3f;
    [SerializeField] private float maxSpawnDelay = 4f;

    [Header("Placement Zone")]
    [SerializeField] private float minRadius = 10f;
    [SerializeField] private float maxRadius = 30f;

    // UBAH: Default false
    private bool isSpawning = false;
    private List<GameObject> activeDecorations = new List<GameObject>();

    // Tambahan: Referensi coroutine agar bisa dihentikan secara spesifik
    private Coroutine spawnCoroutine;
    private Coroutine cleanupCoroutine;

    public void StartSpawning()
    {
        if (isSpawning) return;

        isSpawning = true;
        // Simpan referensinya
        spawnCoroutine = StartCoroutine(SpawnRoutine());
        cleanupCoroutine = StartCoroutine(CleanupRoutine());
    }

    // FUNGSI BARU: Untuk menghentikan siklus spawn dan cleanup
    public void StopSpawning()
    {
        if (!isSpawning) return;
        isSpawning = false;

        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        if (cleanupCoroutine != null) StopCoroutine(cleanupCoroutine);

        Debug.Log("Decoration Spawner Dihentikan.");
    }

    // Sisa kode di bawah ini SAMA PERSIS dengan sebelumnya
    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnDelay, maxSpawnDelay));
            SpawnDecoration();
        }
    }

    private void SpawnDecoration()
    {
        if (decorationPrefabs.Count == 0 || playerParent == null) return;

        float angle = Random.Range(0f, Mathf.PI * 2);
        float radius = Random.Range(minRadius, maxRadius);
        float offsetX = Mathf.Cos(angle) * radius;
        float offsetY = Mathf.Sin(angle) * radius;

        Vector3 spawnPos = new Vector3(
            playerParent.position.x + offsetX,
            playerParent.position.y + offsetY,
            playerParent.position.z + spawnDistanceZ
        );

        GameObject prefab = decorationPrefabs[Random.Range(0, decorationPrefabs.Count)];
        Quaternion randomRot = Quaternion.Euler(0, 0, Random.Range(0, 360));
        GameObject dec = Instantiate(prefab, spawnPos, randomRot);
        activeDecorations.Add(dec);
    }

    private IEnumerator CleanupRoutine()
    {
        while (true)
        {
            for (int i = activeDecorations.Count - 1; i >= 0; i--)
            {
                if (activeDecorations[i] == null)
                {
                    activeDecorations.RemoveAt(i);
                    continue;
                }

                if (activeDecorations[i].transform.position.z < playerParent.position.z - despawnDistanceBehind)
                {
                    Destroy(activeDecorations[i]);
                    activeDecorations.RemoveAt(i);
                }
            }
            yield return new WaitForSeconds(1.5f);
        }
    }
}