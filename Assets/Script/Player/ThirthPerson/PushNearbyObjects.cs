using UnityEngine;

public class PushNearbyObjects : MonoBehaviour
{
    [Header("Push Settings")]
    public float pushRadius = 5f;
    public float pushForce = 10f;
    public LayerMask pushLayerMask;
    public ForceMode forceMode = ForceMode.Impulse;

    public void Push()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, pushRadius, pushLayerMask);
        foreach (var col in colliders)
        {
            Rigidbody rb = col.attachedRigidbody;
            if (rb != null)
            {
                Vector3 dir = (col.transform.position - transform.position).normalized;
                rb.AddForce(dir * pushForce, forceMode);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pushRadius);
    }
}
