using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class R_MoveToLineState : State
{
    private EnemyRange enemyRange;
  

    int defenseLineMask = LayerMask.GetMask("DefenseLine");

    public R_MoveToLineState(Entity entity, FiniteStateMachine stateMachine, string animBoolName, EnemyRange enemyRange)
        : base(entity, stateMachine, animBoolName)
    {
        this.enemyRange = enemyRange;
    }

    public override void Enter()
    {
        base.Enter();
        if (enemyRange.agent.isOnNavMesh && enemyRange.agent.isActiveAndEnabled)
        {
            enemyRange.agent.isStopped = false;
            enemyRange.agent.SetDestination(enemyRange.defenseGoalPoint.transform.position);
            Debug.Log("Entered MoveToLineState");
        }
    }

    public override void Exit()
    {
        base.Exit();
        if (enemyRange.agent.isOnNavMesh && enemyRange.agent.isActiveAndEnabled)
        {
            enemyRange.agent.isStopped = true;
        }

        enemyRange.anim.SetFloat(animBoolName, 0);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        if (UIManager.Instance.CurrentState != UIManager.GameState.InGame)
        {
            enemyRange.agent.enabled = false;
            enemyRange.anim.SetFloat(animBoolName, 0);
            return;
        }

        // Tính waitTimer khi ??ng yên
        

        Collider[] hits = Physics.OverlapSphere(enemyRange.transform.position, enemyRange.rangeAttackRange, enemyRange.allyLayer);
        if (hits.Length > 0)
        {
            stateMachine.ChangeState(enemyRange.rangeAttackState);
            return;
        }

        if (IsTouchingDefenseLine())
        {
            enemyRange.isTouchingDefenseLine = true;
        }

        enemyRange.anim.SetFloat(animBoolName, enemyRange.agent.velocity.magnitude);
    }

    private bool IsTouchingDefenseLine()
    {
        Collider[] colliders = Physics.OverlapSphere(enemyRange.transform.position, 0.1f, defenseLineMask);
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
