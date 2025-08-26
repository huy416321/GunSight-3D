using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using UnityEngine.Animations.Rigging;
using Fusion;
using System;

public class ThirdPersonShooterController : NetworkBehaviour

{
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private PlayerHealth isdead;
    [Networked] private Vector3 NetAimTarget { get; set; }
    [SerializeField] private Transform aimTarget; // Kéo Sphere (target của constraint) vào đây
    // Networked animation states
    [Networked] private bool IsAiming { get; set; }
    [Networked] private bool IsKneeling { get; set; }
    [Networked] private bool IsUsingSkill { get; set; }
    [Networked] private bool IsReloading { get; set; }
    [Networked] private bool IsthrowGrenade { get; set; }
    [Networked] private float NetAimRigWeight { get; set; }
    [Networked] private bool IsShooting { get; set; } // Thêm biến này để đồng bộ trạng thái bắn


    [SerializeField] private Classplayer classPlayer;
    [SerializeField] private Rig aimRig;
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    [SerializeField] private CinemachineVirtualCamera followVirtualCamera;
    [SerializeField] private float normalSensitivity;
    [SerializeField] private float aimSensitivity;
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
    [SerializeField] private Transform debugTransform;
    [SerializeField] private Transform pfBulletProjectile;
    [SerializeField] private Transform spawnBulletPosition;
    [SerializeField] private Transform vfxHitGreen;
    [SerializeField] private Transform vfxHitRed;

    private Vector3 mouseWorldPosition;
    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;
    private Animator animator;
    private float aimRigweight;
    private float shootCooldown = 0f;
    private int currentAmmo = 0;

    public new Light light;
    [Networked] private bool IsLightOn { get; set; }

