using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyRange : Entity
{
    public R_MoveToLineState moveToLineState { get; private set; }
    public R_RangeAttackState rangeAttackState { get; private set; }
    public R_MeeleAttackState meeleAttackState { get; private set; }
    public R_CelebrateState celebrateState { get; private set; }

    public LayerMask allyLayer;
    public float meeleAttackRange;
    public float rangeAttackRange;
    public EnemyRangeStats enemyRangeStats;
    public GameObject defenseGoalPoint;
    public float attackCooldown;
    public float energyGet;
    public bool isTouchingDefenseLine = false;
    public GameObject popupPrefab;
    public Canvas uiCanvas;
    public GameObject throwPoint;
    public Transform targetAlly;
    public ParticleSystem deathEffect;
    public float speed;


    public float extraDetectionRange = 0;
    public override void Awake()
    {
        anim = GetComponent<Animator>();
        stateMachine = new FiniteStateMachine();
        agent = GetComponent<NavMeshAgent>();

        meeleAttackRange = enemyRangeStats.MeeleAttackRange;
        rangeAttackRange = enemyRangeStats.RangeAttackRange;
        attackCooldown = enemyRangeStats.attackCooldown;
        currentHealth = enemyRangeStats.health;
        energyGet = enemyRangeStats.energyGets;
        defenseGoalPoint = GameObject.FindGameObjectWithTag("DefenseLine");


        moveToLineState = new R_MoveToLineState(this, stateMachine, "Speed", this);
        meeleAttackState = new R_MeeleAttackState(this, stateMachine, "Attack", this);
        rangeAttackState = new R_RangeAttackState(this, stateMachine, "Attack", this);
        celebrateState = new R_CelebrateState(this, stateMachine, "Celebrate", this);
        uiCanvas = FindObjectOfType<Canvas>();
    }
    public void Start()
    {
    }
    public void OnEnable()
    {
        agent.enabled = true;
        agent.speed = enemyRangeStats.speed;

        isDead = false;
        currentHealth = enemyRangeStats.health;
    }
    public void OnDisable()
    {
        agent.enabled = false;
        agent.speed = 0;
        
    }
    public override void Update()
    {
         speed=agent.velocity.magnitude;
        base.Update();
        if (isDead)
        {
            ParticleEffectPool.Instance.SpawnDeathEffect(transform.position);
            EnemyPoolManager.Instance.ReturnToPool(transform.parent.gameObject);
            ShowPopupOnDeath(transform.position);
            EnergyInGame.Instance.GetEnergy(energyGet);
        }
        if (isTouchingDefenseLine && UIManager.Instance.isLose == false)
        {
            UIManager.Instance.SetLose(true);
            isTouchingDefenseLine = false;
        }
    }
    public override void ResetEntity()
    {
        base.ResetEntity();
    
     
        isTouchingDefenseLine = false;

        

        anim.applyRootMotion = false;

        anim.Play("Blend Tree", 0, 0f);
        anim.Rebind();
        anim.Update(0f);
        anim.ResetTrigger("isCelebrate");
        stateMachine.Initialize(moveToLineState);
    }
  
    public Transform GetClosestTargetInRange(float radius)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, allyLayer);
        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (Collider col in hits)
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
    public void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 dir = (targetPosition - transform.position).normalized;
        dir.y = 0;
        transform.forward = dir;
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangeAttackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, meeleAttackRange);
    }
    public void Animation_TriggerAttackRange()
    {

        AudioManager.Instance.Play("enemy2");
        rangeAttackState.TriggerAttackEnemyRange();
    }
    public void Animation_TriggerAttackMeele()
    {

        AudioManager.Instance.Play("enemy1");
        meeleAttackState.TriggerAttackEnemyMeele();
    }

    private void ShowPopupOnDeath(Vector3 worldPos)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        if (screenPos.z < 0) return;

        GameObject popup = Instantiate(popupPrefab, uiCanvas.transform);
        RectTransform popupRect = popup.GetComponent<RectTransform>();
        RectTransform canvasRect = uiCanvas.GetComponent<RectTransform>();

        Vector2 anchoredPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out anchoredPos))
        {
            popupRect.anchoredPosition = anchoredPos;
            popupRect.localScale = Vector3.one;
        }
    }

}
