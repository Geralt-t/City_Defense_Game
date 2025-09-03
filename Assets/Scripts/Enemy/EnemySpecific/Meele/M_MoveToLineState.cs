using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class M_MoveToLineState : State
{
    private EnemyMeele enemyMeele;
    int defenseLineMask = LayerMask.GetMask("DefenseLine");
    public M_MoveToLineState(Entity entity, FiniteStateMachine stateMachine, string animBoolName, EnemyMeele enemyMeele)
        : base(entity, stateMachine, animBoolName)
    {
        this.enemyMeele = enemyMeele;
    }

    public override void Enter()
    {
        base.Enter();
        enemyMeele.totalChaseTime = 0f;
        if (enemyMeele.agent.isOnNavMesh && enemyMeele.agent.isActiveAndEnabled)
        {
            enemyMeele.agent.isStopped = false;
            enemyMeele.agent.SetDestination(enemyMeele.defenseGoalPoint.transform.position);
        }
        
    }

    public override void Exit()
    {
        base.Exit();
        enemyMeele.agent.isStopped = true;
        enemyMeele.anim.SetFloat(animBoolName, 0);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        //if (GameManager.Instance.GetCurrentState() == GameManager.GameState.Lose)
        //{
        //    stateMachine.ChangeState(enemyMeele.celebrateState);
        //    return;
        //}
        //Debug.Log("Agent Speed: " + enemyMeele.agent.velocity.magnitude);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        if (UIManager.Instance.CurrentState == UIManager.GameState.InGame)
        {
            Collider[] hits = Physics.OverlapSphere(enemyMeele.transform.position, enemyMeele.detectionRange, enemyMeele.allyLayer);
            if (hits.Length > 0)
            {
                stateMachine.ChangeState(enemyMeele.chaseState);
                return;
            }
            enemyMeele.anim.SetFloat(animBoolName, enemyMeele.agent.velocity.magnitude);
            if (IsTouchingDefenseLine() == true)
            {
                enemyMeele.isTouchingDefenseLine = true;
            }
        }
        else
        {
            enemyMeele.agent.enabled = false;
            enemyMeele.anim.SetFloat(animBoolName, 0);
        }
    }
    private bool IsTouchingDefenseLine()
    {
        Collider[] colliders = Physics.OverlapSphere(enemyMeele.transform.position, 0.3f, defenseLineMask);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("DefenseLine"))
            {
               
                return true;

            }
        }
        return false;
    }
}
