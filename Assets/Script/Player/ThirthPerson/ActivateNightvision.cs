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

    public void Activate()
    {
        if (starterAssetsInputs != null && starterAssetsInputs.skill)
        {
            isNightVisionOn = !isNightVisionOn;
            if (NightVisionEffect != null)
                NightVisionEffect.SetActive(isNightVisionOn);
        }
    }


}
