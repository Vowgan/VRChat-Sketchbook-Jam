
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ModifyPenProxy : UdonSharpBehaviour
{
    public Texture2D onePixel;
    public Camera stylusCamera;

    private Color oldColor;

    [UdonSynced]public Color syncedColor;
    
    private Rect _cameraRect;
    
    void Start()
    {
        _cameraRect = stylusCamera.pixelRect;
    }

    public override void OnDeserialization()
    {
        if (syncedColor != oldColor)
        {
            oldColor = syncedColor;
            ColorSync();
        }
       
    }
    
    private void OnPostRender()
    {
        
        if (!Networking.IsOwner(gameObject))
        {
            return;
        }

        onePixel.ReadPixels(_cameraRect, 0, 0, false);
        onePixel.Apply( false );

        syncedColor = onePixel.GetPixel(0, 0);
        RequestSerialization();
        
    }
    
    public void ColorSync()
    {
        stylusCamera.backgroundColor = syncedColor;
        stylusCamera.clearFlags = CameraClearFlags.SolidColor;
        stylusCamera.Render();
        stylusCamera.clearFlags = CameraClearFlags.Nothing;
    }
    
}
