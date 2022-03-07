using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    enum EnemyLevel
    {
        level1, level2, level3, level4, boss
    }

    // status
    EnemyLevel level;
    protected float speed;
    public int hp;
    public int attackDamage;
    public float experience;
    
    public bool isInQueue = false;
    public bool isAlive = true;
    public bool isAttacking = false;
    public Player player;
    [SerializeField]
    PlayerStatus playerStatus;

    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        level = SetEnemyLevel();
        SetEnemyStatus();
        //animator.keepAnimatorControllerStateOnDisable = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isAlive)
        {
            // 플레이어 공격 큐에 없다면 gate까지 이동
            if (isInQueue == false)
            {
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") == true)
                    animator.SetTrigger("Walk");
                transform.position += Vector3.left * speed * Time.deltaTime;
            }
                
            else // 자신이 큐에 들어갔다면 공격
            {
                if (isAttacking == false)
                    Attack(attackDamage);
            }
        }
    }
    EnemyLevel SetEnemyLevel()
    {
        EnemyLevel level;
        switch (gameObject.tag)
        {
            case "Enemy1":
                level = EnemyLevel.level1;
                break;
            case "Enemy2":
                level = EnemyLevel.level2;
                break;
            case "Enemy3":
                level = EnemyLevel.level3;
                break;
            case "Enemy4":
                level = EnemyLevel.level4;
                break;
            default:
                level = EnemyLevel.boss;
                break;
        }
        return level;
    }
    void SetEnemyStatus()
    {
        float strengthRate = ((GameManager.instance.stage - 1) / 5 + 1 ) * GameManager.instance.stageReinforcement;
        if (GameManager.instance.stage < 6)
            strengthRate = 1.0f;

        // stage의 5주기마다 stageReinforcement * n배 강해짐
        switch (level)
        {
            case EnemyLevel.level1:
                hp = (int)(Random.Range(1, 3) * strengthRate);
                speed = Random.Range(1, 4) * strengthRate * GameManager.instance.snailRate;
                attackDamage = (int)(Random.Range(1, 3) * strengthRate * GameManager.instance.DemoralizingRate);
                break;

            case EnemyLevel.level2:
                hp = (int)(Random.Range(2, 4) * strengthRate);
                speed = Random.Range(1, 3) * strengthRate *GameManager.instance.snailRate;
                attackDamage = (int)(Random.Range(2, 4) * strengthRate * GameManager.instance.DemoralizingRate);
                break;

            case EnemyLevel.level3:
                hp = (int)(Random.Range(3, 5) * strengthRate);
                speed = Random.Range(1, 4) * strengthRate * GameManager.instance.snailRate;
                attackDamage = (int)(Random.Range(3, 4) * strengthRate * GameManager.instance.DemoralizingRate);
                break;

            case EnemyLevel.level4:
                hp = (int)(Random.Range(4, 6) * strengthRate);
                speed = Random.Range(1, 4) * strengthRate * GameManager.instance.snailRate;
                attackDamage = (int)(Random.Range(4, 5) * strengthRate * GameManager.instance.DemoralizingRate);
                break;

            case EnemyLevel.boss:
                hp = (int)(Random.Range(60, 70) * strengthRate);
                speed = Random.Range(2, 4) * strengthRate * GameManager.instance.snailRate;
                attackDamage = (int)(Random.Range(5, 7) * strengthRate * GameManager.instance.DemoralizingRate);
                break;
        }
        experience = hp * strengthRate;
    }

    void Attack(int attackDamage)
    {
        isAttacking = true;

        if (level == EnemyLevel.boss)
            animator.SetTrigger("Attack" + Random.Range(1, 7));
        else if (level == EnemyLevel.level4)
            animator.SetTrigger("Attack" + Random.Range(1, 4));
        else
            animator.SetTrigger("Attack" + Random.Range(1, 3));

        StartCoroutine(AttackProcess());
    }
    IEnumerator AttackProcess()
    {
        while (true)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack") &&
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f)
            {
                if (player != null)
                    player.Damaged(attackDamage);

                int trueDamage = (int)((attackDamage - playerStatus.defense) * GameManager.instance.tetkaiRate);

                if (trueDamage < 0)
                    trueDamage = 0;

                if (hp >= 0 && playerStatus.hedgehog)
                    Damaged(trueDamage);
                break;
            }
            yield return null;
        }
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length * 0.5f + 0.5f);
        isAttacking = false;
    }

    public void Damaged(int attackDamage)
    {
        hp -= attackDamage;

        if (hp <= 0)
        {
            if (GameManager.instance.bossStage)
                GameManager.instance.spawnStart = false;
            StartCoroutine(DieProcess());
            playerStatus.currentExp += experience * playerStatus.expRate;
        }
    }

    IEnumerator DieProcess()
    {
        player.attackableQueue.Dequeue();
        isInQueue = false;
        InitEnemy();
        animator.SetTrigger("Dead");
        while (true)
        {
            
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                FindObjectOfType<EnemySpawn>().enemyObjectPool.Add(gameObject);
                gameObject.SetActive(false);
                break;
            }
            yield return null;
        }        
    }

    public void InitEnemy()
    {
        if (isInQueue) // 플레이어의 공격 큐에 들어갔지만 정문을 통과한 경우 큐에서 빠져 나가야함(큐의 첫 번째가 아니라도 통과 가능하므로 검색)
        {
            player.attackableQueue.Clear(); // 다 초기화하고 다시 플레이어 쪽에서 enqueue
        }
        isAlive = false;
        isInQueue = false;
        isAttacking = false;
        player = null;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") == true && player == null)
        {
            if (isInQueue)
                player = collision.gameObject.GetComponent<Player>();
        }  
    }
}
