using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/WeaponData")]
public class WeaponData : ScriptableObject
{
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip emptyAmmoSound;
    public string weaponName;
    public GameObject bulletPrefab;
    public float fireRate = 0.2f;
    public int maxAmmo = 30;
    public float recoilAmount = 1.5f;
    public float bulletSpread = 0.05f; // độ lệch ngẫu nhiên
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

}
