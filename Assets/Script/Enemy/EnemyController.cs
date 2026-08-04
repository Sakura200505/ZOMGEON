using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [Header("Enemy Settings")]
    public int maxHp = 3;
    public int ammoDamage = 5;
    public int score = 100;
    public float deadTime = 3f;
    public float attackInterval = 1f;
    public string targetTag = "Player";

    private int hp;

    private Animator animator;
    private NavMeshAgent agent;
    private Transform target;
    private bool moveEnabled = true;
    private bool attacking = false;
    private bool isDead = false;

    private GameManager gameManager;
    private FirstPersonGunController player;

    public int Hp
    {
        get => hp;
        set
        {
            hp = Mathf.Clamp(value, 0, maxHp);

            if (hp <= 0 && !isDead)
            {
                StartCoroutine(Die());
            }
        }
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        GameObject targetObj = GameObject.FindGameObjectWithTag(targetTag);
        if (targetObj != null)
            target = targetObj.transform;

        GameObject gmObj = GameObject.FindGameObjectWithTag("GameController");
        if (gmObj != null)
            gameManager = gmObj.GetComponent<GameManager>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.GetComponentInChildren<FirstPersonGunController>();

        Hp = maxHp;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;
    }

    void Update()
    {
        if (isDead)
            return;

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

    public void TakeDamage(int amount)
    {
        Hp -= amount;

        Debug.Log($"{gameObject.name} HP : {Hp}");
    }

    IEnumerator Die()
    {
        isDead = true;
        moveEnabled = false;
        agent.isStopped = true;
        animator.SetTrigger("Dead");

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;

        if (gameManager != null)
            gameManager.Score += score;

        yield return new WaitForSeconds(deadTime);

        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
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
}