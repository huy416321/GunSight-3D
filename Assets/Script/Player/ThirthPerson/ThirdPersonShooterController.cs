using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using UnityEngine.Animations.Rigging;
using Fusion;
using System;

public class ThirdPersonShooterController : MonoBehaviour
{

    [SerializeField] private Classplayer classPlayer;
    [SerializeField] private Rig aimRig;
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
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

    public GameObject NightVisionEffect;
    public Light light;

    private void Awake()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        animator = GetComponent<Animator>();
    }

    private void Update()
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

        RpcAim();
        RpcSkill();
        RpcKneel();
        RpcShoot();
        Rpclight();


        aimRig.weight = Mathf.Lerp(aimRig.weight, aimRigweight, Time.deltaTime * 20f);

    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RpcShoot()
    {
        if (starterAssetsInputs.shoot)
        {
            /*
            // Hit Scan Shoot
            if (hitTransform != null) {
                // Hit something
                if (hitTransform.GetComponent<BulletTarget>() != null) {
                    // Hit target
                    Instantiate(vfxHitGreen, mouseWorldPosition, Quaternion.identity);
                } else {
                    // Hit something else
                    Instantiate(vfxHitRed, mouseWorldPosition, Quaternion.identity);
                }
            }
            //*/
            //*
            // Projectile Shoot
            Vector3 aimDir = (mouseWorldPosition - spawnBulletPosition.position).normalized;
            Instantiate(pfBulletProjectile, spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
            //*/
            starterAssetsInputs.shoot = false;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RpcAim()
    {
        if (starterAssetsInputs.aim)
        {
            aimVirtualCamera.gameObject.SetActive(true);
            thirdPersonController.SetSensitivity(aimSensitivity);
            thirdPersonController.SetRotateOnMove(false);
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 1f, Time.deltaTime * 13f));
            aimRigweight = 1f;

            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
        }
        else
        {
            aimVirtualCamera.gameObject.SetActive(false);
            thirdPersonController.SetSensitivity(normalSensitivity);
            thirdPersonController.SetRotateOnMove(true);
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 13f));
            aimRigweight = 0f;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RpcKneel()
    {
        if (starterAssetsInputs.kneel)
        {
            animator.SetBool("Kneel", true);
        }
        else
        {
            animator.SetBool("Kneel", false);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RpcSkill()
    {
        if (starterAssetsInputs.skill && classPlayer == Classplayer.Shielder)
        {
            animator.SetTrigger("Skill");
            starterAssetsInputs.skill = false;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void Rpclight()
    {
        if (starterAssetsInputs.light)
        {
            light.enabled = !light.enabled;
            starterAssetsInputs.light = false;
        }
    }


    public enum Classplayer
    {
        Rife,
        Sniper,
        Shielder
    }
}