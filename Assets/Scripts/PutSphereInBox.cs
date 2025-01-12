using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PutSphereInBox : MonoBehaviour
{
    private Collider parentCollider;
    private Collider childCollider;

    void Start()
    {
        // Get the parent collider
        parentCollider = transform.parent.GetComponent<Collider>();

        // Get the child object's collider
        childCollider = GetComponent<Collider>();

        if (parentCollider == null || childCollider == null)
        {
            Debug.LogError("Parent or Child does not have a Collider component!");
        }
    }

    void Update()
    {
        CheckIfInsideParent();
    }

    void CheckIfInsideParent()
    {
        if (parentCollider != null && childCollider != null)
        {
            // Check if the child object is within the parent's bounds
            if (parentCollider.bounds.Contains(childCollider.bounds.center))
            {
                Debug.Log($"{gameObject.name} is inside the parent object.");
            }
            else
            {
                Debug.Log($"{gameObject.name} is outside the parent object.");
                transform.position = parentCollider.bounds.center;
            }
        }
    }
}