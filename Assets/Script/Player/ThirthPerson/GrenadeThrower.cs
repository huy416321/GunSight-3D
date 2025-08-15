using UnityEngine;
using Fusion;
using StarterAssets;
using UnityEngine.Rendering;

public class GrenadeThrower : NetworkBehaviour
{
    [Header("Grenade Settings")]
    public GameObject grenadePrefab;
    public Transform throwPoint;
    public float throwForce = 15f;
    public AudioClip throwSound;

    [Header("flashover")]
    [Header("FlashBag Settings")]
    public Volume flashover;
    public CanvasGroup flashoverCanvasGroup;
    public AudioClip flashoverSound;

    [Range(0, 1)] public float throwVolume = 0.7f;
    private StarterAssetsInputs starterAssetsInputs;

    private void Awake()
    {
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
    }
    private void Update()
    {
        
    }

    public void ThrowGrenade()
    {
        if (grenadePrefab == null || throwPoint == null) return;
        // Spawn grenade qua Fusion
        var grenadeObj = Runner.Spawn(grenadePrefab, throwPoint.position, throwPoint.rotation, Object.InputAuthority);
        Rigidbody rb = grenadeObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(throwPoint.up * throwForce, ForceMode.Impulse);
        }
        if (throwSound != null)
        {
            AudioSource.PlayClipAtPoint(throwSound, throwPoint.position, throwVolume);
        }
    }

    private void Flashbanged()
    {
        flashover.weight = 1f;
        AudioSource.PlayClipAtPoint(flashoverSound, transform.position, throwVolume);
        flashoverCanvasGroup.alpha = 1f;
    }
}
