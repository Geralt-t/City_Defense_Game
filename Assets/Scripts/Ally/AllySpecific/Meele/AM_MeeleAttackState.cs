using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AM_MeeleAttackState : State
{
    public AllyMeele allyMeele;
    private float lastAttackTime;
    protected AttackDetails attackDetails;
    public AM_MeeleAttackState(Entity entity, FiniteStateMachine stateMachine, string animBoolName, AllyMeele allyMeele)
        : base(entity, stateMachine, animBoolName)
    {
        this.allyMeele = allyMeele;
    }
    public override void Enter()
    {

        base.Enter();
        allyMeele.agent.isStopped = true;
        attackDetails.damageAmount = allyMeele.stats.damage;
        LookAtTarget();
        lastAttackTime = Time.time;
        allyMeele.anim.SetBool(animBoolName, true);
        
        if (allyMeele is SpecialUnit special)
        {
            special.PlayAttackEffect();
        }
    }
    public override void Exit()
    {
        base.Exit();
        allyMeele.anim.SetBool(animBoolName, false);
        if (allyMeele is SpecialUnit special)
        {
            special.StopAttackEffect();
        }
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
      

        if (allyMeele.targetEnemy == null || !allyMeele.targetEnemy.gameObject.activeInHierarchy)
        {
            stateMachine.ChangeState(allyMeele.idleState);
            return;
        }

        float distance = Vector3.Distance(allyMeele.transform.position, allyMeele.targetEnemy.position);

        if (distance > allyMeele.attackRange)
        {
            stateMachine.ChangeState(allyMeele.moveToEnemyState);
            return;
        }

        LookAtTarget();

        if (Time.time >= lastAttackTime + allyMeele.attackCooldown)
        {
            lastAttackTime = Time.time;
        }
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
    private void LookAtTarget()
    {
        if (allyMeele.targetEnemy == null) return;

        Vector3 dir = (allyMeele.targetEnemy.position - allyMeele.transform.position).normalized;
        dir.y = 0f;
        if (dir != Vector3.zero)
            allyMeele.transform.rotation = Quaternion.LookRotation(dir);
    }
    public void TriggerAttackEnemyMeele()
    {
       

        Collider[] detectedObjects;
        Vector3 attackOrigin = (allyMeele is SpecialUnit special) ? special.SpecialHitPoint.transform.position : allyMeele.transform.position;

        detectedObjects = Physics.OverlapSphere(attackOrigin, allyMeele.attackRange, allyMeele.enemyLayer);

        if (allyMeele is SpecialUnit specialUnit)
        {
            foreach (Collider collider in detectedObjects)
            {
                if (collider.gameObject.activeInHierarchy)
                {
                    collider.SendMessage("Damage", attackDetails, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
        else
        {
            Transform closestEnemy = null;
            float minDistance = float.MaxValue;

            foreach (Collider collider in detectedObjects)
            {
                if (collider.gameObject.activeInHierarchy)
                {
                    float distance = Vector3.Distance(attackOrigin, collider.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestEnemy = collider.transform;
                    }
                }
            }

            if (closestEnemy != null)
            {
                closestEnemy.SendMessage("Damage", attackDetails, SendMessageOptions.DontRequireReceiver);
            }
        }

    }
  

}

