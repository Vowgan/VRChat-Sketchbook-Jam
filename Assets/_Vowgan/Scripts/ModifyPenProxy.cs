
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ModifyPenProxy : UdonSharpBehaviour
{
    public Texture2D onePixel;
    public Camera stylusCamera;
    
    private Rect _cameraRect;
    
    void Start()
    {
        _cameraRect = stylusCamera.pixelRect;
    }

    private void OnPostRender()
    {
        onePixel.ReadPixels(_cameraRect, 0, 0, false);
        onePixel.Apply( false );
    }
}
