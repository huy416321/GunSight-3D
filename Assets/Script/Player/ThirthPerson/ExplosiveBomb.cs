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
        // Gây sát thương lên các player trong bán kính
        Collider[] hits = Physics.OverlapSphere(transform.position, damageRadius);
        foreach (var hit in hits)
        {
            var health = hit.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
        // Hủy bom sau khi nổ
        if (Object.HasStateAuthority)
            Runner.Despawn(Object);
        else
            Destroy(gameObject);
    }
}
