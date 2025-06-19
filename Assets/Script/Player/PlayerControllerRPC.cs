using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerRPC : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    private Vector2 moveInput;
    private CharacterController characterController;
    private Animator animator;

    [Header("Camera")]
    public Camera playerCamera;

    [Header("Weapon System")]
    public WeaponData[] weapons; // Danh sách vũ khí
    private int currentWeaponIndex = 0;
    private int currentAmmo;
    private float lastFireTime = -999f;
    public GameObject[] weaponObjects; // Các vũ khí tương ứng với weapons
    private bool isFiring = false;

    [Header("Dash")]
    public float dashDistance = 5f;
    public float dashCooldown = 2f;
    public float dashDuration = 0.2f;

    private bool isDashing = false;
    private float lastDashTime = -999f;


    [Header("Bullet Settings")]
    public GameObject[] bulletPrefabs; // Mỗi vũ khí 1 loại đạn
    public Transform firePoint;
    public float fireCooldown = 0.3f;


    [Header("UI")]
    public TextMeshProUGUI nameText;

    [Header("Grenade")]
    public GameObject grenadePrefab;
    public Transform grenadeSpawnPoint;
    public float grenadeThrowForce = 12f;
    public float grenadeCooldown = 3f;
    private float lastGrenadeTime = -999f;

    [Networked] public Vector3 NetworkedPosition { get; set; }
    [Networked] public Quaternion NetworkedRotation { get; set; }
    [Networked] public float NetworkedSpeed { get; set; }
    [Networked] public bool NetworkedIsDashing { get; set; }

    private LocalAmmoUI localAmmoUI;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            playerCamera.gameObject.SetActive(true);
            string playerName = PlayerPrefs.GetString("playerName", "Người chơi");
            RPC_SetName(playerName);
            // Tìm LocalAmmoUI trên scene, chỉ local player mới làm
            localAmmoUI = LocalAmmoUI.Instance;
            if (localAmmoUI != null)
            {
                localAmmoUI.Show(true);
                localAmmoUI.SetAmmo(currentAmmo, weapons[currentWeaponIndex].maxAmmo);
            }
        }
        else
        {
            playerCamera.gameObject.SetActive(false);
        }

        // Chỉ hiển thị 1 khẩu lúc đầu
        SwitchWeapon(currentWeaponIndex);

        transform.position = NetworkedPosition;
        transform.rotation = NetworkedRotation;
    }

    private void Update()
    {
        if (!Object.HasInputAuthority)
        {
            // Nếu không có quyền điều khiển, cập nhật vị trí và animation từ network
            transform.position = NetworkedPosition;
            transform.rotation = NetworkedRotation;
            animator.SetFloat("Speed", NetworkedSpeed);
            // KHÔNG set lại animator.SetBool("Dash", ...) ở đây
            return;
        }
        // Di chuyển
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        characterController.SimpleMove(move * moveSpeed);
        float speed = moveInput.sqrMagnitude;
        animator.SetFloat("Speed", speed);
        NetworkedSpeed = speed;
        // KHÔNG set lại animator.SetBool("Dash", ...) ở đây
        NetworkedIsDashing = isDashing;

        // Bắn liên thanh
        var weapon = weapons[currentWeaponIndex];
        float fireCooldown = weapon.fireRate;

        if (isFiring && Time.time - lastFireTime >= fireCooldown && currentAmmo > 0)
        {
            lastFireTime = Time.time;
            currentAmmo--;

            Vector3 forward = firePoint.forward;
            forward = Quaternion.Euler(
                Random.Range(-weapon.bulletSpread, weapon.bulletSpread),
                Random.Range(-weapon.bulletSpread, weapon.bulletSpread),
                0) * forward;

            RPC_Fire(firePoint.position, forward, currentWeaponIndex);
            RPC_PlayAnim("Shoot");
            ApplyRecoil(weapon.recoilAmount);
        }    

        // Gửi vị trí lên network
        RPC_UpdateTransform(transform.position, transform.rotation);

        if (localAmmoUI != null)
        {
            localAmmoUI.SetAmmo(currentAmmo, weapons[currentWeaponIndex].maxAmmo);
        }
    }

    void LateUpdate()
    {
        if (playerCamera != null && Object.HasInputAuthority)
        {
            playerCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.canceled ? Vector2.zero : context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (!Object.HasInputAuthority) return;

        Vector2 mouseScreenPosition = context.ReadValue<Vector2>();
        Ray ray = playerCamera.ScreenPointToRay(mouseScreenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Default")))
        {
            Vector3 lookPoint = hit.point;
            Vector3 direction = (lookPoint - transform.position);
            direction.y = 0f;
            direction.Normalize();

            if (direction != Vector3.zero)
            {
                float offsetY = 50f; // chỉnh lại nếu model không hướng Z+
                transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, offsetY, 0);
            }
        }
    }

// Bắn
    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        isFiring = true;
        else if (context.canceled)
        isFiring = false;
    }
    private void ApplyRecoil(float amount)
    {
        transform.position -= transform.forward * 0.05f * amount;
        // hoặc thêm animation nếu thích
    }
