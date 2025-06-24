using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;

public class FlashlightController : NetworkBehaviour
{
    [Header("Flashlight Settings")]
    public Light flashlight;
    public float maxDistance = 20f;
    public LayerMask playerLayer;

    private bool isOn = false;

    // Hàm sự kiện cho Input System
    public void OnToggleFlashlight(InputAction.CallbackContext context)
    {
        if (!Object.HasInputAuthority) return;
        if (context.performed)
        {
            isOn = !isOn;
            flashlight.enabled = isOn;
        }
    }

    private void Update()
    {
        if (!Object.HasInputAuthority) return;

        // Luôn ẩn tất cả player khác trước
        foreach (var p in FindObjectsByType<PlayerControllerRPC>(FindObjectsSortMode.None))
        {
            if (p != null && !p.Object.HasInputAuthority)
            {
                p.SetVisibleForOther(false);
            }
        }

        if (isOn)
        {
            Ray ray = new Ray(flashlight.transform.position, flashlight.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, playerLayer))
            {
                var player = hit.collider.GetComponent<PlayerControllerRPC>();
                if (player != null && !player.Object.HasInputAuthority)
                {
                    // Hiện player bị chiếu sáng
                    player.SetVisibleForOther(true);
                }
            }
        }
    }
}
