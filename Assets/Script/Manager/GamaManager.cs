using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("スコア")]
    [SerializeField] private int maxScore = 99999;
    [SerializeField] private Text scoreText;

    [Header("敵")]
    [SerializeField] private Text enemyCountText;
    [SerializeField] private EnemySpawner[] spawners;
    [SerializeField] private int clearEnemyCount = 5;

    [Header("プレイヤー")]
    [SerializeField] private FirstPersonMovement firstPerson;
    [SerializeField] private FirstPersonGunController gunController;

    [Header("UI")]
    [SerializeField] private Text centerText;
    [SerializeField] private float waitTime = 2f;

    private int score = 0;
    private int defeatedEnemyCount = 0;
    private bool isCleared = false;

    [System.NonSerialized]
    public bool gameOver = false;

    public int Score
    {
        get => score;

        set
        {
            score = Mathf.Clamp(value, 0, maxScore);

            if (scoreText != null)
            {
                scoreText.text = score.ToString("D8");
            }
        }
    }

    private void Start()
    {
        UpdateEnemyCount();
        SetSpawners(true);
    }

    private void Update()
    {
        UpdateEnemyCount();
    }

    public IEnumerator GameStart()
    {
        SetSpawners(true);

        yield return new WaitForSeconds(1f);
    }

    public IEnumerator GameOver()
    {
        gameOver = true;

        if (firstPerson != null)
        {
            // firstPerson.playerCanMove = false;
            // firstPerson.enableCameraMovement = false;
        }

        if (gunController != null)
        {
            gunController.shootEnabled = false;
        }

        SetSpawners(false);

        if (centerText != null)
        {
            centerText.enabled = true;
            centerText.text = "GAME OVER";
        }

        yield return new WaitForSeconds(waitTime);

        DestroyEnemies();

        if (centerText != null)
        {
            centerText.enabled = false;
            centerText.text = "";
        }

        gameOver = false;
    }

    private void SetSpawners(bool enable)
    {
        if (spawners == null)
            return;

        foreach (EnemySpawner spawner in spawners)
        {
            if (spawner != null)
            {
                spawner.spawnEnabled = enable;
            }
        }
    }

    private void DestroyEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }

        UpdateEnemyCount();
    }

    private void UpdateEnemyCount()
    {
        int count = GameObject.FindGameObjectsWithTag("Enemy").Length;

        if (enemyCountText != null)
        {
            enemyCountText.text = "残数 : " + count;
        }
    }
}