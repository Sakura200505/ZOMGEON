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
    public bool moveEnabled = true;

    [SerializeField] int maxHp = 3;
    [SerializeField] int ammoDamage = 5;
    [SerializeField] int attackInterval = 1;
    [SerializeField] int score = 100;
    [SerializeField] string targetTag = "Player";
    [SerializeField] float deadTime = 3;

    bool attacking = false;
    int hp;
    float moveSpeed;
    Animator animator;
    BoxCollider boxCollider;
    Rigidbody rigidBody;
    NavMeshAgent agent;
    Transform target;
    GameManager gameManager;
    FirstPersonGunController player;

    // HP�����p
    public int Hp
    {
        get => hp;
        set
        {
            hp = Mathf.Clamp(value, 0, maxHp);
            if (hp <= 0 && photonView.IsMine)
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

        GameObject targetObj = GameObject.FindGameObjectWithTag(targetTag);
        if (targetObj != null) target = targetObj.transform;

        GameObject gmObj = GameObject.FindGameObjectWithTag("GameController");
        if (gmObj != null) gameManager = gmObj.GetComponent<GameManager>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.GetComponentInChildren<FirstPersonGunController>();

        moveSpeed = agent.speed;

        if (photonView.IsMine)
        {
            Hp = maxHp;
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return; // MasterClient のみ

        if (moveEnabled && target != null)
        {
            agent.SetDestination(target.position);
            animator.SetFloat("Speed", agent.velocity.magnitude, 0.1f, Time.deltaTime);
        }
        else
        {
            animator.SetFloat("Speed", 0, 0.1f, Time.deltaTime);
        }
    }


    void Move()
    {
        agent.speed = moveSpeed;
        animator.SetFloat("Speed", agent.speed, 0.1f, Time.deltaTime);

        agent.SetDestination(target.position);
        rigidBody.linearVelocity = agent.desiredVelocity; // linearVelocity �� velocity
    }

    void Stop()
    {
        agent.speed = 0;
        animator.SetFloat("Speed", agent.speed, 0.1f, Time.deltaTime);
    }

    IEnumerator Dead()
    {
        moveEnabled = false;
        Stop();

        if (gameManager != null)
            gameManager.Score += score;

        animator.SetTrigger("Dead");
        boxCollider.enabled = false;
        rigidBody.isKinematic = true;

        yield return new WaitForSeconds(deadTime);

        if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
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
        if (!attacking)
        {
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

        yield return null;
    }

    // HP���l�b�g���[�N����
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(hp);
        }
        else
        {
            hp = (int)stream.ReceiveNext();
        }
    }
}
