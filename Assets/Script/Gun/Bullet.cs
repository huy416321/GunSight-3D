using Fusion;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public float speed = 20f;
    public float lifetime = 3f;

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
        var player = other.GetComponent<PlayerControllerRPC>();
        if (player != null && player != this)
        {
            if (Object.HasStateAuthority)
            {
                player.TakeDamage(20f); // Sát thương mỗi viên đạn, có thể chỉnh theo weapon
            }
        }
        if (Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}
