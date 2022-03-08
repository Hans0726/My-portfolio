using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    // common status
    public PlayerStatus playerStatus;

    // independent status
    [System.NonSerialized]
    public int hp;
    [System.NonSerialized]
    public int hpMax;
    bool isAlive = true;
    [SerializeField]
    bool isAttacking = false;
    SpriteRenderer sr;
    public Queue<GameObject> attackableQueue;

    Animator animator;
    Slider playerhpSlider;
    Text playerhpText;
    
    void Start()
    {
        hpMax = playerStatus.hpMax;
        hp = playerStatus.hp;
        sr = GetComponent<SpriteRenderer>();
        attackableQueue = new Queue<GameObject>();
        animator = GetComponent<Animator>();
        playerhpSlider = GetComponentInChildren<Slider>();
        playerhpText = GetComponentInChildren<Text>();
        StartCoroutine(HealingFactor());
    }

    // Update is called once per frame
    void Update()
    {
        if (attackableQueue.Count > 0 && isAttacking == false)
        {
            Attack(playerStatus.attackDamage);
        }
        if (playerStatus.currentExp >= playerStatus.totalExp)
        {
            LevelUP();
        }
        playerhpSlider.value = (float)hp / (float)hpMax;
    }
    public void CheckHP()
    {
        playerhpText.text = hp + " / " + hpMax;
    }
    public void SetFalseCheckHP()
    {
        playerhpText.text = " ";
    }
    IEnumerator HealingFactor()
    {
        WaitForSeconds waitSec = new WaitForSeconds(5f);

        while (true)
        {
            if (GameManager.instance.stageStart)
                hp += playerStatus.healValue;

            if (hp >= hpMax)
                hp = hpMax;
            yield return waitSec;
        }
    }
    void Attack(int attackDamage)
    {
        isAttacking = true;
        animator.SetFloat("AttackSpeed", playerStatus.attackSpeed);
        animator.SetTrigger("Attack" + Random.Range(1, 4));
        SoundManager.instance.audioSourceEFX.PlayOneShot(SoundManager.instance.attack[Random.Range(0,3)]);
        StartCoroutine(AttackProcess());
    }

    IEnumerator AttackProcess()
    {
        Enemy enemy = attackableQueue.Peek().GetComponent<Enemy>();

        while (true)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack") &&
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f)
            {
                if (playerStatus.criticalRate >= Random.Range(1, 101))
                    enemy.Damaged(playerStatus.attackDamage * 2);
                else
                    enemy.Damaged(playerStatus.attackDamage);

                if (playerStatus.bloodsucking == true)
                {
                    int bloodsuckingValue = (int)((float)playerStatus.attackDamage * 0.3f);
                    if (bloodsuckingValue < 1)
                        hp += 1;
                    else
                        hp += bloodsuckingValue;
                    if (hp >= hpMax)
                        hp = hpMax;
                }
                break;
            }
            yield return null;
        }
        if (playerStatus.fastSwordTechnique)
        {
            int choose = Random.Range(0, 5);
            if (choose < 2)
                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length * 0.5f);
            else
                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length * 0.5f + 0.5f);
        }
        else
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length * 0.5f + 0.5f);
        isAttacking = false;
    }

    public void Damaged(int attackDamage)
    {
        if (playerStatus.avoidability < Random.Range(1, 101))
        {
            int trueDamage = (int)((attackDamage - playerStatus.defense) * GameManager.instance.tetkaiRate);
            if (trueDamage < 0)
                trueDamage = 0;
            hp -= trueDamage;
            playerStatus.hp = hp;
            StartCoroutine(DamagedEffect());
            if (hp <= 0)
                StartCoroutine(DieProcess());
        }
    }
    IEnumerator DamagedEffect()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.3f);
        sr.color = Color.white;
    }
    IEnumerator DieProcess()
    {
        hp = 0; 
        isAlive = false;
        foreach (GameObject enemy in attackableQueue)
        {
            enemy.GetComponent<Enemy>().isInQueue = false;
            enemy.GetComponent<Enemy>().isAttacking = false;
            enemy.GetComponent<Enemy>().player = null;
        }
        attackableQueue.Clear();

        playerStatus.hp = hpMax;

        SoundManager.instance.audioSourceEFX.PlayOneShot(SoundManager.instance.death);
        animator.SetTrigger("Death");

        float time = GameManager.instance.revivalTime;
        UIManager.instance.clockRevival.gameObject.SetActive(true);
        while (true)
        {
            time -= Time.deltaTime;
            UIManager.instance.clockRevival.text = "ÇÃ·¹ÀÌ¾î Àç¹èÄ¡±îÁö: " + time.ToString("00") + "ÃÊ";
            if (time <= 0)
            {
                UIManager.instance.clockRevival.gameObject.SetActive(false);
                break;
            }
            yield return null;
        }
        GameManager.instance.playerPositionX.Remove(transform.position.x);
        GameManager.instance.placeableNum++;
        Destroy(gameObject);
    }

    void LevelUP()
    {
        SoundManager.instance.audioSourceEFX.PlayOneShot(SoundManager.instance.levelUP);
        playerStatus.level++;
        playerStatus.hpMax += 5;
        hpMax += 5;
        playerStatus.currentExp -= playerStatus.totalExp;
        playerStatus.skillPoints++;
        playerStatus.totalExp *= GameManager.instance.expReinforcement;
        UIManager.instance.cardPanel.SetActive(true);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (isAlive && attackableQueue.Count < playerStatus.attackableNum)
            {
                if (enemy.isInQueue == false && enemy.isAlive == true)
                {
                    attackableQueue.Enqueue(collision.gameObject);
                    enemy.isInQueue = true;
                }
            }
        }
    }
}
