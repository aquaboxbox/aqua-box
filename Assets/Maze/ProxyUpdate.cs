using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProxyUpdate : MonoBehaviour
{
    // Target for material to render around
    [SerializeField] public Transform target;
    [SerializeField] public float radius = 1.0f;
    [SerializeField] public float dithering = 0.1f;

    MeshRenderer meshRenderer;

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        meshRenderer.material.SetVector("_ProxyTarget", target.position);
        meshRenderer.material.SetFloat("_ProxyRadius", radius);
        meshRenderer.material.SetFloat("_Dithering", dithering);
    }
}
