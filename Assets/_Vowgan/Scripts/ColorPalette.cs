
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class ColorPalette : UdonSharpBehaviour
{
    
    public SpriteRenderer[] savedColors;
    public int currentSaveIndex = 0;
    public Image colorPreview;

    private float oldRedVal;
    private float oldGreenVal;
    private float oldBlueVal;
    
    [UdonSynced]public float redVal;
    [UdonSynced]public float greenVal;
    [UdonSynced]public float blueVal;

    public Slider redSlider;
    public Slider greenSlider;
    public Slider blueSlider;
    
    public ModifyPenProxy penProxy;
    
    void Start()
    {
        
    }

    public void setOwner()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
    }

    public override void OnDeserialization()
    {
        if (redVal != oldRedVal)
        {
            oldRedVal = redVal;
            redSlider.value = redVal;
        }
        
        if (blueVal != oldBlueVal)
        {
            oldBlueVal = blueVal;
            blueSlider.value = blueVal;
        }
        
        if (greenVal != oldGreenVal)
        {
            oldGreenVal = greenVal;
            greenSlider.value = greenVal;
        }
    }

    public void setColor()
    {
        colorPreview.color = new Color(redVal, greenVal, blueVal);
    }

    public void setRed()
    {
        redVal = redSlider.value;
        RequestSerialization();
        setColor();
    }
    
    public void setGreen()
    {
        greenVal = greenSlider.value;
        RequestSerialization();
        setColor();
    }
    
    public void setBlue()
    {
        blueVal = blueSlider.value;
        RequestSerialization();
        setColor();
    }

    public void saveColor()
    {
        savedColors[currentSaveIndex].enabled = true;
        savedColors[currentSaveIndex].color = new Color(redVal, greenVal, blueVal);
        
        currentSaveIndex += 1;
        if (currentSaveIndex >= 42)
        {
            currentSaveIndex = 0;
        }
    }
    
    public void modifyColor()
    {
        Color penColor = penProxy.onePixel.GetPixel(0, 0);
        
        Debug.Log( $"Red + {penColor.r * 255}" );
        Debug.Log( $"Green + {penColor.g * 255}" );
        Debug.Log( $"Blue + {penColor.b * 255}" );
        
        redSlider.value = penColor.r;
        greenSlider.value = penColor.g;
        blueSlider.value = penColor.b;
    }
    
}
