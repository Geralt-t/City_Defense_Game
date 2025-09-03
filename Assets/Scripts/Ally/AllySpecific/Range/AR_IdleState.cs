using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AR_IdleState : State
{
    public AllyRange allyRange;
    public AR_IdleState(Entity entity, FiniteStateMachine stateMachine, string animBoolName, AllyRange allyRange)
        : base(entity, stateMachine, animBoolName)
    {
        this.allyRange = allyRange;
    }
    public override void Enter()
    {
        base.Enter();
       
    }
    public override void Exit()
    {
        base.Exit();
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (UIManager.Instance.GetCurrentState() == UIManager.GameState.Win)
        {
            stateMachine.ChangeState(allyRange.celebrateState);
        }
       
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        Collider[] enemies = Physics.OverlapSphere(allyRange.transform.position, allyRange.attackRange, allyRange.enemyLayer);

        if (enemies.Length > 0)
        {
            allyRange.targetEnemy = allyRange.GetNearestEnemy(enemies);
            float distance = Vector3.Distance(allyRange.transform.position, allyRange.targetEnemy.position);

            if (distance <= allyRange.attackRange)
                stateMachine.ChangeState(allyRange.rangeAttackState);

        }
    }
}
