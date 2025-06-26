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

    private IEnumerator RevealAllPlayersCoroutine()
    {
        isRevealing = true;
        // Lưu và tăng tầm chiếu đèn pin
        if (flashlightController != null && flashlightController.flashlight != null)
        {
            originalMaxDistance = flashlightController.maxDistance;
            originalSpotAngle = flashlightController.flashlight.spotAngle;
            originalSphereRadius = flashlightController.sphereRadius;
            flashlightController.maxDistance *= flashlightRangeMultiplier;
            flashlightController.flashlight.spotAngle *= flashlightAngleMultiplier;
            flashlightController.sphereRadius *= flashlightRadiusMultiplier;
        }
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
        // Khôi phục tầm chiếu đèn pin
        if (flashlightController != null && flashlightController.flashlight != null)
        {
            flashlightController.maxDistance = originalMaxDistance;
            flashlightController.flashlight.spotAngle = originalSpotAngle;
            flashlightController.sphereRadius = originalSphereRadius;
        }
        isRevealing = false;
        revealCoroutine = null;
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
}
