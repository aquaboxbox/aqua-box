using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerStart : MonoBehaviour
{   
    public bool gameStarted = false;

    void OnTriggerEnter(Collider other)
    {

        Debug.Log("Hello");
        // Print "Hello" to the console when a collision occurs
        if(gameStarted)
        {
            Debug.Log("Game already begun");
        }
        else
        {
            Debug.Log("Game started");
            gameStarted = true;
        }

    }
}
