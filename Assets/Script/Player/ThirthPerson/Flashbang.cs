using UnityEngine;
using Fusion;
using UnityEngine.Rendering;

public class Flashbang : NetworkBehaviour
{
    private bool exploded = false;

    private void Start()
    {
        Invoke(nameof(Explode), 2f);
    }

    private void Explode()
    {
        if (exploded) return;
        exploded = true;
        ApplyFlashToPlayersNearby();
        Invoke(nameof(DestroySelf), 1f);
    }

    private void DestroySelf()
    {
        if (Object.HasStateAuthority)
            Runner.Despawn(Object);
        else
            Destroy(gameObject);
    }
    // Gọi hàm này khi bom nổ để tác động lên tất cả người chơi khác
    public void ApplyFlashToPlayersNearby(float radius = 15f, float maxAngle = 45f)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var hit in hits)
        {
            var player = hit.GetComponent<NetworkBehaviour>();
            if (player != null && player != this)
            {
                var cam = hit.GetComponentInChildren<Camera>();
                if (cam != null)
                {
                    Vector3 toFlash = (transform.position - cam.transform.position).normalized;
                    float angle = Vector3.Angle(cam.transform.forward, toFlash);
                    if (angle <= maxAngle)
                    {
                        var flashScript = hit.GetComponent<Flashbang>();
                        if (flashScript != null)
                        {
                            flashScript.ActivateFlashEffect(cam.transform, maxAngle);
                        }
                    }
                }
            }
        }
    }
    [Header("Flash Effect Settings")]
    public float flashEffectDuration = 2f;
    public Volume flashEffectVolume; // Volume effect (post-processing)
    public CanvasGroup flashCanvasGroup; // UI trắng
    public AudioClip flashEffectSound;
    [Range(0, 1)] public float flashEffectVolumeSound = 0.7f;


    // Gọi hàm này khi flashbang nổ (ví dụ từ script bom hoặc trigger)
    // Chỉ kích hoạt hiệu ứng nếu camera nhìn vào bom trong một góc nhất định
    public void ActivateFlashEffect(Transform playerCamera, float maxAngle = 45f)
    {
        if (playerCamera == null) return;
        Vector3 toFlash = (transform.position - playerCamera.position).normalized;
        float angle = Vector3.Angle(playerCamera.forward, toFlash);
        if (angle <= maxAngle)
        {
            if (flashEffectVolume != null)
                flashEffectVolume.weight = 1f;
            if (flashCanvasGroup != null)
                flashCanvasGroup.alpha = 1f;
            if (flashEffectSound != null)
                AudioSource.PlayClipAtPoint(flashEffectSound, transform.position, flashEffectVolumeSound);
            Invoke(nameof(DeactivateFlashEffect), flashEffectDuration);
        }
    }

    private void DeactivateFlashEffect()
    {
        if (flashEffectVolume != null)
            flashEffectVolume.weight = 0f;
        if (flashCanvasGroup != null)
            flashCanvasGroup.alpha = 0f;
    }
}
