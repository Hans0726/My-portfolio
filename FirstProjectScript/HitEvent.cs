using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEvent : MonoBehaviour
{
    public GameObject attackEffectFactory;
    public Transform attackPoint;
    public float attackRange = 1f;
    public LayerMask enemyLayer;
    PlayerMove pm;
    EnemyFSM efsm;

    private void Start()
    {
        pm = FindObjectOfType<PlayerMove>();
    }
    private void OnEnable()
    {
        efsm = FindObjectOfType<EnemyFSM>();
    }
    public void HitPlayer()
    {
        
        if (Vector3.Distance(efsm.gameObject.transform.position, pm.gameObject.transform.position) < efsm.attackDistance)
        {
            SoundManager.instance.audioSourceEFX.PlayOneShot(SoundManager.instance.audioDamaged);
            pm.Damaged();
        }
    }

    public void HitEnemy()
    {
        Collider[] hitEnemy = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider enemy in hitEnemy)
        {
            GameObject attackEffect = Instantiate(attackEffectFactory);
            attackEffect.transform.position = attackPoint.position;
            SoundManager.instance.audioSourceEFX.PlayOneShot(SoundManager.instance.audioKick[Random.Range(0,2)]);
            enemy.GetComponentInParent<EnemyFSM>().Damaged();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}

