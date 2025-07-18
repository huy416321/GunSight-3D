using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;

public class FlashlightController : NetworkBehaviour
{
    [Header("Flashlight Settings")]
    public Light flashlight;
    public float maxDistance = 20f;
    public LayerMask playerLayer;
    public LayerMask wallLayer; // LayerMask cho tường/vật cản
    public float sphereRadius = 1.5f; // Bán kính vùng phát hiện, có thể chỉnh từ script khác

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
            RaycastHit[] hits = Physics.SphereCastAll(ray, sphereRadius, maxDistance, playerLayer);
            foreach (var hit in hits)
            {
                var player = hit.collider.GetComponent<PlayerControllerRPC>();
                if (player != null && !player.Object.HasInputAuthority)
                {
                    // Kiểm tra có bị tường che không
                    Vector3 dir = (player.transform.position - flashlight.transform.position).normalized;
                    float dist = Vector3.Distance(flashlight.transform.position, player.transform.position);
                    if (!Physics.Raycast(flashlight.transform.position, dir, dist, wallLayer))
                    {
                        // Không bị tường che, cho hiện hình
                        player.SetVisibleForOther(true);
                    }
                }
            }
        }
    }
}
