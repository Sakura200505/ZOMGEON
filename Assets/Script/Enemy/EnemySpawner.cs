using Photon.Pun;
using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public bool spawnEnabled = false;

    [SerializeField] private int maxEnemies = 10;
    [SerializeField] private float minSpawnInterval = 1;
    [SerializeField] private float maxSpawnInterval = 3;
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    private bool spawning = false;

    private void Start()
    {
        Debug.Log("EnemySpawnerãNìÆ");
    }

    void Update()
    {
        // MasterClientÇæÇØÇ™ìGÇê∂ê¨Ç∑ÇÈ
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (spawnEnabled)
        {
            StartCoroutine(SpawnTimer());
        }
    }

    IEnumerator SpawnTimer()
    {
        if (spawning)
            yield break;

        if (SpawnEnemy())
        {
            spawning = true;

            float interval = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(interval);

            spawning = false;
        }
    }

    bool SpawnEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length >= maxEnemies)
            return false;

        int choosedIndex = Random.Range(0, enemyPrefabs.Length);

        int spawnIndex = Random.Range(0, spawnPoints.Length);

        Vector3 position = spawnPoints[spawnIndex].position;

        // PhotonÇ≈ìGÇê∂ê¨
        PhotonNetwork.Instantiate(
            enemyPrefabs[choosedIndex].name,
            position,
            Quaternion.identity);

        return true;
    }
}