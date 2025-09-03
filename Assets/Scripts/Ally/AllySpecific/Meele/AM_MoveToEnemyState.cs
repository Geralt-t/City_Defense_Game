using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AM_MoveToEnemyState : State
{
    public AllyMeele allyMeele;
    public AM_MoveToEnemyState(Entity entity, FiniteStateMachine stateMachine, string animBoolName, AllyMeele allyMeele)
        : base(entity, stateMachine, animBoolName)
    {
        this.allyMeele = allyMeele;
    }
    public override void Enter()
    {
        base.Enter();
       
        if (allyMeele.targetEnemy != null)
        {
            allyMeele.MoveToTarget(allyMeele.targetEnemy);
        }
    }
    public override void Exit()
    {
        base.Exit();
        allyMeele.anim.SetFloat(animBoolName, 0f);
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (allyMeele.targetEnemy == null || !allyMeele.targetEnemy.gameObject.activeInHierarchy)
        {
            stateMachine.ChangeState(allyMeele.idleState);
            return;
        }
        float distance = Vector3.Distance(allyMeele.transform.position, allyMeele.targetEnemy.position);

        allyMeele.MoveToTarget(allyMeele.targetEnemy);
        allyMeele.anim.SetFloat(animBoolName, allyMeele.realSpeed);

        if (distance <= allyMeele.attackRange)
        {
            stateMachine.ChangeState(allyMeele.meeleAttackState);
        }
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
    
}
