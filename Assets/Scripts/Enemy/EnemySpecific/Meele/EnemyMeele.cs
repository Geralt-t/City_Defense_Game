using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyMeele : Entity
{
    public M_MoveToLineState moveToLineState { get; private set; }
    public M_ChaseState chaseState { get; private set; }
    public M_AttackState attackState { get; private set; }
    public M_CelebrateState celebrateState { get; private set; }
    public EnemyMeeleStats enemyMeeleStats;
    public GameObject defenseGoalPoint;
    public LayerMask allyLayer;

    public float detectionRange;
    public float attackRange;
    public float attackCooldown ;
    public float energyGet;
    public ParticleSystem deathEffect;
    public GameObject popupPrefab; 
    public Canvas uiCanvas;
    public bool isTouchingDefenseLine=false;
    public float totalChaseTime = 0f;
    public float maxChaseTime = 1.5f;
    public override void Awake()
    {
        anim = GetComponent<Animator>();
        stateMachine = new FiniteStateMachine();
        agent = GetComponent<NavMeshAgent>();
        currentHealth =enemyMeeleStats.health;
        detectionRange = enemyMeeleStats.detectionRange;
        attackRange = enemyMeeleStats.attackRange;
        
        attackCooldown = enemyMeeleStats.attackCooldown;
        energyGet = enemyMeeleStats.energyGets;

        defenseGoalPoint = GameObject.FindGameObjectWithTag("DefenseLine");
        moveToLineState = new M_MoveToLineState(this, stateMachine, "Speed", this);
        chaseState = new M_ChaseState(this, stateMachine, "Speed", this);
        attackState = new M_AttackState(this, stateMachine, "Attack", this);
        celebrateState = new M_CelebrateState(this, stateMachine, "Celebrate", this);
        uiCanvas = FindObjectOfType<Canvas>();
    }

    public override void Update()
    {
        base.Update();
        if (isDead)
        {
            ParticleEffectPool.Instance.SpawnDeathEffect(transform.position);
            EnemyPoolManager.Instance.ReturnToPool(transform.parent.gameObject);
            ShowPopupOnDeath(transform.position);
            EnergyInGame.Instance.GetEnergy(energyGet);
        }
        if (isTouchingDefenseLine && UIManager.Instance.isLose==false)
        {
            UIManager.Instance.SetLose(true);
            isTouchingDefenseLine = false;
        }
    }
    public void Start()
    {
      
    }
    public void OnEnable()
    {
        agent.enabled = true;
        agent.speed = enemyMeeleStats.speed;
        isDead = false;
        currentHealth = enemyMeeleStats.health;

    }
    public void OnDisable()
    {
        agent.enabled = false;
        agent.speed = 0;

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

       
            stateMachine.Initialize(moveToLineState); // G?i tr?c ti?p n?u ch?c ch?n ?ã vào NavMesh
       
    }
   

    public Transform GetNearestAlly()
    {
        Collider[] allies = Physics.OverlapSphere(transform.position, detectionRange, allyLayer);
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (var ally in allies)
        {
            if (!ally.gameObject.activeInHierarchy) continue;

            float distance = Vector3.Distance(transform.position, ally.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = ally.transform;
            }
        }

        return nearest;
    }


    public bool IsAllyInAttackRange()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, allyLayer);
        return hits.Length > 0;
    }

    public void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 dir = (targetPosition - transform.position).normalized;
        dir.y = 0;
        transform.forward = dir;
    }

    public void AnimationTrigger_Attack()
    {
        attackState.TriggerAttackEnemyMeele();
        AudioManager.Instance.Play("enemy1");
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
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
