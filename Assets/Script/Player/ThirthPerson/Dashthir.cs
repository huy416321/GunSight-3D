using System.Collections;
using UnityEngine;

public class Dashthir : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashDistance = 5f;
    public float dashCooldown = 2f;
    public float dashSpeed = 20f;
    public AudioClip dashSound;
    public bool canDash = true;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    public void Dash()
    {
        if (!canDash) return;
        canDash = false;
        Vector3 dashDir = transform.forward;
        StartCoroutine(DashCoroutine(dashDir));
        if (dashSound != null)
            AudioSource.PlayClipAtPoint(dashSound, transform.position, FootstepAudioVolume);
            Invoke(nameof(ResetDash), dashCooldown);
    }

    private IEnumerator DashCoroutine(Vector3 direction)
    {
        float dashTime = dashDistance / dashSpeed;
        float elapsed = 0f;
        while (elapsed < dashTime)
        {
            transform.position += direction * dashSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void ResetDash()
    {
        canDash = true;
    }
}
