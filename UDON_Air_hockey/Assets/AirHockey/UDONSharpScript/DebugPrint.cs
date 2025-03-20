
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DebugPrint : UdonSharpBehaviour
{
    public Disk disk;

    void Start()
    {
        
    }

    public override void Interact()
    {
        disk.DebugSyncPrint();
    }
}
