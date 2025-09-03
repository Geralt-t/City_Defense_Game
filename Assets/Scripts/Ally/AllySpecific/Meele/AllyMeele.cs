using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using static AllyPoolManager;

public class AllyMeele : Entity
{
    protected GameObject rootObject;
    public AM_CelebrateState celebrateState{get;  set ;}
    public AM_IdleState idleState{get;  set ;}
    public AM_MeeleAttackState meeleAttackState{get; set ;} 
    public AM_MoveToEnemyState moveToEnemyState{get; set ;}
    public GameObject SpecialHitPoint;

    public LayerMask enemyLayer;

    public float detectionRange;

    public float attackRange;

    public AllyMeleeStats stats;
    public Meele_AllyConfigLevel upgradeConfig;
    public float attackCooldown;
    public Transform targetEnemy;
    public AllyType allyType = AllyType.Meele;
    public float realSpeed => agent.velocity.magnitude;
    public override void Awake()
    {
        base.Awake();
        initStats();
        agent.speed = stats.moveSpeed;
        detectionRange = stats.detectionRange;
        attackRange = stats.attackRange;
        currentHealth = stats.health;
        rootObject = transform.parent.gameObject;
        celebrateState = new AM_CelebrateState(this, stateMachine, "Victory", this);
        idleState = new AM_IdleState(this, stateMachine, "Speed", this);
        moveToEnemyState = new AM_MoveToEnemyState(this, stateMachine, "Speed", this);
        meeleAttackState = new AM_MeeleAttackState(this, stateMachine, "Attack", this);
        stateMachine.Initialize(idleState);
    }
    public virtual void initStats()
    {
        var (tier, sub) = AllyLevelManager.Instance.GetCurrentLevel(allyType);
        stats = upgradeConfig.GetStats(tier, sub);
        if (stats == null)
        {
            Debug.LogError($"Cant find AllyMeleeStats for Tier {tier} - Sub {sub}");
            return;
        }
    }
    public override void Update()
    {
        stateMachine.currentState.LogicUpdate();
        if (isDead)
            AllyPoolManager.Instance.ReturnToPool(allyType, rootObject);
    }
    public Transform GetNearestEnemy(Collider[] enemies)
    {
        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (var col in enemies)
        {
            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = col.transform;
            }
        }

        return closest;
    }
    public void MoveToTarget(Transform enemy)
    {
        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
            agent.SetDestination(enemy.position);
        }
    }
    public virtual void Animation_TriggerAttack()
    {
        meeleAttackState.TriggerAttackEnemyMeele();
        PlayShootSFX();
    }
    public void ResetForm()
    {
        currentHealth = stats.health;
    }
    public void PlayShootSFX()
    {
        var (tier, _) = AllyLevelManager.Instance.GetCurrentLevel(allyType);

        string sfxName = tier switch
        {
            1 => "meele1",
            2 => "meele2",
            3 => "meele3"
        };

        AudioManager.Instance.Play(sfxName);
    }
}
