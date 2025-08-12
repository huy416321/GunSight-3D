using UnityEngine;
using Fusion;

public class SimpleNetworkTransform : NetworkBehaviour
{
    [Networked] private Vector3 NetworkPosition { get; set; }
    [Networked] private Quaternion NetworkRotation { get; set; }

    public float InterpSpeed = 15f;

    public override void FixedUpdateNetwork()
    {
        if (HasInputAuthority)
        {
            Debug.Log($"[SNT] HasInputAuthority: Syncing position {transform.position} and rotation {transform.rotation}");
            NetworkPosition = transform.position;
            NetworkRotation = transform.rotation;
        }
        else
        {
            Debug.Log($"[SNT] No InputAuthority: Interpolating to {NetworkPosition} / {NetworkRotation}");
            transform.position = Vector3.Lerp(transform.position, NetworkPosition, Runner.DeltaTime * InterpSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, NetworkRotation, Runner.DeltaTime * InterpSpeed);
        }
    }
}