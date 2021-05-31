
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ColorPalette : UdonSharpBehaviour
{
    
    // TODO: Color palette selection/saving
    
    public Color[] paletteColors = new Color[42];

    public float redVal;
    public float greenVal;
    public float blueVal;
    
    void Start()
    {
        
    }

    public void setColor(int selection)
    {
        paletteColors[selection] = new Color(redVal, greenVal, blueVal);
    }

    public void setRed(float val)
    {
        redVal = val;
    }
    
    public void setBlue(float val)
    {
       blueVal = val;
    }
    
    public void setGreen(float val)
    {
       greenVal = val;
    }
    
}
