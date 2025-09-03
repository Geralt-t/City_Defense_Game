using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class M_CelebrateState : State
{
    public EnemyMeele enemyMeele;
    private int celebrateIndex;

    public M_CelebrateState(Entity etity, FiniteStateMachine stateMachine, string animBoolName, EnemyMeele enemyMeele) : base(etity, stateMachine, animBoolName)
    {
        this.enemyMeele = enemyMeele;
    }
    public override void Enter()
    {

        base.Enter();

        enemyMeele.agent.isStopped = true;
        enemyMeele.agent.ResetPath();

        celebrateIndex = Random.Range(0, 3);

        enemyMeele.anim.SetInteger(animBoolName, celebrateIndex);
        enemyMeele.anim.SetTrigger("isCelebrate");
    }

    public override void Exit()
    {
        base.Exit();
        enemyMeele.anim.SetInteger(animBoolName, -1); 
        enemyMeele.anim.ResetTrigger("isCelebrate");
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}
