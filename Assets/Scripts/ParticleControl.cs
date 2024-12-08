using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleControl : MonoBehaviour
{
    
    private Vector3 centerPosition;
    public float moveSpeed = 3f;
    private bool isColliding = false;
    private int currentCollider = 0;
    
    private Rigidbody rb;

    void Start()
    {   
        rb = GetComponent<Rigidbody>();
        DisableParticle();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.X)) ActivateParticle(); // For debugging


    }

    void ActivateParticle()
    {   
        FindCenterOfCube();
        transform.position = centerPosition;

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    void DisableParticle()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void FindCenterOfCube()
    {
        GameObject centerOfCube = GameObject.Find("CenterOfCube");

        if (centerOfCube != null)
        {
            centerPosition = centerOfCube.transform.position;
            //Debug.Log($"Center of Cube found at: {centerPosition}");
        }
        else
        {
            //Debug.LogError("GameObject named 'CenterOfCube' not found in the scene.");
        }
    }

    private void OnCollisionEnter(Collision other) 
    {   
        FindCenterOfCube();
        transform.position = centerPosition;

        Debug.Log("Collision!");
    }

}
