using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AllyPoolManager;

public class AllyRange : Entity
{
    private GameObject rootObject;
    public AR_CelebrateState celebrateState { get; private set; }
    public AR_IdleState idleState { get; private set; }
    public AR_RangeAttackState rangeAttackState { get; private set; }


    public GameObject projectiles;
    public LayerMask enemyLayer;

    public float attackRange;

    public AllyRangeStats stats;
    public Range_AllyConfigLevel upgradeConfig;
    public float attackCooldown;
    public Transform targetEnemy;
    public AllyType allyType = AllyType.Range;
    public Transform firePoint;
   

    public override void Awake()
    {
        firePoint = transform.Find("FirePoint");
        base.Awake();
        var (tier, sub) = AllyLevelManager.Instance.GetCurrentLevel(allyType);
        stats = upgradeConfig.GetStats(tier, sub);
        if (stats == null)
        {
            Debug.LogError($"Cant find AllyRangeStats for Tier {tier} - Sub {sub}");
            return;
        }
        attackRange = stats.attackRange;
        currentHealth = stats.health;
        rootObject = transform.parent.gameObject;
        celebrateState = new AR_CelebrateState(this, stateMachine, "Victory", this);
        idleState = new AR_IdleState(this, stateMachine, "Speed", this);
        rangeAttackState = new AR_RangeAttackState(this, stateMachine, "Attack", this);
        stateMachine.Initialize(idleState);
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
    public void AnimationTrigger_Attack()
    {
        rangeAttackState.TriggerAttack();
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
            1 => "gunpistol",
            2 => "gunlazer",
            3 => "gunrifle"
        };

        AudioManager.Instance.Play(sfxName);
    }
}
