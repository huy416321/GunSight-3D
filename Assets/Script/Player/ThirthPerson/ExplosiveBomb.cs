using UnityEngine;
using Fusion;

public class ExplosiveBomb : NetworkBehaviour
{
    [Header("Explosion Settings")]
    public float explosionDelay = 2f;
    public float damageRadius = 8f;
    public int damage = 40;
    public GameObject explosionEffect;
    public AudioClip explosionSound;
    [Range(0, 1)] public float explosionVolume = 0.8f;

    private bool exploded = false;

    private void Start()
    {
        Invoke(nameof(Explode), explosionDelay);
    }

    private void Explode()
    {
        if (exploded) return;
        exploded = true;
        // Hiệu ứng nổ
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        if (explosionSound != null)
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, explosionVolume);
        // Gây sát thương và đẩy các vật trong bán kính
        Collider[] hits = Physics.OverlapSphere(transform.position, damageRadius);
        int destroyWallLayer = LayerMask.NameToLayer("DestroyWall");
        foreach (var hit in hits)
        {
            // Sát thương player
            var health = hit.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
            // Đẩy các vật có Rigidbody
            var rb = hit.attachedRigidbody;
            if (rb != null && !rb.isKinematic)
            {
                Vector3 dir = (hit.transform.position - transform.position).normalized;
                float force = 20f; // Có thể chỉnh lực nổ
                rb.AddExplosionForce(force, transform.position, damageRadius, 1f, ForceMode.Impulse);
            }
            if (hit.gameObject.layer == destroyWallLayer)
            {
                // Tắt collider sau khi nổ
                hit.enabled = false;
            }
        }
        // Hủy bom sau khi nổ
        if (Object.HasStateAuthority)
            Runner.Despawn(Object);
        else
            Destroy(gameObject);
    }
}
