using System.Collections;
using UnityEngine;
using Fusion;

public class PlayerSkillController : NetworkBehaviour
{
    [Header("Skill Settings")]
    public float revealDuration = 5f;
    private Coroutine revealCoroutine;
    private bool canUseRevealSkill = true;
    public float revealCooldown = 10f;
    private bool isRevealing = false;

    [Header("Flashlight Reference")]
    public FlashlightController flashlightController;
    private float originalMaxDistance;
    private float originalSpotAngle;
    private float originalSphereRadius; // Lưu lại bán kính vùng phát hiện
    public float flashlightRangeMultiplier = 10f;
    public float flashlightAngleMultiplier = 2f;
    public float flashlightRadiusMultiplier = 2f; // Multiplier cho sphereRadius

    // Giá trị đèn pin khi ngắm bắn
    public float aimFlashlightSpotAngle = 20f;
    public float aimFlashlightMaxDistance = 50f;
    public float aimFlashlightSphereRadius = 8f;
    public float aimFlashlightRange = 20f; // Không dùng, chỉ tăng maxDistance
    private bool flashlightAimed = false;
    private float defaultSpotAngle;
    private float defaultMaxDistance;
    private float defaultSphereRadius;
    private float defaultRange;
    private float defaultIntensity;

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

    private void UpdateFlashlightState()
    {
        if (flashlightController == null || flashlightController.flashlight == null)
            return;
        if (!flashlightAimed)
        {
            defaultSpotAngle = flashlightController.flashlight.spotAngle;
            defaultMaxDistance = flashlightController.maxDistance;
            defaultSphereRadius = flashlightController.sphereRadius;
            defaultRange = flashlightController.flashlight.range;
            defaultIntensity = flashlightController.flashlight.intensity;
            flashlightAimed = true;
        }
        if (isAiming)
        {
            flashlightController.flashlight.spotAngle = aimFlashlightSpotAngle;
            flashlightController.maxDistance = aimFlashlightMaxDistance;
            flashlightController.sphereRadius = aimFlashlightSphereRadius;
            flashlightController.flashlight.range = 20f;
            flashlightController.flashlight.intensity = 100f;
        }
        else if (isRevealing)
        {
            flashlightController.maxDistance = defaultMaxDistance * flashlightRangeMultiplier;
            flashlightController.flashlight.spotAngle = defaultSpotAngle * flashlightAngleMultiplier;
            flashlightController.sphereRadius = defaultSphereRadius * flashlightRadiusMultiplier;
            // Không thay đổi range và intensity khi chỉ reveal
        }
        else
        {
            flashlightController.flashlight.spotAngle = defaultSpotAngle;
            flashlightController.maxDistance = defaultMaxDistance;
            flashlightController.sphereRadius = defaultSphereRadius;
            flashlightController.flashlight.range = defaultRange;
            flashlightController.flashlight.intensity = defaultIntensity;
        }
    }

    private IEnumerator RevealAllPlayersCoroutine()
    {
        isRevealing = true;
        UpdateFlashlightState();
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
        UpdateFlashlightState();
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
            UpdateFlashlightState();
        }
        else if (context.canceled)
        {
            isAiming = false;
            UpdateFlashlightState();
        }
    }
    private IEnumerator DashPushCoroutine()
    {
        var characterController = GetComponent<CharacterController>();
        // Dash theo hướng input, nếu không có input thì dash theo forward
        Vector3 dashDir = dashInputDirection.sqrMagnitude > 0.1f ? dashInputDirection.normalized : transform.forward;
        float dashSpeed = dashDistance / 0.18f;
        float elapsed = 0f;
        float dashDuration = 0.18f;
        while (elapsed < dashDuration)
        {
            if (characterController != null)
                characterController.Move(dashDir * dashSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        // Đẩy các đối tượng phía trước trong bán kính pushRadius
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        RaycastHit[] hits = Physics.SphereCastAll(origin, pushRadius, dashDir, dashDistance, pushLayerMask);
        foreach (var hit in hits)
        {
            var hitRb = hit.collider.attachedRigidbody;
            if (hitRb != null)
            {
                Vector3 pushDir = (hitRb.position - origin).normalized + Vector3.up * pushUpward;
                hitRb.AddForce(pushDir.normalized * dashForce, ForceMode.Impulse);
            }
        }
    }

    void Update()
    {
        // Không set trực tiếp chỉ số flashlight ở đây nữa
    }
}
