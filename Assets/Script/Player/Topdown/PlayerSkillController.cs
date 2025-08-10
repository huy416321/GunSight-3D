using System.Collections;
using UnityEngine;
using Fusion;

public class PlayerSkillController : NetworkBehaviour
{
    [Header("Camera Reference")]
    public Camera playerCamera;
    [Header("Skill Settings")]
    public float revealDuration = 5f;
    private Coroutine revealCoroutine;
    private bool canUseRevealSkill = true;
    public float revealCooldown = 10f;
    private bool isRevealing = false;

    [Header("Field of View Reference")]
    public FieldOfViewMesh fieldOfViewMesh;
    private float defaultViewAngle;
    private float defaultViewRadius;
    private bool fovAimed = false;

    [Header("Dash Push Skill Settings")]
    public float dashForce = 20f;
    public float dashDistance = 3f;
    public float dashPushCooldown = 8f;
    public float pushRadius = 2f;
    public float pushUpward = 0.5f;
    public LayerMask pushLayerMask;
    private bool canUseDashPushSkill = true;
    private Coroutine dashPushCooldownCoroutine;

    [Header("Aiming")]
    public bool isAiming = false;

    [Header("Animation")]
    public Animator animator; // Thêm biến Animator cho animation dash

    // Dash theo hướng input
    private Vector3 dashInputDirection = Vector3.zero;
    private Vector2 moveInput = Vector2.zero; // Lưu input từ New Input System

    // Kích hoạt kỹ năng: nhìn thấy tất cả player trong 5 giây
    public void ActivateRevealAllSkill()
    {
        Debug.Log("Kích hoạt kỹ năng Revealing All Players");
        if (!canUseRevealSkill) return;
        if (!Object.HasInputAuthority) return; // Chỉ local player mới được phép kích hoạt
        if (revealCoroutine != null)
            StopCoroutine(revealCoroutine);
        if (animator != null)
            animator.SetTrigger("RevealSkill"); // Gọi animation reveal
        StartCoroutine(RevealAllSkillWithStun());
    }

    private IEnumerator RevealAllSkillWithStun()
    {
        // Không cho di chuyển 1 giây
        var playerController = GetComponent<PlayerControllerRPC>();
        float originalMoveSpeed = 5f;
        if (playerController != null)
        {
            originalMoveSpeed = playerController.moveSpeed;
            playerController.moveSpeed = 5f;
        }
        yield return new WaitForSeconds(0.5f);
        if (playerController != null)
            playerController.moveSpeed = originalMoveSpeed;
        // Sau khi "stun" xong thì mới bắt đầu reveal
        revealCoroutine = StartCoroutine(RevealAllPlayersCoroutine());
        StartCoroutine(RevealSkillCooldownCoroutine());
    }

    public bool IsRevealing() => isRevealing;

    private IEnumerator RevealSkillCooldownCoroutine()
    {
        canUseRevealSkill = false;
        yield return new WaitForSeconds(revealCooldown);
        canUseRevealSkill = true;
    }

    private void UpdateFOVState()
    {
        if (fieldOfViewMesh == null)
            return;
        if (!fovAimed)
        {
            defaultViewAngle = fieldOfViewMesh.viewAngle;
            defaultViewRadius = fieldOfViewMesh.viewRadius;
            fovAimed = true;
        }
        if (isAiming)
        {
            fieldOfViewMesh.viewAngle = 10f;
            fieldOfViewMesh.viewRadius = 25f; // Tăng bán kính vùng nhìn khi ngắm
            // Có thể tăng viewRadius nếu muốn khi aim
        }
        else if (isRevealing)
        {
            fieldOfViewMesh.viewAngle = defaultViewAngle * 1.5f;
            fieldOfViewMesh.viewRadius = defaultViewRadius * 1.5f;
        }
        else
        {
            fieldOfViewMesh.viewAngle = defaultViewAngle;
            fieldOfViewMesh.viewRadius = defaultViewRadius;
        }
    }

