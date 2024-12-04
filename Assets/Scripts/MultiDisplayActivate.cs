using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiDisplayActivate : MonoBehaviour
{
    void Start()
    {
        if (Display.displays.Length > 1)
            Display.displays[1].Activate(Display.displays[1].systemWidth,
                                         Display.displays[1].systemHeight, 60);

        Debug.Log("Activated display");
    }
}