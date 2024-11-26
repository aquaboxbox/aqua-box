using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Webcam : MonoBehaviour
{
    public RenderTexture RenderTarget;

    // Start is called before the first frame update
    void Start()
    {
        WebCamTexture webcamTexture = new WebCamTexture();
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = webcamTexture;
        webcamTexture.Play();
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        //you could also pass 'dest' as the second parameter. in that case it would
        //pass the result down the chain to the next component that uses this method
        //that is also attached to this camera gameobject. if 'dest' is equal to null
        //that means we would be at the end of the chain and the final result would be
        //rendered to the screen directly. in this case we are shortcutting by manually
        //passing a value of null.
        Graphics.Blit(RenderTarget, dest);
    }

}
