using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyManager : MonoBehaviour
{
    private GameObject[] enemyBox;

    [SerializeField] private string nextSceneName;

    void Update()
    {
        enemyBox = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemyBox.Length == 0)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}