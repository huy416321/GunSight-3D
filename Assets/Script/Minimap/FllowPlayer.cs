using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;

public class FllowPlayer : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float offsetX, offsetZ;
    [SerializeField] private float LerpSpeed;

    private void LateUpdate()
    {
        // Chỉ camera local mới follow player
        if (playerCamera != null && Object.HasInputAuthority && playerCamera.gameObject.activeInHierarchy)
        {
            // Đặt góc nhìn camera minimap
            playerCamera.transform.rotation = Quaternion.Euler(90, 0, 0);

            // Camera minimap chỉ follow player local
            Vector3 targetPos = transform.position + new Vector3(offsetX, 15f, offsetZ);
            playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, targetPos, LerpSpeed * Time.deltaTime);
        }
    }
}
