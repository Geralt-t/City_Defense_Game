using UnityEngine;

public class M_AttackState : State
{
    private EnemyMeele enemyMeele;
    private Transform targetAlly;
    protected AttackDetails attackDetails;
 
    public M_AttackState(Entity entity, FiniteStateMachine stateMachine, string animBoolName, EnemyMeele enemyMeele)
        : base(entity, stateMachine, animBoolName)
    {
        this.enemyMeele = enemyMeele;
    }

    public override void Enter()
    {
        base.Enter();
        enemyMeele.agent.isStopped = true;
        enemyMeele.agent.ResetPath();
        attackDetails.damageAmount = enemyMeele.enemyMeeleStats.damage;
        targetAlly = enemyMeele.GetNearestAlly();

        if (targetAlly != null)
        {
            enemyMeele.LookAtTarget(targetAlly.position);
            enemyMeele.anim.SetBool(animBoolName, true); 
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (!enemyMeele.IsAllyInAttackRange())
        {
            // N?u chase time v??t quá gi?i h?n ? quay v? MoveToLine
            if (enemyMeele.totalChaseTime > enemyMeele.maxChaseTime)
            {
                stateMachine.ChangeState(enemyMeele.moveToLineState);
            }
            else
            {
                stateMachine.ChangeState(enemyMeele.chaseState);
            }
            return;
        }

        targetAlly = enemyMeele.GetNearestAlly();

        if (targetAlly != null)
        {
            enemyMeele.LookAtTarget(targetAlly.position);
        }
    }

    public override void Exit()
    {
        base.Exit();
        enemyMeele.anim.SetBool(animBoolName, false); 
        targetAlly = null;
    }
    public void TriggerAttackEnemyMeele()
    {

        Collider[] detectedObjects = Physics.OverlapSphere(enemyMeele.transform.position, enemyMeele.attackRange, enemyMeele.allyLayer );

        foreach (Collider collider in detectedObjects)
        {
            collider.transform.SendMessage("Damage", attackDetails, SendMessageOptions.DontRequireReceiver);
        }
}
   
}
