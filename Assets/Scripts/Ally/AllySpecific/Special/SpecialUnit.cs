using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AllyPoolManager;
using UnityEngine.UIElements;
using UnityEngine.AI;

public class SpecialUnit : AllyMeele
{
    public bool isPlay = false;
    public override void Awake()
    {
       base.Awake();
        
    }
 public override void initStats()
    {

    }
    public ParticleSystem attackEffect;
    public override void Update()
    {
        stateMachine.currentState.LogicUpdate();
        if (isDead || UIManager.Instance.CurrentState != UIManager.GameState.InGame)
        {
            Object.Destroy(rootObject);
            isPlay = false;
        }
    }
    public void PlayAttackEffect()
    {
        if (attackEffect != null && !attackEffect.isPlaying)
            attackEffect.Play();
    }

    public void StopAttackEffect()
    {
        if (attackEffect != null && attackEffect.isPlaying)
            attackEffect.Stop();
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    public override void Animation_TriggerAttack()
    {
        meeleAttackState.TriggerAttackEnemyMeele();
        if (isPlay == false)
        {
            AudioManager.Instance.Play("special");
            isPlay = true;
        }
    }
}
