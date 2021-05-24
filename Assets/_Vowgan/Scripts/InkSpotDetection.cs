
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class InkSpotDetection : UdonSharpBehaviour
{
    
    public Camera cam;
    public Color startColor = new Color(0.1f, 0.1f, 0.1f);
    
    private void Start()
    {
        cam.enabled = false;
        GetComponent<MeshRenderer>().material.color = startColor;
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