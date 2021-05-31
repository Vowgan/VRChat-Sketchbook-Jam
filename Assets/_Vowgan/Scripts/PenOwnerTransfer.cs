
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PenOwnerTransfer : UdonSharpBehaviour
{
    public GameObject tip;
    void Start()
    {
        
    }

    public override void OnPickup()
    {
        Networking.SetOwner(Networking.LocalPlayer, tip);
    }
}
