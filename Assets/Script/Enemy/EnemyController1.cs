using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PhotonView))]
public class EnemyController1 : MonoBehaviourPun
{
    [Header("Enemy Settings")]
    public int maxHp = 3;
    public int ammoDamage = 5;
    public int score = 100;
    public float deadTime = 3f;
    public float attackInterval = 1f;
    public string targetTag = "Player";

    [HideInInspector] public int Hp;

    private Animator animator;
    private NavMeshAgent agent;
    private Transform target;
    private bool moveEnabled = true;
    private bool attacking = false;
    private GameManager gameManager;

    void Start()
    {
        Debug.Log($"{gameObject.name}" + $"ViewID={photonView.ViewID}" + $"IsMine={photonView.IsMine}");

        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        GameObject targetObj = GameObject.FindGameObjectWithTag(targetTag);
        if (targetObj != null) target = targetObj.transform;

        GameObject gmObj = GameObject.FindGameObjectWithTag("GameController");
        if (gmObj != null) gameManager = gmObj.GetComponent<GameManager>();

        Hp = maxHp;

        // Rigidbody を持っていたら kinematic にする
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    void Update()
    {
        if (!photonView.IsMine)
        {
            Debug.Log("Enemy Not Mine");
            return;
        }

        Debug.Log("Enemy Moving");

        if (moveEnabled && target != null)
        {
            Move();
        }
        else
        {
            Stop();
        }
}

    void Move()
    {
        agent.SetDestination(target.position);
        animator.SetFloat("Speed", agent.velocity.magnitude, 0.1f, Time.deltaTime);
    }

    void Stop()
    {
        agent.ResetPath();
        animator.SetFloat("Speed", 0, 0.1f, Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(AttackTimer());
        }
    }

    IEnumerator AttackTimer()
    {
        if (attacking) yield break;

        attacking = true;
        moveEnabled = false;

        animator.SetTrigger("Attack");

        // プレイヤーの弾を減らす（RPCで）
        PhotonView playerView = collisionPlayerPhotonView();
        if (playerView != null)
        {
            playerView.RPC("TakeDamageFromEnemy", RpcTarget.All, ammoDamage);
        }

        yield return new WaitForSeconds(attackInterval);

        attacking = false;
        moveEnabled = true;
    }

    // MasterClient にダメージを送る
    [PunRPC]
    public void TakeDamage(int amount)
    {
        if (!photonView.IsMine) return;

        Hp -= amount;
        if (Hp <= 0)
        {
            StartCoroutine(Die());
        }
    }

    IEnumerator Die()
    {
        moveEnabled = false;
        Stop();

        if (gameManager != null)
        {
            gameManager.Score += score;
        }

        animator.SetTrigger("Dead");
        agent.enabled = false;

        yield return new WaitForSeconds(deadTime);

        PhotonNetwork.Destroy(gameObject);
    }

    // ※プレイヤーのPhotonView取得用（OnCollisionEnter で使用）
    private PhotonView collisionPlayerPhotonView()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            return playerObj.GetComponent<PhotonView>();
        }
        return null;
    }
}
