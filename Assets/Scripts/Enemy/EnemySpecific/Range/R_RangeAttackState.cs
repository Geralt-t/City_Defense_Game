using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class R_RangeAttackState : State
{
    private EnemyRange enemyRange;
    private AttackDetails attackDetails;
    private float lastAttackTime;

    public R_RangeAttackState(Entity entity, FiniteStateMachine stateMachine, string animBoolName, EnemyRange enemyRange)
        : base(entity, stateMachine, animBoolName)
    {
        this.enemyRange = enemyRange;
    }

    public override void Enter()
    {
        base.Enter();

        if (enemyRange.agent.isOnNavMesh && enemyRange.agent.isActiveAndEnabled)
        {
            enemyRange.agent.isStopped = true;
            enemyRange.agent.updatePosition = false; 
            enemyRange.agent.updateRotation = false;
            enemyRange.agent.ResetPath();
        }

        attackDetails.damageAmount = enemyRange.enemyRangeStats.rangeDamage;
        lastAttackTime = Time.time;

        enemyRange.targetAlly = enemyRange.GetClosestTargetInRange(enemyRange.rangeAttackRange);

        if (enemyRange.targetAlly != null)
        {
            enemyRange.LookAtTarget(enemyRange.targetAlly.position);
            enemyRange.anim.SetInteger("AttackMode", 0); // Gi? s? AttackMode 0 l� t?n c�ng t?m xa
            enemyRange.anim.SetBool(animBoolName, true); // V� d?: "IsAttacking"
        }
        else
        {
            // N?u kh�ng c� m?c ti�u, chuy?n tr?ng th�i v? di chuy?n
            stateMachine.ChangeState(enemyRange.moveToLineState);
            return;
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // ?u ti�n 1: Ki?m tra m?c ti�u c?n chi?n
        if (enemyRange.GetClosestTargetInRange(enemyRange.meeleAttackRange) != null)
        {
            stateMachine.ChangeState(enemyRange.meeleAttackState);
            return;
        }

        // ?u ti�n 2: Ki?m tra m?c ti�u hi?n t?i c�n trong t?m kh�ng (c� buffer ?? tr�nh gi?t)
        float exitBuffer = 0.1f;
        float exitRange = enemyRange.rangeAttackRange - exitBuffer;

        // N?u m?c ti�u bi?n m?t, kh�ng ho?t ??ng, ho?c ra kh?i t?m ?�nh, chuy?n tr?ng th�i
        if (enemyRange.targetAlly == null || !enemyRange.targetAlly.gameObject.activeInHierarchy ||
            Vector3.Distance(enemyRange.transform.position, enemyRange.targetAlly.position) > exitRange)
        {
            stateMachine.ChangeState(enemyRange.moveToLineState);
            return;
        }

        // Lu�n nh�n v? ph�a m?c ti�u khi ?ang t?n c�ng
        enemyRange.LookAtTarget(enemyRange.targetAlly.position);

        // Ki?m tra th?i gian h?i chi�u
        if (Time.time >= lastAttackTime + enemyRange.attackCooldown)
        {
            enemyRange.anim.SetBool(animBoolName, true);
        }
    }

    public override void Exit()
    {
        base.Exit();
        if (enemyRange.agent.isOnNavMesh && enemyRange.agent.isActiveAndEnabled)
        {
            enemyRange.agent.isStopped = false;
            enemyRange.agent.updatePosition = true;  
            enemyRange.agent.updateRotation = true; 
        }
        enemyRange.anim.SetBool(animBoolName, false);
        enemyRange.targetAlly = null; // X�a m?c ti�u khi tho�t tr?ng th�i
    }


    public void TriggerAttackEnemyRange()
    {
        if (enemyRange.targetAlly == null || !enemyRange.targetAlly.gameObject.activeInHierarchy)
        {
            stateMachine.ChangeState(enemyRange.moveToLineState);
            return;
        }

        if (EnemyProjectilesPool.Instance == null)
        {
            
            return;
        }

        GameObject projectileGO = EnemyProjectilesPool.Instance.GetProjectile();
        if (projectileGO == null)
        {
            return;
        }

        ProjectileEnemy projectile = projectileGO.GetComponent<ProjectileEnemy>();
        if (projectile == null)
        {
            return;
        }
        projectile.Launch(
            enemyRange.throwPoint.transform.position,
            enemyRange.targetAlly.position,
            attackDetails.damageAmount // Truy?n gi� tr? s�t th??ng
        );

        // C?p nh?t th?i gian t?n c�ng cu?i c�ng SAU KHI ??n ???c ph�ng
        lastAttackTime = Time.time;
     
    }
}