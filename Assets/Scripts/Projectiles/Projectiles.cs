using UnityEngine;

public class Projectiles : MonoBehaviour
{
    private AttackDetails attackDetails;
    private Rigidbody rb;

    [SerializeField] private float speed = 10f;
    [SerializeField] private LayerMask whatIsTarget;
    [SerializeField] private Transform damagePosition;
    [SerializeField] private float damageRadius = 0.5f;
    [SerializeField] private float timeActivated = 1f;

    private float spawnTime;
    private bool hasHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        spawnTime = Time.time;
        hasHit = false;
    }

    private void FixedUpdate()
    {
        if (hasHit) return;

        Collider[] hitTargets = Physics.OverlapSphere(damagePosition.position, damageRadius, whatIsTarget);
        if (hitTargets.Length > 0)
        {
            hasHit = true;
            foreach (var target in hitTargets)
            {
                target.SendMessage("Damage", attackDetails, SendMessageOptions.DontRequireReceiver);
                break;
            }

            ReturnToPool();
        }

        if (Time.time >= spawnTime + timeActivated)
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        rb.velocity = Vector3.zero;
        ProjectilesPoolManager.Instance.ReturnToPool(gameObject);
    }

    public static void SpawnAndFireProjectile(GameObject projectilePrefab, Vector3 spawnPosition, float damage, Transform target)
    {
        if (projectilePrefab == null || target == null)
            return;

        Vector3 direction = (target.position - spawnPosition).normalized;

        GameObject projectileObj = ProjectilesPoolManager.Instance.GetProjectile();
        projectileObj.transform.position = spawnPosition;

        // ?? QUAN TR?NG: xoay viên ??n theo h??ng b?n
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        // Ví d?: bù xoay tr?c X 90 ?? n?u ??n b? ng?a lên
        lookRotation *= Quaternion.Euler(90, 0, 0);

        projectileObj.transform.rotation = lookRotation;

        Rigidbody projRb = projectileObj.GetComponent<Rigidbody>();
        Projectiles projScript = projectileObj.GetComponent<Projectiles>();
        projectileObj.SetActive(true);

        if (projRb != null)
        {
            projRb.velocity = direction * projScript.speed;
        }

        if (projScript != null)
        {
            projScript.attackDetails = new AttackDetails
            {
                damageAmount = damage,
                position = spawnPosition
            };
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (damagePosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(damagePosition.position, damageRadius);
        }
    }
}
