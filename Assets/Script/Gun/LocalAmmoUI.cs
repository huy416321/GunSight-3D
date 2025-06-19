using UnityEngine;
using TMPro;

public class LocalAmmoUI : MonoBehaviour
{
    public static LocalAmmoUI Instance;
    public TextMeshProUGUI ammoText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetAmmo(int current, int max)
    {
        if (ammoText != null)
            ammoText.text = $"{current} / {max}";
    }

    public void Show(bool show)
    {
        if (ammoText != null)
            ammoText.gameObject.SetActive(show);
    }
}
