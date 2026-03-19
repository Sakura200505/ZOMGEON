using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChangeSceneTime : MonoBehaviour
{
    private float counttime = 0.0f;
    public float timeLimit = 120.0f;
    public Text timerText; // 残り時間を表示するためのTextコンポーネント

    // Start is called before the first frame update
    void Start()
    {
        if (timerText == null)
        {
            Debug.LogError("TimerTextが設定されていません。インスペクターで設定してください。");
        }
    }

    // Update is called once per frame
    void Update()
    {
        counttime += Time.deltaTime;

        // 残り時間を計算
        float remainingTime = Mathf.Max(0, timeLimit - counttime);

        // 残り時間をUIに表示
        if (timerText != null)
        {
            timerText.text = "残り時間: " + remainingTime.ToString("F1") + "s";
        }

        // 時間が超過した場合、GameOverシーンをロード
        if (remainingTime <= 0)
        {
            SceneManager.LoadScene("GameOver");
        }
    }
}
