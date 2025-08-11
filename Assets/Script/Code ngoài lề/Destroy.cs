using UnityEngine;
using Fusion;

public class Destroy : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(this.gameObject, 2f);
    }

}
