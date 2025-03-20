
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class Adjuster : UdonSharpBehaviour
{
    public GameObject floorAdjuster;
    private float baseHeight;
    private Slider _Adjinter;

    void Start()
    {
        // 基準となる高さを取得
        baseHeight = floorAdjuster.transform.position.y;

        _Adjinter = gameObject.GetComponent<Slider>();
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (Networking.LocalPlayer == player)
        {

            // 基準となる高さを取得
            baseHeight = floorAdjuster.transform.position.y;

            _Adjinter = gameObject.GetComponent<Slider>();
        }
    }

    public void Adjust()
    {
        // 現在のスライダーの値に合わせて床の高さと現在位置を変更
        floorAdjuster.transform.position = new Vector3(floorAdjuster.transform.position.x, baseHeight + _Adjinter.value, floorAdjuster.transform.position.z);
    }

}
