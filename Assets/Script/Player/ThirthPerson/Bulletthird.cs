using Fusion;
using UnityEngine;

public class Bulletthird : NetworkBehaviour
{
    public float speed = 20f;
    public float lifetime = 3f;

    public float damage; 

    // Lưu team của người bắn
    public bool isPoliceShooter;

    private float aliveTime;

    public override void Spawned()
    {
    aliveTime = 0f;
    Debug.Log($"[Bulletthird] Spawned on client {Runner.LocalPlayer.PlayerId}, ObjectId: {Object.Id}");
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
            Debug.Log($"[Bulletthird] OnTriggerEnter on client {Runner.LocalPlayer.PlayerId}, ObjectId: {Object.Id}, Hit: {other.name}");
            var playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                float finalDamage = (playerHealth.isPolice == isPoliceShooter) ? damage * 1f : damage * 2f;
                playerHealth.TakeDamage(finalDamage);
            }
            Runner.Despawn(Object);
        }
    }
}
