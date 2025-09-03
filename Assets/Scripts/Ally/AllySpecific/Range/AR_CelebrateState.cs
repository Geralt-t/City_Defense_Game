using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AR_CelebrateState : State
{
    public AllyRange allyRange;
    private float celebrateDuration = 2f;
    public AR_CelebrateState(Entity entity, FiniteStateMachine stateMachine, string animBoolName, AllyRange allyRange)
        : base(entity, stateMachine, animBoolName)
    {
        this.allyRange = allyRange;
    }
    public override void Enter()
    {
        base.Enter();
        startTime = Time.time;
        allyRange.anim.SetTrigger(animBoolName);
    }
    public override void Exit()
    {
        base.Exit();
        allyRange.anim.ResetTrigger(animBoolName);
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (Time.time >= startTime + celebrateDuration)
        {
            stateMachine.ChangeState(allyRange.idleState);
            
        }
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}
