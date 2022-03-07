using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFSM : MonoBehaviour
{
    SphereCollider sightCol;
    GameObject player;

    public bool playerInSight = false;
    bool isAttacking = false;

    float viewAngle = 200.0f;
    public float attackDistance;
    public float attackDelay = 2f;
    public float turnSmoothTime = 0.5f;
    float turnSmoothVelocity;

    int hp = 2;
    NavMeshAgent nav;

    Animator animator;

    enum EnemyState
    {
        Search, Move, Attack, Die
    }

    EnemyState eState;
    // Start is called before the first frame update
    void Start()
    {
        sightCol = GetComponent<SphereCollider>();
        player = GameObject.FindGameObjectWithTag("Player");
        nav = GetComponent<NavMeshAgent>();
        nav.destination = GameManager.instance.RandomSpawnNearPoint(player.transform.position, sightCol.radius);

        if (nav.destination == Vector3.zero)
            nav.destination = GameManager.instance.RandomSpawnNearPoint(player.transform.position, sightCol.radius);

        animator = GetComponentInChildren<Animator>();
        animator.SetFloat("AttackDelay", attackDelay);
        eState = EnemyState.Search;
    }

    // Update is called once per frame
    void Update()
    {
        switch (eState)
        {
            case EnemyState.Search:
                Search();
                break;
            case EnemyState.Move:
                Move();
                break;
            case EnemyState.Attack:
                Attack();
                break;
            case EnemyState.Die:
                Die();
                break;
        }

    }
    void Search()
    {
        if (isAttacking == true)    // 공격 도중 플레이어 추적 실패 후 소규모 탐지(둘러보기)
        {
            // 공격 딜레이 동안 회전하여 탐색
            Vector3 dir = player.transform.position - transform.position;
            transform.rotation = Quaternion.Euler(0f,
                Mathf.SmoothDampAngle
                (transform.eulerAngles.y, Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg, ref turnSmoothVelocity, turnSmoothTime),
                0f);
            return;
        }
        if (playerInSight == true)
        {
            if (SoundManager.instance.audioSourceBGM.isPlaying == false)
                SoundManager.instance.audioSourceBGM.Play();
            nav.isStopped = true;
            nav.ResetPath();
            eState = EnemyState.Move;
            animator.SetTrigger("SearchToMove");
        }
        else 
        {
            if (nav.remainingDistance <= 3f)
                nav.destination = GameManager.instance.RandomSpawnNearPoint(player.transform.position, 40f);
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == player)
        {
            Vector3 dir = other.transform.position - transform.position;
            float angle = Vector3.Angle(dir, transform.forward);

            if (angle < viewAngle * 0.5f)
            {
                RaycastHit hit;

                if (Physics.Raycast(transform.position, dir.normalized, out hit, sightCol.radius))
                {
                    if (hit.collider.gameObject == player)
                    {
                        playerInSight = true;
                        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
                    }
                }
            }
            else
                playerInSight = false;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            playerInSight = false;
        }
    }

    void Move()
    {
        nav.stoppingDistance = attackDistance;
        nav.destination = player.transform.position;

        if (playerInSight == false)
        {
            animator.SetTrigger("MoveToSearch");
            eState = EnemyState.Search;
        }
        else if (Vector3.Distance(transform.position, player.transform.position) < attackDistance)
        {
            nav.isStopped = true;
            nav.ResetPath();
            eState = EnemyState.Attack;
            animator.SetTrigger("MoveToAttackDelay");
        }
    }

    void Attack()
    {
        if (Vector3.Distance(transform.position, player.transform.position) < attackDistance)   // 공격 범위 내면 공격
        {
            animator.SetTrigger("AttackDelayToAttack");
            isAttacking = true;
            nav.velocity = Vector3.zero;
            eState = EnemyState.Search;     // 공격 후 멈춘 동안 탐지
            StartCoroutine(AttackDelay());
        }
        else if (playerInSight == true) // 공격 범위 외면 추격
        {
            eState = EnemyState.Move;
            animator.SetTrigger("AttackDelayToMove");
        }
        else
        {
            eState = EnemyState.Search;
            animator.SetTrigger("AttackDelayToSearch");
        }
    }
    public void Damaged()
    {
        Vector3 dir = player.transform.forward;
        nav.velocity = dir * 15;
        hp--;
        if (hp == 0)
        {
            SoundManager.instance.audioSourceBGM.Pause();
            eState = EnemyState.Die;
        }
            
    }

    void Die()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Die") == false)
            animator.SetTrigger("Die");
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {
            Destroy(gameObject);
        }
            
    }

    IEnumerator AttackDelay()
    {
        yield return new WaitForSeconds(attackDelay);
        isAttacking = false;
        animator.SetTrigger("AttackDelayToSearch");
    }
}



