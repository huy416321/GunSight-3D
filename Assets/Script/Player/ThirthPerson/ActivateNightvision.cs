using UnityEngine;
using StarterAssets;

public class ActivateNightvision : MonoBehaviour
{
    [SerializeField] private GameObject NightVisionEffect;
    private StarterAssetsInputs starterAssetsInputs;
    private bool isNightVisionOn = false;

    void Awake()
    {
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
    }

    void Update()
    {
        if (starterAssetsInputs != null && starterAssetsInputs.nightVision)
        {
            isNightVisionOn = !isNightVisionOn;
            if (NightVisionEffect != null)
                NightVisionEffect.SetActive(isNightVisionOn);
            starterAssetsInputs.nightVision = false;
        }
    }
}
