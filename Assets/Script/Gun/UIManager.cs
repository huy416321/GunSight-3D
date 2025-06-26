using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoText;

    public void UpdateAmmo(int current, int max)
    {
        ammoText.text = $"{current} / {max}";
    }
}
