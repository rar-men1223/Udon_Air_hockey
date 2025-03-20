
using UdonSharp;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class SlipAdjuster : UdonSharpBehaviour
{
    public GameEngine gameEngine;

    void Start()
    {
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (Networking.LocalPlayer == player)
        {
            //gameEngine.UpdateSlipRatio();
        }
    }

    public void updateRatio()
    {
        //gameEngine.UpdateSlipRatio();
    }
}
