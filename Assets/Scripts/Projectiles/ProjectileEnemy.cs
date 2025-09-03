using UnityEngine;

public class ProjectileEnemy : MonoBehaviour
{
    public float launchForce = 15f; // L?c phóng ban ??u
    // public float gravityScale = 1f; // Không dùng tr?c ti?p trong code này n?a
    public float arcHeightMultiplier = 0.5f; // Chi?u cao cung bay
    public float lifetime = 5f; // Th?i gian t?i ?a projectile t?n t?i tr??c khi t? h?y/v? pool
    private AttackDetails attackDetails;
    private Vector3 targetPosition; // V? trí m?c tiêu (dùng ?? tính toán cung bay ban ??u)
    private Vector3 startPosition;  // V? trí b?t ??u
    private float journeyLength;    // T?ng quãng ???ng
    private float startTime;        // Th?i gian b?t ??u bay
    private bool isFlying = false;  // ?ang bay?
    private bool hasHit = false;    // ?ánh d?u ?ã va ch?m ?? tránh g?i nhi?u l?n

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
       
        // ? Gán giá tr? damage vào attackDetails
        attackDetails = new AttackDetails
        {
            damageAmount = damage,
            position = startPos,
            // B?n có th? thêm các field khác n?u AttackDetails có, ví d?:
            // attacker = this.gameObject, // n?u c?n
        };

        gameObject.SetActive(true);

        Invoke(nameof(ReturnProjectileSafely), lifetime);
    }


    private void Update()
    {
        if (!isFlying || hasHit) return; // N?u ?ã hit ho?c không bay, d?ng update

        float timeSinceStart = Time.time - startTime;
        // Projectile bay theo h??ng ??n targetPosition, không ph? thu?c vào va ch?m
        float normalizedTime = timeSinceStart * launchForce / journeyLength;

        if (normalizedTime >= 1.0f)
        {
            
        }

        Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, normalizedTime); // V? trí ngang
        float arcHeight = arcHeightMultiplier * journeyLength;
        float yOffset = arcHeight * (normalizedTime - normalizedTime * normalizedTime); // Tính ?? cao cung

        currentPos.y += yOffset; // Áp d?ng ?? cao cung
        transform.position = currentPos;
    }

    // --- Thay ??i l?n ? ?ây: Logic sát th??ng và tr? v? pool n?m trong OnTriggerEnter ---
    private void OnTriggerEnter(Collider other)
    {
        // Ki?m tra xem projectile có ?ang bay và ch?a gây sát th??ng không
        if (!isFlying || hasHit) return;

        // Ki?m tra xem có va ch?m v?i Ally hay không
        if (other.CompareTag("Ally"))
        {
            Debug.Log($"Projectile hit an Ally: {other.name}");
            hasHit = true; // ?ánh d?u ?ã hit ?? ch? gây sát th??ng m?t l?n

            // Gây sát th??ng cho ??ng minh
            other.SendMessage("Damage", attackDetails, SendMessageOptions.DontRequireReceiver);

            // H?y Invoke (timer t? h?y) vì ?ã va ch?m
            CancelInvoke(nameof(ReturnProjectileSafely));

            // Tr? projectile v? pool sau khi gây sát th??ng
            ReturnProjectileSafely();
        }
        // B?n có th? thêm các ?i?u ki?n va ch?m khác, ví d?: ch?m ??t, ch?m t??ng
        else if (other.CompareTag("Road"))
        {
            Debug.Log($"Projectile hit non-ally object: {other.name}");
            hasHit = true;
            CancelInvoke(nameof(ReturnProjectileSafely));
            ReturnProjectileSafely();
        }
    }

    // Hàm an toàn ?? tr? projectile v? pool, ???c g?i t? OnTriggerEnter ho?c Invoke
    private void ReturnProjectileSafely()
    {
        isFlying = false;
        if (pool != null)
        {
            pool.ReturnToPool(gameObject);
        }
        else
        {
            Destroy(gameObject); // D? phòng
        }
    }
}