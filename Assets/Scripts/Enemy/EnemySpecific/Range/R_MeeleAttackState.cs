using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class R_MeeleAttackState : State
{
    private EnemyRange enemyRange;
    private Transform targetAlly;
    protected AttackDetails attackDetails;
    private float lastAttackTime;

    public R_MeeleAttackState(Entity entity, FiniteStateMachine stateMachine, string animBoolName, EnemyRange enemyRange)
        : base(entity, stateMachine, animBoolName)
    {
        this.enemyRange = enemyRange;
    }

    public override void Enter()
    {
        base.Enter();

        enemyRange.agent.isStopped = true;
        enemyRange.agent.ResetPath();
        attackDetails.damageAmount = enemyRange.enemyRangeStats.damage;
        lastAttackTime = Time.time;

        targetAlly = enemyRange.GetClosestTargetInRange(enemyRange.meeleAttackRange);

        if (targetAlly != null)
        {
            enemyRange.LookAtTarget(targetAlly.position);
            enemyRange.anim.SetInteger("AttackMode", 1); 
            enemyRange.anim.SetBool(animBoolName, true); 
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        Collider[] meeleHits = Physics.OverlapSphere(enemyRange.transform.position, enemyRange.meeleAttackRange, enemyRange.allyLayer);
        if (meeleHits.Length == 0)
        {
            stateMachine.ChangeState(enemyRange.rangeAttackState);
            return;
        }

        targetAlly = enemyRange.GetClosestTargetInRange(enemyRange.meeleAttackRange);
        if (targetAlly != null)
        {
            enemyRange.LookAtTarget(targetAlly.position);

            if (Time.time >= lastAttackTime + enemyRange.enemyRangeStats.attackCooldown)
            {
                lastAttackTime = Time.time;

            }
        }
       
    }

    public override void Exit()
    {
        base.Exit();

        enemyRange.anim.SetBool(animBoolName, false);
        enemyRange.anim.SetTrigger("Cancel");

        targetAlly = null;
    }
    public void TriggerAttackEnemyMeele()
    {
        Collider[] detectedObjects = Physics.OverlapSphere(enemyRange.transform.position, enemyRange.meeleAttackRange, enemyRange.allyLayer);

        foreach (Collider collider in detectedObjects)
        {
            collider.transform.SendMessage("Damage", attackDetails, SendMessageOptions.DontRequireReceiver);
        }
    }
}
