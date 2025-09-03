using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class R_CelebrateState : State
{
    public EnemyRange enemyRange;
    private int celebrateIndex;
    public R_CelebrateState(Entity etity, FiniteStateMachine stateMachine, string animBoolName, EnemyRange enemyRange) : base(etity, stateMachine, animBoolName)
    {
        this.enemyRange = enemyRange;
    }
    public override void Enter()
    {

        base.Enter();

        enemyRange.agent.isStopped = true;
        enemyRange.agent.ResetPath();

        celebrateIndex = Random.Range(0, 3);

        enemyRange.anim.SetInteger(animBoolName, celebrateIndex);
        enemyRange.anim.SetTrigger("isCelebrate");
    }
    public override void Exit()
    {
        base.Exit();
        enemyRange.anim.ResetTrigger("isCelebrate");
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
