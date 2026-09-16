using System.Collections;
using UnityEngine;

public class fruitspawner : MonoBehaviour
{
    [Header("Objects")]
    public GameObject[] objectPrefabs;

    [Header("Spawner")]
    public fruitspawner otherSpawner;

    [Header("Target")]
    public Transform target;

    [Header("Spawn")]
    public float spawnInterval = 1f;

    [Header("Movement")]
    public float moveSpeed = 3f;

    private void Start()
    {
        // Hanya spawner utama yang menjalankan random
        if (otherSpawner != null && transform.GetInstanceID() > otherSpawner.transform.GetInstanceID())
            return;

        StartCoroutine(SpawnObject());
    }

    IEnumerator SpawnObject()
    {
        while (true)
        {
            SpawnPair();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnPair()
    {
        // Pilih SATU objek secara random
        int randomIndex = Random.Range(0, objectPrefabs.Length);

        // Spawn untuk spawner ini
        Spawn(objectPrefabs[randomIndex]);

        // Spawn untuk spawner satunya
        if (otherSpawner != null)
        {
            otherSpawner.Spawn(objectPrefabs[randomIndex]);
        }
    }

    void Spawn(GameObject prefab)
    {
        GameObject obj = Instantiate(
            prefab,
            transform.position,
            Quaternion.identity
        );

        StartCoroutine(MoveObject(obj));
    }

    IEnumerator MoveObject(GameObject obj)
    {
        while (obj != null)
        {
            float targetX = target.position.x;

            Vector3 newPosition = obj.transform.position;

            newPosition.x = Mathf.MoveTowards(
                newPosition.x,
                targetX,
                moveSpeed * Time.deltaTime
            );

            obj.transform.position = newPosition;

            if (Mathf.Approximately(obj.transform.position.x, targetX))
            {
                Destroy(obj);
                yield break;
            }

            yield return null;
        }
    }
}