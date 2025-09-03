using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.AI;
using static AllyPoolManager;

public class Barrier : MonoBehaviour
{
    private GameObject rootObject;
    public FiniteStateMachine stateMachine;
    public Rigidbody2D rb { get; private set; }
    public Animator anim { get; private set; }
    protected float currentHealth;
    protected bool isDead;
    //public NavMeshObstacle navMeshObstacle;
    public AllyType allyType = AllyType.Barrier;
    public AllyBarrierStats stats;
    public Barrier_AllyConfigLevel upgradeConfig;
    public virtual void Awake()
    {
        var (tier, sub) = AllyLevelManager.Instance.GetCurrentLevel(allyType);
        stats = upgradeConfig.GetStats(tier, sub);
        if (stats == null)
        {
            Debug.LogError($"Cant find BarrierMeleeStats for Tier {tier} - Sub {sub}");
            return;
        }
        rootObject = transform.parent.gameObject;
        //navMeshObstacle =GetComponent<NavMeshObstacle>();
        anim = GetComponent<Animator>();
        stateMachine = new FiniteStateMachine();
        currentHealth = stats.health;
    }
    public virtual void FixedUpdate()
    {
    }
    public virtual void Update()
    {
        if (isDead)
            AllyPoolManager.Instance.ReturnToPool(allyType, rootObject);
    }
    public virtual void Damage(AttackDetails attackDetails)
    {
        currentHealth -= attackDetails.damageAmount;
        if (currentHealth <= 0)
        { isDead = true; }
    }
    public void ResetForm()
    {
        currentHealth = stats.health;
    }
}
