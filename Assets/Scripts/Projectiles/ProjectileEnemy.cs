using UnityEngine;

public class ProjectileEnemy : MonoBehaviour
{
    public float launchForce = 15f; // L?c ph�ng ban ??u
    // public float gravityScale = 1f; // Kh�ng d�ng tr?c ti?p trong code n�y n?a
    public float arcHeightMultiplier = 0.5f; // Chi?u cao cung bay
    public float lifetime = 5f; // Th?i gian t?i ?a projectile t?n t?i tr??c khi t? h?y/v? pool
    private AttackDetails attackDetails;
    private Vector3 targetPosition; // V? tr� m?c ti�u (d�ng ?? t�nh to�n cung bay ban ??u)
    private Vector3 startPosition;  // V? tr� b?t ??u
    private float journeyLength;    // T?ng qu�ng ???ng
    private float startTime;        // Th?i gian b?t ??u bay
    private bool isFlying = false;  // ?ang bay?
    private bool hasHit = false;    // ?�nh d?u ?� va ch?m ?? tr�nh g?i nhi?u l?n

    private EnemyProjectilesPool pool; // Tham chi?u ??n pool

    private void Awake()
    {
        pool = EnemyProjectilesPool.Instance; // L?y instance c?a pool
    }

    public void Launch(Vector3 startPos, Vector3 targetPos, float damage)
    {
        transform.position = startPos;
        startPosition = startPos;
        targetPosition = targetPos;
        journeyLength = Vector3.Distance(startPosition, targetPos);
        startTime = Time.time;
        isFlying = true;
        hasHit = false;

        transform.rotation = Quaternion.identity;

        Rigidbody rb = GetComponent<Rigidbody>();
       
        // ? G�n gi� tr? damage v�o attackDetails
        attackDetails = new AttackDetails
        {
            damageAmount = damage,
            position = startPos,
            // B?n c� th? th�m c�c field kh�c n?u AttackDetails c�, v� d?:
            // attacker = this.gameObject, // n?u c?n
        };

        gameObject.SetActive(true);

        Invoke(nameof(ReturnProjectileSafely), lifetime);
    }


    private void Update()
    {
        if (!isFlying || hasHit) return; // N?u ?� hit ho?c kh�ng bay, d?ng update

        float timeSinceStart = Time.time - startTime;
        // Projectile bay theo h??ng ??n targetPosition, kh�ng ph? thu?c v�o va ch?m
        float normalizedTime = timeSinceStart * launchForce / journeyLength;

        if (normalizedTime >= 1.0f)
        {
            
        }

        Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, normalizedTime); // V? tr� ngang
        float arcHeight = arcHeightMultiplier * journeyLength;
        float yOffset = arcHeight * (normalizedTime - normalizedTime * normalizedTime); // T�nh ?? cao cung

        currentPos.y += yOffset; // �p d?ng ?? cao cung
        transform.position = currentPos;
    }

    // --- Thay ??i l?n ? ?�y: Logic s�t th??ng v� tr? v? pool n?m trong OnTriggerEnter ---
    private void OnTriggerEnter(Collider other)
    {
        // Ki?m tra xem projectile c� ?ang bay v� ch?a g�y s�t th??ng kh�ng
        if (!isFlying || hasHit) return;

        // Ki?m tra xem c� va ch?m v?i Ally hay kh�ng
        if (other.CompareTag("Ally"))
        {
            Debug.Log($"Projectile hit an Ally: {other.name}");
            hasHit = true; // ?�nh d?u ?� hit ?? ch? g�y s�t th??ng m?t l?n

            // G�y s�t th??ng cho ??ng minh
            other.SendMessage("Damage", attackDetails, SendMessageOptions.DontRequireReceiver);

            // H?y Invoke (timer t? h?y) v� ?� va ch?m
            CancelInvoke(nameof(ReturnProjectileSafely));

            // Tr? projectile v? pool sau khi g�y s�t th??ng
            ReturnProjectileSafely();
        }
        // B?n c� th? th�m c�c ?i?u ki?n va ch?m kh�c, v� d?: ch?m ??t, ch?m t??ng
        else if (other.CompareTag("Road"))
        {
            Debug.Log($"Projectile hit non-ally object: {other.name}");
            hasHit = true;
            CancelInvoke(nameof(ReturnProjectileSafely));
            ReturnProjectileSafely();
        }
    }

    // H�m an to�n ?? tr? projectile v? pool, ???c g?i t? OnTriggerEnter ho?c Invoke
    private void ReturnProjectileSafely()
    {
        isFlying = false;
        if (pool != null)
        {
            pool.ReturnToPool(gameObject);
        }
        else
        {
            Destroy(gameObject); // D? ph�ng
        }
    }
}