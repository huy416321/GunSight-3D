using UnityEngine;
using TMPro;
using Fusion;

public class PlayerAmmoUI : NetworkBehaviour
{
    public TextMeshProUGUI ammoText;
    private PlayerControllerRPC playerController;

    private void Awake()
    {
        playerController = GetComponent<PlayerControllerRPC>();
        if (!Object.HasInputAuthority && ammoText != null)
        {
            ammoText.gameObject.SetActive(false); // Ẩn UI nếu không phải local player
        }
    }

    public void UpdateAmmoUI(int currentAmmo, int maxAmmo)
    {
        if (Object.HasInputAuthority && ammoText != null)
        {
            ammoText.text = $"{currentAmmo} / {maxAmmo}";
        }
    }
}