using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour {

    // Component References
    private Rigidbody rb;
    [SerializeField] private Material deadParticle;
    [SerializeField] private Material aliveParticle;
    // Variables
    private Vector3 lastPosition;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody>();
        lastPosition = transform.position;
            //DisableParticle();

        Time.fixedDeltaTime = 0.01f;
    }

    void OnEnable()
    {
        transform.localPosition = new Vector3(0.0011f, 0.085f, -0.00037f);
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update() {
        if(Input.GetKeyDown(KeyCode.X)) gameObject.SetActive(true); // For debugging
        // Try to move the particle back to the last position
        //Vector3 direction = lastPosition - transform.position;
        //rb.AddForce(direction * 10.0f * 100f);

        // Update the last position
        //lastPosition = transform.position;
    }

    private void DisableParticle()
    {
        gameObject.SetActive(false);
    }

    
}