    private IEnumerator RevealAllPlayersCoroutine()
    {
        isRevealing = true;
        UpdateFOVState();
        // Hiện tất cả player khác chỉ trên máy local
        foreach (var p in FindObjectsByType<PlayerControllerRPC>(FindObjectsSortMode.None))
        {
            if (p != null && !p.Object.HasInputAuthority)
                p.SetVisibleForOther(true);
        }
        yield return new WaitForSeconds(revealDuration);
        // Ẩn lại tất cả player khác (nếu không bị chiếu đèn pin)
        foreach (var p in FindObjectsByType<PlayerControllerRPC>(FindObjectsSortMode.None))
        {
            if (p != null && !p.Object.HasInputAuthority)
                p.SetVisibleForOther(false);
        }
        isRevealing = false;
        UpdateFOVState();
        revealCoroutine = null;
    }

    // Kích hoạt kỹ năng húc đẩy đối tượng phía trước
    public void ActivateDashPushSkill()
    {
        if (!canUseDashPushSkill) return;
        if (!Object.HasInputAuthority) return;
        if (dashPushCooldownCoroutine != null)
            StopCoroutine(dashPushCooldownCoroutine);
        dashPushCooldownCoroutine = StartCoroutine(DashPushSkillCooldownCoroutine());
        if (animator != null)
            animator.SetTrigger("DashPush"); // Gọi animation dash
        StartCoroutine(DashPushCoroutine());
    }

    private IEnumerator DashPushSkillCooldownCoroutine()
    {
        canUseDashPushSkill = false;
        yield return new WaitForSeconds(dashPushCooldown);
        canUseDashPushSkill = true;
    }

    // Sự kiện cho Input System để kích hoạt kỹ năng
    public void OnActivateRevealSkill(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (!Object.HasInputAuthority) return;
        if (context.performed)
        {
            ActivateRevealAllSkill();
        }
    }

    // Sự kiện cho Input System để kích hoạt skill húc đẩy
    public void OnActivateDashPushSkill(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (!Object.HasInputAuthority) return;
        if (context.performed)
        {
            // Lấy hướng input từ New Input System
            Vector3 inputDir = new Vector3(moveInput.x, 0, moveInput.y);
            dashInputDirection = transform.TransformDirection(inputDir);
            ActivateDashPushSkill();
        }
    }

    // Sự kiện cho Input System để bật/tắt ngắm bắn (chuột phải)
    public void OnAim(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (!Object.HasInputAuthority) return;
        if (context.performed)
        {
            isAiming = true;
            UpdateFOVState();

            // Camera nhìn theo chuột khi aim
            if (playerCamera != null)
            {
                Vector2 mouseScreenPosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
                Ray ray = playerCamera.ScreenPointToRay(mouseScreenPosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Default", "Ground")))
                {
                    Vector3 lookPoint = hit.point;
                    Vector3 direction = (lookPoint - transform.position);
                    direction.y = 0f;
                    direction.Normalize();
                    if (direction != Vector3.zero)
                    {
                        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                    }
                }
                else
                {
                    // Nếu raycast không trúng gì thì tắt trạng thái ngắm bắn
                    isAiming = false;
                    UpdateFOVState();
                }
            }
        }
        else if (context.canceled)
        {
            isAiming = false;
            UpdateFOVState();
        }
    }
    private IEnumerator DashPushCoroutine()
    {
        // Chờ 2 giây trước khi đẩy
        yield return new WaitForSeconds(0.5f);
        // Chỉ đẩy các object phía trước (không dash), giảm một nửa khoảng cách đẩy
        Vector3 dashDir = dashInputDirection.sqrMagnitude > 0.1f ? dashInputDirection.normalized : transform.forward;
        float pushDistance = dashDistance * 0.5f;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        RaycastHit[] hits = Physics.SphereCastAll(origin, pushRadius, dashDir, pushDistance, pushLayerMask);
        foreach (var hit in hits)
        {
            var hitRb = hit.collider.attachedRigidbody;
            if (hitRb != null)
            {
                Vector3 pushDir = (hitRb.position - origin).normalized + Vector3.up * pushUpward;
                hitRb.AddForce(pushDir.normalized * dashForce, ForceMode.Impulse);
            }
        }
        yield break;
    }

    void Update()
    {
        // Không set trực tiếp chỉ số flashlight ở đây nữa
    }
}
