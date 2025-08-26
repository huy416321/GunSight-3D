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
    public ForceMode forceMode = ForceMode.Impulse;
    public float pushForce = 10f;
    public LayerMask pushLayerMask;
    private bool canUseDashPushSkill = true;
    private Coroutine dashPushCooldownCoroutine;
    public AudioClip pushSound;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Header("Aiming")]
    public bool isAiming = false;

    [Header("Animation")]
    public Animator animator; // Thêm biến Animator cho animation dash

    // Dash theo hướng input
    public WeaponData weaponData;
    private Vector3 dashInputDirection = Vector3.zero;
    private Vector2 moveInput = Vector2.zero; // Lưu input từ New Input System
    

    [Header("DestroyWall Dash Skill")]
    public float dashDuration = 0.2f;
    public float dashSpeed = 20f;
    public LayerMask destroyWallLayer;
    private bool isDashing = false;

    [Header("Invincible Skill")]
    public float invincibleDuration = 5f;
    public bool isInvincible = false;

    // Kích hoạt kỹ năng: nhìn thấy tất cả player trong 5 giây
public void ActivateRevealAllSkill()
{
    Debug.Log("Kích hoạt kỹ năng Revealing All Players");
    if (!canUseRevealSkill) return;
    if (!Object.HasInputAuthority) return;
    RPC_TriggerRevealSkill();
    StartCoroutine(RevealAllSkillWithStun());
}

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_TriggerRevealSkill()
    {
        if (animator != null)
            animator.SetTrigger("RevealSkill");
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
        RPC_TriggerDashPushSkill();
        if (dashPushCooldownCoroutine != null)
            StopCoroutine(dashPushCooldownCoroutine);
        dashPushCooldownCoroutine = StartCoroutine(DashPushSkillCooldownCoroutine());
        RPC_DashPushEffect(transform.position);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_DashPushEffect(Vector3 origin)
    {
        Collider[] colliders = Physics.OverlapSphere(origin, pushRadius, pushLayerMask);
        int destroyWallLayer = LayerMask.NameToLayer("DestroyWall");
        foreach (var col in colliders)
        {
            Rigidbody rb = col.attachedRigidbody;
            if (rb != null)
            {
                Vector3 dir = (col.transform.position - origin).normalized;
                rb.AddForce(dir * pushForce, forceMode);
            }
            // Tắt collider cho mọi object có layer DestroyWall
            if (col.gameObject.layer == destroyWallLayer)
            {
                var colliderComponent = col.GetComponent<Collider>();
                if (colliderComponent != null)
                    colliderComponent.enabled = false;
            }
        }
        if (pushSound != null)
        {
            AudioSource.PlayClipAtPoint(pushSound, origin, FootstepAudioVolume);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_TriggerDashPushSkill()
    {
        if (animator != null)
            animator.SetTrigger("DashPush");
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
        Collider[] colliders = Physics.OverlapSphere(transform.position, pushRadius, pushLayerMask);
        bool foundDestroyWall = false;
        int destroyWallLayer = LayerMask.NameToLayer("DestroyWall");
        foreach (var col in colliders)
        {
            Rigidbody rb = col.attachedRigidbody;
            if (rb != null)
            {
                Vector3 dir = (col.transform.position - transform.position).normalized;
                rb.AddForce(dir * pushForce, forceMode);
            }
            if (col.gameObject.layer == destroyWallLayer)
            {
                foundDestroyWall = true;
                // Tắt collider sau khi đẩy
                col.enabled = false;
            }
        }
        if (foundDestroyWall && pushSound != null)
        {
            Invoke(nameof(PlayPushSound), 0f);
        }
        yield break;
    }

    // Kích hoạt skill dash đẩy destroywall
    public void ActivateDashDestroyWallSkill(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (!Object.HasInputAuthority || isDashing) return;
        RPC_TriggerDashDestroyWallSkill();
        RPC_DashDestroyWallEffect(transform.position, transform.forward);
        StartCoroutine(DashDestroyWallCoroutine());
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_TriggerDashDestroyWallSkill()
    {
        if (animator != null)
            animator.SetTrigger("DashPush");
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_DashDestroyWallEffect(Vector3 origin, Vector3 dashDir)
    {
        Collider[] hits = Physics.OverlapSphere(origin + dashDir * 1f, pushRadius, destroyWallLayer);
        foreach (var hit in hits)
        {
            var wallRb = hit.attachedRigidbody;
            if (wallRb != null && !wallRb.isKinematic)
            {
                wallRb.AddForce(dashDir * pushForce, forceMode);
            }
            var col = hit.GetComponent<Collider>();
            if (col != null && col.gameObject.layer == LayerMask.NameToLayer("destroywall"))
                col.enabled = false;
        }
    }

    private IEnumerator DashPushSkillCooldownCoroutine()
    {
        canUseDashPushSkill = false;
        yield return new WaitForSeconds(dashPushCooldown);
        canUseDashPushSkill = true;
    }

    private IEnumerator DashDestroyWallCoroutine()
    {
        isDashing = true;
        if (animator != null) animator.SetTrigger("DashPush");
        float timer = 0f;
        var rb = GetComponent<Rigidbody>();
        Vector3 dashDir = transform.forward;
        while (timer < dashDuration)
        {
            if (rb != null)
                rb.linearVelocity = dashDir * dashSpeed;
            else
                transform.position += dashDir * dashSpeed * Time.deltaTime;
            Collider[] hits = Physics.OverlapSphere(transform.position + dashDir * 1f, pushRadius, destroyWallLayer);
            foreach (var hit in hits)
            {
                var wallRb = hit.attachedRigidbody;
                if (wallRb != null && !wallRb.isKinematic)
                {
                    wallRb.AddForce(dashDir * pushForce, forceMode);
                }
                var col = hit.GetComponent<Collider>();
                if (col != null && col.gameObject.layer == LayerMask.NameToLayer("destroywall"))
                    col.enabled = false;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        if (rb != null) rb.linearVelocity = Vector3.zero;
        isDashing = false;
    }

    // Kích hoạt skill miễn nhiễm sát thương
    public void ActivateInvincibleSkill()
    {
        if (!Object.HasInputAuthority || isInvincible) return;
        RPC_TriggerInvincibleSkill();
        StartCoroutine(InvincibleCoroutine());
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_TriggerInvincibleSkill()
    {
        if (animator != null)
            animator.SetTrigger("Invincible");
    }

    private IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;
        // Tạm thời tắt nhận sát thương
        var health = GetComponent<PlayerControllerRPC>();
        if (health != null)
        {
            health.enabled = false;
        }
        yield return new WaitForSeconds(invincibleDuration);
        if (health != null)
        {
            health.enabled = true;
        }
        isInvincible = false;
    }

    private void PlayPushSound()
    {
        AudioSource.PlayClipAtPoint(pushSound, transform.position, FootstepAudioVolume);
    }
}
