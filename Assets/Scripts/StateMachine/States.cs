using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class State
{
    protected FiniteStateMachine stateMachine;
    protected Entity entity;


    public float startTime { get; protected set; }
    protected string animBoolName;

    public State(Entity etity, FiniteStateMachine stateMachine, string animBoolName)
    {
        this.entity = etity;
        this.stateMachine = stateMachine;
        this.animBoolName = animBoolName;

    }

    public virtual void Enter()
    {
        startTime = Time.time;
   
       
    }

    public virtual void Exit()
    {
      
    }

    public virtual void LogicUpdate()
    {

    }

    public virtual void PhysicsUpdate()
    {
        
    }

   
}