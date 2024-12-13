using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectControl : MonoBehaviour
{
    public GameObject particleObject;
    public GameObject mazeObject;
    public GameObject waterObject;

    void Start() {
        particleObject.SetActive(false);
        mazeObject.SetActive(false);
        waterObject.SetActive(true);
    }

    private void Awake()
    {
        // Find the GameObject named "Particle" in the scene

        if (particleObject == null)
        {
            Debug.LogWarning("GameObject with the name 'Particle' not found in the scene.");
        }

        DisableParticle();
    }

    void Update() 
    {
        if(Input.GetKeyDown(KeyCode.X)){
            EnableParticle();
        }
        if(Input.GetKeyDown(KeyCode.B)){
            DisableParticle();
        }
    }

    public void ToggleParticle() {
        particleObject.SetActive(!particleObject.activeSelf);
        mazeObject.SetActive(!mazeObject.activeSelf);
        waterObject.SetActive(!waterObject.activeSelf);
    }

    /// <summary>
    /// Enables the "Particle" GameObject if it exists.
    /// </summary>
    public void EnableParticle()
    {
        if (particleObject != null)
        {
            particleObject.SetActive(true);
            mazeObject.SetActive(true);
            waterObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Cannot enable 'Particle' - GameObject not found.");
        }
    }

    /// <summary>
    /// Disables the "Particle" GameObject if it exists.
    /// </summary>
    public void DisableParticle()
    {
        if (particleObject != null)
        {
            particleObject.SetActive(false);
            mazeObject.SetActive(false);
            waterObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Cannot disable 'Particle' - GameObject not found.");
        }
    }
}
