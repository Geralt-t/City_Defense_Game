using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AM_CelebrateState : State
{
    public AllyMeele allyMeele;
    private float celebrateDuration = 2f;
    private bool finishedCelebration = false;
    public AM_CelebrateState(Entity entity, FiniteStateMachine stateMachine, string animBoolName, AllyMeele allyMeele)
        :base(entity, stateMachine, animBoolName)
    {
        this.allyMeele = allyMeele;
    }
    public override void Enter()
    {
        base.Enter();
        finishedCelebration = false;
        startTime = Time.time;
        // Ng?ng di chuy?n hoàn toàn
        // Kích ho?t animation ?n m?ng (trigger ho?c bool)
        allyMeele.anim.SetTrigger(animBoolName);
    }
    public override void Exit()
    {
        base.Exit();
        allyMeele.anim.ResetTrigger(animBoolName);
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (Time.time >= startTime + celebrateDuration)
        {
            stateMachine.ChangeState(allyMeele.idleState);
            finishedCelebration = true;
        }
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}