//

// Nạp đạn
    public void OnReload(InputAction.CallbackContext context)
    {
        if (!Object.HasInputAuthority) return;
        if (context.performed && !isReloading)
        {
            var weapon = weapons[currentWeaponIndex];
            if (currentAmmo < weapon.maxAmmo)
            {
                StartCoroutine(ReloadCoroutine(weapon));
            }
        }
    }

    private bool isReloading = false;

    private IEnumerator ReloadCoroutine(WeaponData weapon)
    {
        isReloading = true;
        RPC_PlayAnim("Reload");
        moveSpeed = 0; // Dừng di chuyển trong khi nạp đạn

        yield return new WaitForSeconds(1.5f); // thời gian nạp
        moveSpeed = 5f; // Khôi phục tốc độ di chuyển
        currentAmmo = weapon.maxAmmo;
        isReloading = false;
        if (localAmmoUI != null)
        {
            localAmmoUI.SetAmmo(currentAmmo, weapon.maxAmmo);
        }
    }
    //

// Chuyển đổi vũ khí
    public void OnSwitchWeapon1(InputAction.CallbackContext context)
    {
        if (context.performed) SwitchWeapon(0);
    }

    public void OnSwitchWeapon2(InputAction.CallbackContext context)
    {
        if (context.performed) SwitchWeapon(1);
    }

    public void OnSwitchWeapon3(InputAction.CallbackContext context)
    {
        if (context.performed) SwitchWeapon(2);
    }

    private void SwitchWeapon(int index)
    {
        currentWeaponIndex = index;
        currentAmmo = weapons[index].maxAmmo;

        for (int i = 0; i < weaponObjects.Length; i++)
        {
            weaponObjects[i].SetActive(i == index);
        }
        if (Object.HasInputAuthority && localAmmoUI != null)
        {
            localAmmoUI.SetAmmo(currentAmmo, weapons[currentWeaponIndex].maxAmmo);
        }
    }
    //

    // Dash
    public void OnDash(InputAction.CallbackContext context)
    {
        if (!Object.HasInputAuthority) return;
        if (context.performed && !isDashing && Time.time - lastDashTime >= dashCooldown)
        {
            StartCoroutine(DashCoroutine());
        }
    }

    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        NetworkedIsDashing = true;
        lastDashTime = Time.time;
        animator.SetTrigger("Dash");
        // Không set lại animator.SetBool("Dash", ...) trong lúc dash

        Vector3 dashDir = transform.forward;
        float dashSpeed = dashDistance / dashDuration;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            characterController.Move(dashDir * dashSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        NetworkedIsDashing = false;
        // Không set lại animator.SetBool("Dash", ...) ở đây, chỉ để animator tự chuyển state theo exit time
    }
    //

    // RPCs
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_Fire(Vector3 spawnPos, Vector3 forward, int weaponIndex)
    {
        if (bulletPrefabs.Length > weaponIndex && bulletPrefabs[weaponIndex] != null)
        {
            Quaternion rot = Quaternion.LookRotation(forward);
            Vector3 offset = forward * 0.5f;
            Runner.Spawn(bulletPrefabs[weaponIndex], spawnPos + offset, rot, Object.InputAuthority);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_PlayAnim(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_SetName(string name)
    {
        nameText.text = name;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Die()
    {
        animator.SetTrigger("Die");
    }

    public void OnThrowGrenade(InputAction.CallbackContext context)
    {
        if (!Object.HasInputAuthority) return;
        if (context.performed && Time.time - lastGrenadeTime >= grenadeCooldown)
        {
            lastGrenadeTime = Time.time;
            Vector3 throwDir = transform.forward + Vector3.up * 0.3f;
            RPC_ThrowGrenade(grenadeSpawnPoint.position, throwDir * grenadeThrowForce);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_ThrowGrenade(Vector3 pos, Vector3 velocity)
    {
        if (grenadePrefab != null)
        {
            var grenadeObj = Runner.Spawn(grenadePrefab, pos, Quaternion.identity, Object.InputAuthority);
            Rigidbody rb = grenadeObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = velocity;
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_UpdateTransform(Vector3 pos, Quaternion rot)
    {
        NetworkedPosition = pos;
        NetworkedRotation = rot;
    }
}
