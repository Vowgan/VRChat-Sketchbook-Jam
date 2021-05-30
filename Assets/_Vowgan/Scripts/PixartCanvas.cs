
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PixartCanvas : UdonSharpBehaviour
{

    public Camera cam;
    
    private void Start()
    {
        cam.enabled = false;
    }
        
    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("Stylus"))
        {
            cam.enabled = true;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.name.Contains("Stylus"))
        {
            cam.enabled = false;
        }
    }

    public void Clear()
    {
        cam.backgroundColor = Color.black;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.Render();
        cam.clearFlags = CameraClearFlags.Nothing;
        cam.Render();
    }
    
}
