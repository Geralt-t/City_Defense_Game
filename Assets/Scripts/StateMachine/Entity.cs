using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Entity : MonoBehaviour
{
    public FiniteStateMachine stateMachine;
    public Rigidbody rb { get; private set; }
    public Animator anim { get;  set; }
    protected float currentHealth;
    public Vector3 spawnPosition;
    public NavMeshAgent agent { get; set; }
    public bool isDead;
    public virtual void Awake()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        stateMachine = new FiniteStateMachine();

    }
   
    public virtual void ResetEntity()
    {
        isDead = false;
        // Reset animation, debuff, v.v. n?u có
    }
    public virtual void Update()
    {
        if (stateMachine?.currentState != null)
            stateMachine.currentState.LogicUpdate();
    }
    public virtual void FixedUpdate()
    {
        if (stateMachine?.currentState != null)
            stateMachine.currentState.PhysicsUpdate();
    }
    public virtual void Damage(AttackDetails attackDetails)
    {
        currentHealth -= attackDetails.damageAmount;
        if (currentHealth <= 0)
        { isDead = true; }
    }
}