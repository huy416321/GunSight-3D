using Fusion;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public float speed = 20f;
    public float lifetime = 3f;

    // Lưu team của người bắn
    public bool isPoliceShooter;

    private float aliveTime;

    public override void Spawned()
    {
        aliveTime = 0f;
    }

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        aliveTime += Time.deltaTime;
        if (aliveTime > lifetime)
        {
            Runner.Despawn(Object);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Gây sát thương nếu trúng player
        if (Object.HasStateAuthority)
        {
            var player = other.GetComponent<PlayerControllerRPC>();
            if (player != null)
            {
                float damage = (player.isPolice == isPoliceShooter) ? 5f : 20f;
                player.TakeDamage(damage);
            }
            Runner.Despawn(Object);
        }
    }
}
