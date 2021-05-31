
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class InkSpotDetection : UdonSharpBehaviour
{
    
    public Camera cam;
    
    private void Start()
    {
        cam.enabled = false;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (Networking.IsOwner(gameObject))
        {
            if (other.name.Contains("Color"))
            {
                cam.enabled = true;
            }
        }
        
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (Networking.IsOwner(gameObject))
        {
            if (other.name.Contains("Color"))
            {
                cam.enabled = false;
            }
        }
    }
}