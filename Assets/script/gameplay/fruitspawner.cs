using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fruitspawner : MonoBehaviour
{
    [Header("Objects")]
    public GameObject[] objectPrefabs;
    public int maxObjects = 10;

    [Header("Target")]
    public Transform target;

    [Header("Spawn")]
    public float spawnInterval = 1f;

    [Header("Movement")]
    public float moveSpeed = 3f;

    private List<GameObject> objectPool = new List<GameObject>();

    void Start()
    {
        // Membuat object pool dari semua prefab
        for (int i = 0; i < maxObjects; i++)
        {
            // Pilih prefab secara random
            GameObject randomPrefab = objectPrefabs[Random.Range(0, objectPrefabs.Length)];

            GameObject obj = Instantiate(randomPrefab);
            obj.SetActive(false);

            objectPool.Add(obj);
        }

        StartCoroutine(SpawnObject());
    }

    IEnumerator SpawnObject()
    {
        while (true)
        {
            Spawn();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void Spawn()
    {
        foreach (GameObject obj in objectPool)
        {
            if (!obj.activeSelf)
            {
                Fruit fruit = obj.GetComponent<Fruit>();

                if (fruit != null)
                {
                    fruit.ResetFruit();
                }

                // Pindahkan buah ke posisi spawner
                obj.transform.position = transform.position;

                // Aktifkan buah
                obj.SetActive(true);

                return;
            }
        }
    }

    void Update()
    {
        foreach (GameObject obj in objectPool)
        {
            if (!obj.activeSelf)
                continue;

            // Target hanya menggunakan posisi X
            float targetX = target.position.x;

            // Bergerak hanya pada sumbu X
            Vector3 newPosition = obj.transform.position;

            newPosition.x = Mathf.MoveTowards(
                newPosition.x,
                targetX,
                moveSpeed * Time.deltaTime
            );

            obj.transform.position = newPosition;

            // Jika sudah sampai target
            if (Mathf.Approximately(obj.transform.position.x, targetX))
            {
                obj.SetActive(false);
            }
        }
    }
}