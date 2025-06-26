using UnityEngine;
using Fusion;
using System.Collections;

public class Grenade : NetworkBehaviour
{
    public float explosionDelay = 2f;
    public float explosionRadius = 4f;
    public float explosionForce = 700f;
    public GameObject explosionEffectPrefab;

    private bool exploded = false;

    public override void Spawned()
    {
        StartCoroutine(ExplodeAfterDelay());
    }

    private IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(explosionDelay);
        Explode();
    }

    private void Explode()
    {
        if (exploded) return;
        exploded = true;

        if (explosionEffectPrefab)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }

            // Gây sát thương nếu là player khác (tùy ý)
            // var player = hit.GetComponent<PlayerControllerRPC>();
            // if (player != null) { player.RPC_Die(); }
        }

        Runner.Despawn(Object);
    }
}