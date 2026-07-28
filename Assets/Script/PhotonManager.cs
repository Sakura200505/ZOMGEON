using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager Instance;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Prefab Name in Resources/")]
    public string playerPrefabName = "Player";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("マスターサーバーに接続！");
        PhotonNetwork.JoinOrCreateRoom("room1", new RoomOptions(), TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("room1 に入室！");
        SpawnPlayer();
    }

    //===========
    // プレイヤー生成
    //===========
    public void SpawnPlayer()
    {
        // Prefabチェック
        if (Resources.Load(playerPrefabName) == null)
        {
            Debug.LogError($"Resources/{playerPrefabName}.prefab が見つからないよ！");
            return;
        }

        // スポーンポイントが無い
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("spawnPoints が設定されてないよ！");
            return;
        }

        // ランダムスポーン
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];

        PhotonNetwork.Instantiate(playerPrefabName, point.position, point.rotation);
    }
}
