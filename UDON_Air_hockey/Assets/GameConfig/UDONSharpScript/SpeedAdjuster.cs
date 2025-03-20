
using UdonSharp;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class SpeedAdjuster : UdonSharpBehaviour
{
    public GameEngine gameEngine;

    void Start()
    {
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (Networking.LocalPlayer == player)
        {
            //gameEngine.UpdateSpeedRatio();
        }
    }

    public void updateRatio()
    {
        //gameEngine.UpdateSpeedRatio();
    }
}