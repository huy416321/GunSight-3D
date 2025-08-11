using UnityEngine;
using Fusion;

public class SimpleNetworkTransform : NetworkBehaviour
{
    [Networked] private Vector3 NetworkPosition { get; set; }
    [Networked] private Quaternion NetworkRotation { get; set; }

    public float InterpSpeed = 15f;

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            // Sync local transform lên network
            NetworkPosition = transform.position;
            NetworkRotation = transform.rotation;
        }
        else
        {
            // Interpolate từ network về local
            transform.position = Vector3.Lerp(transform.position, NetworkPosition, Runner.DeltaTime * InterpSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, NetworkRotation, Runner.DeltaTime * InterpSpeed);
        }
    }
}