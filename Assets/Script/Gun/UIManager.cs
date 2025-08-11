using TMPro;
using UnityEngine;
using Fusion;

public class UIManager : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoText;

    public void UpdateAmmo(int current, int max)
    {
        ammoText.text = $"{current} / {max}";
    }
}
