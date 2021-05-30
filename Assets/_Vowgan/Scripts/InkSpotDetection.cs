
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
        if (other.name.Contains("Palette"))
        {
            cam.enabled = true;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.name.Contains("Palette"))
        {
            cam.enabled = false;
        }
    }
}