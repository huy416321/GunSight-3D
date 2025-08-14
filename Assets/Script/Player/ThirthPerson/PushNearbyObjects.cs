using System;
using UnityEngine;
using Fusion;

public class PushNearbyObjects : NetworkBehaviour
{
    [Header("Push Settings")]
    public float pushRadius = 5f;
    public float pushForce = 10f;
    public LayerMask pushLayerMask;
    public ForceMode forceMode = ForceMode.Impulse;

    public AudioClip pushSound;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    public void Push()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, pushRadius, pushLayerMask);
        bool foundDestroyWall = false;
        int destroyWallLayer = LayerMask.NameToLayer("DestroyWall");
        foreach (var col in colliders)
        {
            Rigidbody rb = col.attachedRigidbody;
            if (rb != null)
            {
                Vector3 dir = (col.transform.position - transform.position).normalized;
                rb.AddForce(dir * pushForce, forceMode);
            }
            if (col.gameObject.layer == destroyWallLayer)
            {
                foundDestroyWall = true;
                // Tắt collider sau khi đẩy
                col.enabled = false;
            }
        }
        if (foundDestroyWall && pushSound != null)
        {
            Invoke(nameof(PlayPushSound), 0f);
        }

    }

    private void PlayPushSound()
    {
        AudioSource.PlayClipAtPoint(pushSound, transform.position, FootstepAudioVolume);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pushRadius);
    }
}
