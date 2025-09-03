using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AR_RangeAttackState : State
{
    public AllyRange allyRange;
    protected AllyRangeStats rangeData;
    public Projectiles bulletScript;
    public AR_RangeAttackState(Entity entity, FiniteStateMachine stateMachine, string animBoolName, AllyRange allyRange)
        : base(entity, stateMachine, animBoolName)
    {
        this.allyRange = allyRange;
        this.rangeData = allyRange.stats;
    }
    public override void Enter()
    {

        base.Enter();
        LookAtTarget();
        allyRange.anim.SetBool(animBoolName, true);
    }
    public override void Exit()
    {
        base.Exit();
        allyRange.anim.SetBool(animBoolName, false) ;
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (UIManager.Instance.GetCurrentState() == UIManager.GameState.Win)
        {
            stateMachine.ChangeState(allyRange.idleState);
        }
        if (UIManager.Instance.GetCurrentState() == UIManager.GameState.Lose)
        {
            stateMachine.ChangeState(allyRange.idleState);
        }
        if (allyRange.targetEnemy == null ||!allyRange.targetEnemy.gameObject.activeInHierarchy ||!IsEnemyInCameraView(allyRange.targetEnemy))
        {
            allyRange.targetEnemy = null;
            stateMachine.ChangeState(allyRange.idleState);
            LookAtTarget();
            return;
        }

        float distance = Vector3.Distance(allyRange.transform.position, allyRange.targetEnemy.position);

      
      
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
    private void LookAtTarget()
    {
        if (allyRange.targetEnemy == null) return;

        Vector3 dir = (allyRange.targetEnemy.position - allyRange.transform.position).normalized;
        dir.y = 0f;
        if (dir != Vector3.zero)
            allyRange.transform.rotation = Quaternion.LookRotation(dir);
    }
    public void TriggerAttack()
    {
        if (allyRange.firePoint == null || allyRange.targetEnemy == null|| !allyRange.targetEnemy.gameObject.activeInHierarchy) return;

        Projectiles.SpawnAndFireProjectile(allyRange.projectiles,allyRange.firePoint.position, rangeData.attackDamage,allyRange.targetEnemy );
        Debug.DrawLine(allyRange.firePoint.position, allyRange.targetEnemy.position, Color.red, 2f);

    }
    private bool IsEnemyInCameraView(Transform enemy)
    {
        if (enemy == null) return false;

        Vector3 viewportPos = Camera.main.WorldToViewportPoint(enemy.position);

        // viewportPos.z > 0: phía tr??c camera
        // x, y trong [0, 1]: n?m trong khung hình
        return viewportPos.z > 0 &&
               viewportPos.x >= 0 && viewportPos.x <= 1 &&
               viewportPos.y >= 0 && viewportPos.y <= 1;
    }

}
