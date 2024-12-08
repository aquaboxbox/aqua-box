using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleControl : MonoBehaviour
{
    
    private Vector3 centerPosition;
    public float moveSpeed = 3f;
    private bool isColliding = false;
    private int currentCollider = 0;
    [SerializeField] private Material deadParticle;
    [SerializeField] private Material aliveParticle;
    private Rigidbody rb;

    void Start()
    {   
        rb = GetComponent<Rigidbody>();
        DisableParticle();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.X)) ActivateParticle(); // For debugging

        if(Input.GetKeyDown(KeyCode.B)) KillParticle(); // For debugging
    }

    void ActivateParticle()
    {   
        FindCenterOfCube();
        transform.position = centerPosition;

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }

        transform.Find("Point Light").gameObject.SetActive(true);
        // Find the child object named "Sphere"
        Transform sphereChild = transform.Find("Sphere");
        if (sphereChild != null)
        {
            // Get the Renderer component and assign the new material
            Renderer sphereRenderer = sphereChild.GetComponent<Renderer>();
            if (sphereRenderer != null && aliveParticle != null)
            {
                sphereRenderer.material = aliveParticle;
            }
            else
            {
                Debug.LogWarning("Renderer component or 'aliveParticle' material is missing.");
            }
        }
        else
        {
            Debug.LogWarning("Child object 'Sphere' not found.");
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

    private void KillParticle()
    {
        // Find the child object named "Fire"
        Transform fireChild = transform.Find("Fire");
        if (fireChild != null)
        {
            // Deactivate the "Fire" object
            fireChild.gameObject.SetActive(false);
            transform.Find("Point Light").gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Child object 'Fire' not found.");
        }

        // Find the child object named "Sphere"
        Transform sphereChild = transform.Find("Sphere");
        if (sphereChild != null)
        {
            // Get the Renderer component and assign the new material
            Renderer sphereRenderer = sphereChild.GetComponent<Renderer>();
            if (sphereRenderer != null && deadParticle != null)
            {
                sphereRenderer.material = deadParticle;
            }
            else
            {
                Debug.LogWarning("Renderer component or 'deadParticle' material is missing.");
            }
        }
        else
        {
            Debug.LogWarning("Child object 'Sphere' not found.");
        }
    }
}
