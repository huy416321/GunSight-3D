using UnityEngine;
using Fusion;

public class SimpleNetworkTransform : NetworkBehaviour
{
    [Networked] private Vector3 NetworkPosition { get; set; }
    [Networked] private Quaternion NetworkRotation { get; set; }
    [Networked] private Vector3 NetworkVelocity { get; set; }

    public float InterpSpeed = 15f;
    public float SnapThreshold = 2.0f; // Ngưỡng snap nếu lệch xa
    public float ExtrapolationTime = 0.1f; // Thời gian dự đoán chuyển động

    public override void FixedUpdateNetwork()
    {
        if (HasInputAuthority)
        {
            // Gửi vị trí, rotation và velocity lên mạng
            NetworkPosition = transform.position;
            NetworkRotation = transform.rotation;
            NetworkVelocity = GetVelocity();
        }
        else
        {
            // Dự đoán vị trí dựa trên velocity
            Vector3 predictedPosition = NetworkPosition + NetworkVelocity * ExtrapolationTime;
            // Nếu lệch xa thì snap luôn
            if (Vector3.Distance(transform.position, predictedPosition) > SnapThreshold)
            {
                transform.position = predictedPosition;
                transform.rotation = NetworkRotation;
            }
            else
            {
                // Nội suy mượt mà
                transform.position = Vector3.Lerp(transform.position, predictedPosition, Runner.DeltaTime * InterpSpeed);
                transform.rotation = Quaternion.Slerp(transform.rotation, NetworkRotation, Runner.DeltaTime * InterpSpeed);
            }
        }

    }

    // Tính velocity dựa trên sự thay đổi vị trí
    private Vector3 lastPosition;

    private void Start()
    {
        lastPosition = transform.position;
    }

    private Vector3 GetVelocity()
    {
        Vector3 velocity = (transform.position - lastPosition) / Runner.DeltaTime;
        return velocity;
    }

    private void LateUpdate()
    {
        lastPosition = transform.position;
    }
}
