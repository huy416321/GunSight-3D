using UnityEngine;
using UnityEngine.UI;

public class LocalHealthUI : MonoBehaviour
{
    public static LocalHealthUI Instance;
    public Slider healthSlider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetHealth(float current, float max)
    {
        if (healthSlider != null)
            healthSlider.value = current / max;
    }

    public void Show(bool show)
    {
        if (healthSlider != null)
            healthSlider.gameObject.SetActive(show);
    }
}