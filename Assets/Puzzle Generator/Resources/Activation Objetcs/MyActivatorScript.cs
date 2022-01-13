using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyActivatorScript : MonoBehaviour
{
    [SerializeField] GameObject Fire;
    [SerializeField] GameObject Material;
    Material GlowMaterial;


    private void Start()
    {
        GlowMaterial = Material.GetComponent<MeshRenderer>().material;
    }

    public void Activate()
    {
        Fire.SetActive(true);
        GlowMaterial.EnableKeyword("_EMISSION");
    }
}
