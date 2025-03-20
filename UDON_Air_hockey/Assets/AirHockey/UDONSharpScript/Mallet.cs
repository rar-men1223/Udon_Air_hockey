
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class Mallet : UdonSharpBehaviour
{
    public GameObject gameEngine;
    public GameObject disk;
    public GameObject worldGate;
    public GameObject goal;

    void Start()
    {
        GetComponent<Rigidbody>().centerOfMass = new Vector3(0,-0.01f,0);
   
    }

    private void Update()
    {
        // 台の外に持ち出そうとしたら強制的に戻す
        if(transform.localPosition.z < -1.15f)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -1.15f);
        }else if(transform.localPosition.z > 1.15f)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 1.15f);
        }
        if(gameObject.name == "Mallet_a")
        {
            if (transform.localPosition.x > -0.2f)
            {
                transform.localPosition = new Vector3(-0.2f, transform.localPosition.y, transform.localPosition.z);
            }
            else if (transform.localPosition.x < -2.2f)
            {
                transform.localPosition = new Vector3(-2.2f, transform.localPosition.y, transform.localPosition.z);
            }
        }
        else if(gameObject.name == "Mallet_b")
        {
            if (transform.localPosition.x < 0.2f)
            {
                transform.localPosition = new Vector3(0.2f, transform.localPosition.y, transform.localPosition.z);
            }
            else if (transform.localPosition.x > 2.2f)
            {
                transform.localPosition = new Vector3(2.2f, transform.localPosition.y, transform.localPosition.z);
            }
        }
    }

    public override void OnPickup()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        Networking.SetOwner(Networking.LocalPlayer, worldGate);
        Networking.SetOwner(Networking.LocalPlayer, goal);

        //Pickup時に自分の世界でだけ物理演算有効にする
        gameObject.layer = 23;

        //MainScreenのプレイヤー名表示を更新
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, ("UpdatePlayerName"));
    }

    // 全ユーザーにプレイヤー名表示更新をコール
    public void UpdatePlayerName()
    {

        gameEngine.GetComponent<GameEngine>().UpdatePlayerName();
    }

    public override void OnDrop()
    {
        //Drop時に物理演算無効
        gameObject.layer = 13;
    }

}
