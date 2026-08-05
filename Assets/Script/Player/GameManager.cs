using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PhotonGameManager : MonoBehaviourPun
{
    [SerializeField] private int maxScore = 99999;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text enemyCountText;
    [SerializeField] private FirstPersonMovement firstPerson;
    [SerializeField] private FirstPersonGunController gunController;
    [SerializeField] private Text centerText;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private EnemySpawner[] spawners;

    [SerializeField] private int clearEnemyCount = 5;

    private int defeatedEnemyCount = 0;

    [System.NonSerialized] public bool gameOver = false;

    private int score = 0;
    public int Score
    {
        get => score;
        set
        {
            score = Mathf.Clamp(value, 0, maxScore);
            if (scoreText != null)
                scoreText.text = score.ToString("D8");

            // 他クライアントにも同期
            if (photonView.IsMine)
            {
                photonView.RPC("RPC_UpdateScore", RpcTarget.Others, score);
            }
        }
    }

    private void Start()
    {
        UpdateEnemyCount();
    }

    public IEnumerator GameStart()
    {
        if (photonView.IsMine) SetSpawners(true);
        yield return new WaitForSeconds(1);
    }

    public IEnumerator GameOver()
    {
        if (!photonView.IsMine) yield break; // 他クライアントは処理しない

        gameOver = true;

        //firstPerson.playerCanMove = false;
        //firstPerson.enableCameraMovement = false;
        //gunController.shootEnabled = false;

        SetSpawners(false);

        if (centerText != null)
        {
            centerText.enabled = true;
            centerText.text = "Game Over";
        }

        yield return new WaitForSeconds(waitTime);

        DestroyEnemies();

        if (centerText != null)
        {
            centerText.text = "";
            centerText.enabled = false;
        }

        gameOver = false;
    }

    private void DestroyEnemies()
    {
        if (!photonView.IsMine) return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            if (enemy.TryGetComponent<PhotonView>(out PhotonView pv) && pv.IsMine)
            {
                PhotonNetwork.Destroy(enemy);
            }
        }

        UpdateEnemyCount();
    }

    private void SetSpawners(bool isEnable)
    {
        if (!photonView.IsMine) return;

        if (spawners == null || spawners.Length == 0)
        {
            Debug.LogWarning("EnemySpawner 配列が設定されていません！");
            return;
        }

        foreach (EnemySpawner spawner in spawners)
        {
            spawner.spawnEnabled = isEnable;
        }
    }

    private void Update()
    {
        UpdateEnemyCount();
    }

    private void UpdateEnemyCount()
    {
        int enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

        if (enemyCountText != null)
            enemyCountText.text = "残数: " + enemyCount;
    }

    [PunRPC]
    void RPC_UpdateScore(int newScore)
    {
        score = newScore;
        if (scoreText != null)
            scoreText.text = score.ToString("D8");
    }

    public void EnemyDefeated()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        defeatedEnemyCount++;

        Debug.Log($"撃破数：{defeatedEnemyCount}");

        if (defeatedEnemyCount >= clearEnemyCount)
        {
            SetSpawners(false);

            StopAllCoroutines();
        }
    }
}
