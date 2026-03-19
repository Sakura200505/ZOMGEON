using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class EnemyManager : MonoBehaviourPun
{
    private GameObject[] enemyBox;

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return; // MasterClient だけが管理

        enemyBox = GameObject.FindGameObjectsWithTag("Enemy");

        // デバッグ用
        Debug.Log("敵の数：" + enemyBox.Length);

        if (enemyBox.Length == 0)
        {
            // シーン遷移はMasterClientがPhotonNetwork.LoadLevelで行う
            PhotonNetwork.LoadLevel("NextScene");
        }
    }
}
