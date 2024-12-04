using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleControl : MonoBehaviour
{
    
    private Vector3 centerPosition;
    public float moveSpeed = 3f;
    private bool isColliding = false;
    
    void Start()
    {
        DisableParticle();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.X)) ActivateParticle(); // For debugging

        if(!isColliding)
        {
            GoToCenter();
        } 
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

    private void GoToCenter()
    {
        if (Vector3.Distance(transform.position, centerPosition) > 0.001f)
        {
            transform.position = Vector3.Lerp(transform.position, centerPosition, moveSpeed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        isColliding = true;
    }


    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;
    }
}
