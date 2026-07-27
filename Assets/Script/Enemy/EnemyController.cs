using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PhotonView))]
public class EnemyController : MonoBehaviourPun, IPunObservable
{
    [Header("設定")]
    public bool moveEnabled = true;

    [SerializeField] private int maxHp = 3;
    [SerializeField] private int ammoDamage = 5;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private int score = 100;
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private float deadTime = 3f;


    private int hp;

    private bool attacking = false;
    private bool isDead = false;

    private Animator animator;
    private BoxCollider boxCollider;
    private Rigidbody rigidBody;
    private NavMeshAgent agent;

    private Transform target;

    private GameManager gameManager;
    private FirstPersonGunController player;


    // =========================
    // HP
    // =========================

    public int Hp
    {
        get => hp;

        set
        {
            hp = Mathf.Clamp(value, 0, maxHp);

            if (hp <= 0 && !isDead)
            {
                StartCoroutine(Dead());
            }
        }
    }


    void Start()
    {
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider>();
        rigidBody = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();


        // ターゲット取得
        GameObject targetObj = GameObject.FindGameObjectWithTag(targetTag);

        if (targetObj != null)
        {
            target = targetObj.transform;
        }


        // GameManager取得
        GameObject gmObj = GameObject.FindGameObjectWithTag("GameController");

        if (gmObj != null)
        {
            gameManager = gmObj.GetComponent<GameManager>();
        }


        // プレイヤー取得
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.GetComponentInChildren<FirstPersonGunController>();
        }


        if (photonView.IsMine)
        {
            Hp = maxHp;
        }
    }



    void Update()
    {
        // AI制御は所有者のみ
        if (!photonView.IsMine)
            return;


        if (isDead)
            return;


        if (moveEnabled && target != null)
        {
            agent.SetDestination(target.position);

            animator.SetFloat(
                "Speed",
                agent.velocity.magnitude,
                0.1f,
                Time.deltaTime
            );
        }
        else
        {
            animator.SetFloat(
                "Speed",
                0,
                0.1f,
                Time.deltaTime
            );
        }
    }



    // =========================
    // ダメージ処理
    // =========================

    [PunRPC]
    public void TakeDamage(int damage)
    {
        if (!photonView.IsMine)
            return;


        Hp -= damage;


        Debug.Log(
            $"{gameObject.name} HP : {Hp}"
        );
    }




    // =========================
    // 死亡
    // =========================

    IEnumerator Dead()
    {
        isDead = true;

        moveEnabled = false;


        agent.isStopped = true;


        animator.SetTrigger("Dead");


        boxCollider.enabled = false;


        rigidBody.isKinematic = true;



        if (gameManager != null)
        {
            gameManager.Score += score;
        }


        yield return new WaitForSeconds(deadTime);



        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }





    // =========================
    // 攻撃
    // =========================

    private void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine)
            return;


        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(AttackTimer());
        }
    }



    IEnumerator AttackTimer()
    {
        if (attacking)
            yield break;


        attacking = true;

        moveEnabled = false;


        animator.SetTrigger("Attack");


        if (player != null)
        {
            player.Ammo -= ammoDamage;
        }


        yield return new WaitForSeconds(attackInterval);



        attacking = false;

        moveEnabled = true;
    }





    // =========================
    // Photon同期
    // =========================

    public void OnPhotonSerializeView(
        PhotonStream stream,
        PhotonMessageInfo info)
    {

        if (stream.IsWriting)
        {
            stream.SendNext(hp);
            stream.SendNext(isDead);
        }
        else
        {
            hp = (int)stream.ReceiveNext();
            isDead = (bool)stream.ReceiveNext();
        }
    }
}