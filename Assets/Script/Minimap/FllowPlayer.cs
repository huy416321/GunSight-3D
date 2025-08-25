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
            if (playerCamera != null && Object.HasInputAuthority)
        {
            playerCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
            // Lấy trạng thái ngắm từ PlayerSkillController
            Vector3 cameraOffset = new Vector3(0, 15f, 0); // Độ cao camera
        }
    }
}
