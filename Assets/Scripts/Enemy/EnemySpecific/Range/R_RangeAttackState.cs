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
            enemyRange.anim.SetInteger("AttackMode", 0); // Gi? s? AttackMode 0 là t?n công t?m xa
            enemyRange.anim.SetBool(animBoolName, true); // Ví d?: "IsAttacking"
        }
        else
        {
            // N?u không có m?c tiêu, chuy?n tr?ng thái v? di chuy?n
            stateMachine.ChangeState(enemyRange.moveToLineState);
            return;
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // ?u tiên 1: Ki?m tra m?c tiêu c?n chi?n
        if (enemyRange.GetClosestTargetInRange(enemyRange.meeleAttackRange) != null)
        {
            stateMachine.ChangeState(enemyRange.meeleAttackState);
            return;
        }

        // ?u tiên 2: Ki?m tra m?c tiêu hi?n t?i còn trong t?m không (có buffer ?? tránh gi?t)
        float exitBuffer = 0.1f;
        float exitRange = enemyRange.rangeAttackRange - exitBuffer;

        // N?u m?c tiêu bi?n m?t, không ho?t ??ng, ho?c ra kh?i t?m ?ánh, chuy?n tr?ng thái
        if (enemyRange.targetAlly == null || !enemyRange.targetAlly.gameObject.activeInHierarchy ||
            Vector3.Distance(enemyRange.transform.position, enemyRange.targetAlly.position) > exitRange)
        {
            stateMachine.ChangeState(enemyRange.moveToLineState);
            return;
        }

        // Luôn nhìn v? phía m?c tiêu khi ?ang t?n công
        enemyRange.LookAtTarget(enemyRange.targetAlly.position);

        // Ki?m tra th?i gian h?i chiêu
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
        enemyRange.targetAlly = null; // Xóa m?c tiêu khi thoát tr?ng thái
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
            attackDetails.damageAmount // Truy?n giá tr? sát th??ng
        );

        // C?p nh?t th?i gian t?n công cu?i cùng SAU KHI ??n ???c phóng
        lastAttackTime = Time.time;
     
    }
}