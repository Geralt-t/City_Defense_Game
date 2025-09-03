using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AM_IdleState : State
{
    public AllyMeele allyMeele;
    public AM_IdleState(Entity entity, FiniteStateMachine stateMachine, string animBoolName, AllyMeele allyMeele)
        : base(entity, stateMachine, animBoolName)
    {
        this.allyMeele = allyMeele;
    }
    public override void Enter()
    {
        base.Enter();
        allyMeele.anim.SetFloat(animBoolName, 0f);
    }
    public override void Exit()
    {
        base.Exit();
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        if (UIManager.Instance.GetCurrentState() == UIManager.GameState.Win)
        {
            stateMachine.ChangeState(allyMeele.celebrateState);
        }
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        Collider[] enemies = Physics.OverlapSphere(allyMeele.transform.position, allyMeele.detectionRange, allyMeele.enemyLayer);

        if (enemies.Length > 0)
        {
            allyMeele.targetEnemy = allyMeele.GetNearestEnemy(enemies);
            float distance = Vector3.Distance(allyMeele.transform.position, allyMeele.targetEnemy.position);

            if (distance <= allyMeele.attackRange)
                stateMachine.ChangeState(allyMeele.meeleAttackState);
            else if(distance > allyMeele.attackRange || IsEnemyInCameraView(allyMeele.targetEnemy.transform))
            {
                stateMachine.ChangeState(allyMeele.moveToEnemyState);
            }
        }
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
