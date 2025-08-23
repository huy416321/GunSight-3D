using Fusion;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public float speed = 20f;
    public float lifetime = 3f;

    public float damage = 10f; 

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
        // Nếu va chạm với layer Wall thì despawn ngay
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            if (Object.HasStateAuthority)
                Runner.Despawn(Object);
            return;
        }
        // Gây sát thương nếu trúng player
        if (Object.HasStateAuthority)
        {
            var player = other.GetComponent<PlayerControllerRPC>();
            if (player != null)
            {
                float finalDamage = (player.isPolice == isPoliceShooter) ? damage * 0.25f : damage;
                player.TakeDamage(finalDamage);
            }
            Runner.Despawn(Object);
        }
    }
}
