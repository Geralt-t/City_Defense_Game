using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class M_ChaseState : State
{
    private EnemyMeele enemyMeele;
    private Transform targetAlly;
    private float checkTimer = 0f;
    private float checkInterval = 0.25f;
    public M_ChaseState(Entity etity, FiniteStateMachine stateMachine, string animBoolName, EnemyMeele enemyMeele) : base(etity, stateMachine, animBoolName)
    {
        this.enemyMeele=enemyMeele;
    }
    public override void Enter()
    {
        base.Enter();
        targetAlly = enemyMeele.GetNearestAlly();
        if (enemyMeele.agent.enabled && enemyMeele.agent.isOnNavMesh)
        {
            enemyMeele.agent.isStopped = false;

            if (targetAlly != null)
            {
                enemyMeele.agent.SetDestination(targetAlly.position);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        if (enemyMeele.agent.enabled && enemyMeele.agent.isOnNavMesh) { 
            enemyMeele.agent.isStopped = true;
        }
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();

        enemyMeele.totalChaseTime += Time.deltaTime;

        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            targetAlly = enemyMeele.GetNearestAlly();
            checkTimer = 0f;
        }

        if (targetAlly == null || enemyMeele.totalChaseTime > enemyMeele.maxChaseTime)
        {
            stateMachine.ChangeState(enemyMeele.moveToLineState);
            return;
        }

        float distance = Vector3.Distance(enemyMeele.transform.position, targetAlly.position);

        if (distance <= enemyMeele.attackRange)
        {
            stateMachine.ChangeState(enemyMeele.attackState);
            return;
        }

        if (enemyMeele.agent.enabled && enemyMeele.agent.isOnNavMesh)
        {
            if (Vector3.Distance(enemyMeele.agent.destination, targetAlly.position) > 0.5f)
            {
                enemyMeele.agent.SetDestination(targetAlly.position);
            }

            enemyMeele.anim.SetFloat(animBoolName, enemyMeele.agent.desiredVelocity.magnitude);
        }
        enemyMeele.LookAtTarget(targetAlly.position);
        enemyMeele.anim.SetFloat(animBoolName, enemyMeele.agent.desiredVelocity.magnitude);
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
    
}
