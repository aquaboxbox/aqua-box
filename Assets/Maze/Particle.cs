using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour {

    // Component References
    private Rigidbody rb;

    // Variables
    private Vector3 lastPosition;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody>();
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update() {
        
        // Try to move the particle back to the last position
        Vector3 direction = lastPosition - transform.position;
        rb.AddForce(direction * 10.0f * 100f);

        // Update the last position
        lastPosition = transform.position;
    }
}
