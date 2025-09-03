using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NukeLogic : MonoBehaviour
{
    public ParticleSystem nukeEffect;
    public ParticleSystem smokeTrail;
    public PlayerController pc;

    
    [SerializeField]
    private NukeStats nukeStats;
    private Rigidbody rb;

    private bool hasDropped = false;
    private bool hasExploded = false;
    protected AttackDetails attackDetails;
    [SerializeField] private LayerMask whatIsTarget;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        attackDetails.damageAmount = nukeStats.Damage;
        smokeTrail.Play();
        nukeEffect.Stop();
        rb.useGravity = true;
        hasDropped = true;
    }
    private void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!hasDropped || hasExploded)
            return;

        // Kích n? khi ch?m "Road"
        if (collision.collider.CompareTag("Road"))
        {
            Explode();
        }
    }

    void Explode()
    {
        hasExploded = true;

        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;

        smokeTrail.Stop();
        nukeEffect.Play();

        // Gây sát th??ng trong bán kính
        Collider[] hits = Physics.OverlapSphere(transform.position, nukeStats.ExplosionRadius, whatIsTarget);

        foreach (var hit in hits)
        {
            hit.transform.SendMessage("Damage", attackDetails, SendMessageOptions.DontRequireReceiver);
            Debug.Log("[Nuke] Gây sát th??ng lên: " + hit.name);
        }

        Destroy(gameObject, 1f);
    }
}
