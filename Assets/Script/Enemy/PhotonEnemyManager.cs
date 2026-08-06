using UnityEngine;
using Photon.Pun;

public class PhotonEnemyManager : MonoBehaviourPun
{
    private GameObject[] enemyBox;

    [SerializeField] private string nextSceneName = "NextScene";

    void Update()
    {
        // MasterClient‚Ì‚İƒV[ƒ“‘JˆÚ‚ğŠÇ—
        if (!PhotonNetwork.IsMasterClient)
            return;

        enemyBox = GameObject.FindGameObjectsWithTag("Enemy");

        Debug.Log("“G‚Ì”F" + enemyBox.Length);

        if (enemyBox.Length == 0)
        {
            PhotonNetwork.LoadLevel(nextSceneName);
        }
    }
}