    private void Awake()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        animator = GetComponent<Animator>();
        // Khởi tạo số lượng đạn khi bắt đầu
        if (weaponData != null)
            currentAmmo = weaponData.maxAmmo;
    }

    private void Update()
    {
        if (isdead.isDead) return;
         // Đếm thời gian cooldown bắn
        if (shootCooldown > 0f)
            shootCooldown -= Time.deltaTime;   
        if (HasInputAuthority)
        {
            mouseWorldPosition = Vector3.zero;
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            Transform hitTransform = null;
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
            {
                debugTransform.position = raycastHit.point;
                mouseWorldPosition = raycastHit.point;
                hitTransform = raycastHit.transform;
            }

            // Đồng bộ vị trí target aim qua mạng (làm mượt NetAimTarget)
            NetAimTarget = Vector3.Lerp(NetAimTarget, mouseWorldPosition, Time.deltaTime * 20f);

            // Cập nhật trạng thái networked
            IsAiming = starterAssetsInputs.aim;
            IsShooting = starterAssetsInputs.shoot;
            IsKneeling = starterAssetsInputs.kneel;
            IsUsingSkill = starterAssetsInputs.skill;
            IsReloading = starterAssetsInputs.reload;
            IsthrowGrenade = starterAssetsInputs.throwGrenade;

            if (Input.GetKeyDown(KeyCode.C)) // "LockMouse" là tên input bạn tự đặt trong InputManager hoặc InputSystem
            {
                bool isLocked = Cursor.lockState != CursorLockMode.Locked;
                Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !isLocked;
                Debug.Log(isLocked ? "Lock mouse" : "Unlock mouse");
            }

            // Cập nhật NetAimRigWeight cho mọi client
            NetAimRigWeight = aimRigweight;
            
            if (light != null)
                light.enabled = IsLightOn;
            Rpclight();
            // Luôn bật camera follow cho player local
            if (followVirtualCamera != null && !followVirtualCamera.gameObject.activeSelf)
                followVirtualCamera.gameObject.SetActive(true);

            // Bật/tắt camera ngắm cho local player
            if (starterAssetsInputs.aim)
            {
                if (aimVirtualCamera != null && !aimVirtualCamera.gameObject.activeSelf)
                    aimVirtualCamera.gameObject.SetActive(true);
                aimRig.weight = 1f;
            }
            else
            {
                if (aimVirtualCamera != null && aimVirtualCamera.gameObject.activeSelf)
                    aimVirtualCamera.gameObject.SetActive(false);
                aimRig.weight = 0f;
            }

            // Set Animator cho local player
            animator.SetBool("Kneel", starterAssetsInputs.kneel);

            if (starterAssetsInputs.shoot && currentAmmo > 0)
            {
                animator.SetBool("Shoot", starterAssetsInputs.shoot);
                aimRigweight = 1f; // Giữ aimRig khi bắn
            }
            else
            {
                animator.SetBool("Shoot", false);
                aimRigweight = 0f;
            }

            if (starterAssetsInputs.skill && starterAssetsInputs.aim)
            {
                starterAssetsInputs.aim = false;
                animator.SetBool("Skill", starterAssetsInputs.skill);
            }
            else
            {
                animator.SetBool("Skill", starterAssetsInputs.skill);
            }
            
            if (starterAssetsInputs.throwGrenade && starterAssetsInputs.aim)
            {
                starterAssetsInputs.aim = false; // Tắt aim khi ném bom
                animator.SetBool("ThorwGrenade", starterAssetsInputs.throwGrenade);
            }
            else
            {
                animator.SetBool("ThorwGrenade", starterAssetsInputs.throwGrenade);
            }

            if (currentAmmo < weaponData.maxAmmo)
            {
                animator.SetBool("Reload", starterAssetsInputs.reload); // Không tự động reload khi còn đạn
            }
            else
            {
                animator.SetBool("Reload", false);
            }

            animator.SetLayerWeight(1, starterAssetsInputs.aim ? 1f : 0f);
        }
        else
        {
            // Tắt camera follow cho player remote
            if (followVirtualCamera != null && followVirtualCamera.gameObject.activeSelf)
                followVirtualCamera.gameObject.SetActive(false);
            // Luôn tắt camera aim cho player remote
            if (aimVirtualCamera != null && aimVirtualCamera.gameObject.activeSelf)
                aimVirtualCamera.gameObject.SetActive(false);

            // Set Animator cho remote player
            animator.SetBool("Kneel", IsKneeling);
            animator.SetBool("Skill", IsUsingSkill);
            animator.SetBool("Reload", IsReloading);
            animator.SetBool("ThorwGrenade", IsthrowGrenade);
            animator.SetLayerWeight(1, IsAiming ? 1f : 0f);
            // Nếu muốn remote client cũng quay hướng aim, có thể thêm code nội suy transform.forward ở đây
        }

        // Luôn sync aimRig cho mọi người chơi
        aimRig.weight = Mathf.Lerp(aimRig.weight, HasInputAuthority ? aimRigweight : NetAimRigWeight, Time.deltaTime * 20f);

        // Luôn cập nhật vị trí target cho constraint (mọi client) - làm mượt
        if (aimTarget != null)
            aimTarget.position = Vector3.Lerp(aimTarget.position, NetAimTarget, Time.deltaTime * 20f);
        // Xoay nhân vật theo hướng aim khi đang aim (chỉ trục Y)
        if ((HasInputAuthority && starterAssetsInputs.aim) || (!HasInputAuthority && IsAiming))
        {
            Vector3 lookDir = NetAimTarget - transform.position;
            lookDir.y = 0f; // Chỉ xoay trục Y
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 20f);
            }
        }

        // Xử lý bắn đạn: chỉ gửi RPC khi nhấn shoot
        if (starterAssetsInputs.shoot && weaponData != null && shootCooldown <= 0f && currentAmmo > 0)
        {
            // Tính hướng đạn với độ lệch từ weaponData.bulletSpread
            Vector3 aimDir = (mouseWorldPosition - spawnBulletPosition.position).normalized;
            float spread = weaponData.bulletSpread;
            aimDir = Quaternion.Euler(
                UnityEngine.Random.Range(-spread, spread),
                UnityEngine.Random.Range(-spread, spread),
                0f) * aimDir;
            // Xoay nhân vật theo hướng bắn (chỉ trục Y)
            Vector3 lookDir = aimDir;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 100f);
            }

            RPC_Fire(spawnBulletPosition.position, aimDir);
            shootCooldown = weaponData.fireRate;
            currentAmmo--;
            // Cập nhật UI ammo
            if (LocalAmmoUI.Instance != null)
                LocalAmmoUI.Instance.SetAmmo(currentAmmo, weaponData.maxAmmo);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_Fire(Vector3 spawnPos, Vector3 forward)
    {
        if (weaponData != null && weaponData.bulletPrefab != null)
        {
            Debug.Log($"RPC_Fire called by {Object.InputAuthority} at position {spawnPos} with forward {forward}");
            Quaternion rot = Quaternion.LookRotation(forward, Vector3.up);
            Vector3 offset = forward * 0.5f;
            AudioSource.PlayClipAtPoint(weaponData.fireSound, transform.position, weaponData.FootstepAudioVolume);
            var bulletObj = Runner.Spawn(
                weaponData.bulletPrefab,
                spawnPos + offset,
                rot,
                Object.InputAuthority
            );
            // Gán team cho viên đạn nếu có script Bulletthird
            var bullet = bulletObj != null ? bulletObj.GetComponent<Bulletthird>() : null;
            if (bullet != null)
            {
                bullet.isPoliceShooter = false; // hoặc truyền biến team nếu có
            }
        }
    }

    private void Reload()
    {
        if (weaponData != null && currentAmmo < weaponData.maxAmmo)
        {
            currentAmmo = weaponData.maxAmmo;
            Debug.Log($"Player {Object.Id} reloaded. Current ammo: {currentAmmo}");
            // Cập nhật UI ammo
            if (LocalAmmoUI.Instance != null)
                LocalAmmoUI.Instance.SetAmmo(currentAmmo, weaponData.maxAmmo);
        }
    }
    private void Reloadsound()
    {
        if (weaponData != null && weaponData.reloadSound != null)
        {
            AudioSource.PlayClipAtPoint(weaponData.reloadSound, transform.position, weaponData.FootstepAudioVolume);
        }
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void Rpclight()
    {
        if (starterAssetsInputs.light)
        {
            IsLightOn = !IsLightOn;
            if (light != null)
                light.enabled = IsLightOn;
            starterAssetsInputs.light = false;
        }
        else
        {
            // Remote client: cập nhật trạng thái light
            if (light != null)
                light.enabled = IsLightOn;
        }
    }


    public enum Classplayer
    {
        Rife,
        Sniper,
        Shielder
    }
}