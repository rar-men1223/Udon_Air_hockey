
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class MalletDisk: UdonSharpBehaviour
{
    public GameObject Disk;

    public Text text;

    void Start()
    {
        
    }

    public void OnCollisionEnter(Collision collision)
    {
        text.text += "CollisionCheck1:" + collision.gameObject.name + "\n";
        // 接触相手がDiskなら権限を取得
        if (collision.gameObject.name == Disk.name)
        {
            text.text += "CollisionCheck2\n";

            if(Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {

                text.text += "Object Name:" + gameObject.name + " Current Owner:" + Networking.GetOwner(gameObject).playerId + "\n";
                text.text += "Object Name:" + Disk.name       + " Current Owner:" + Networking.GetOwner(Disk).playerId + "\n";

                Networking.SetOwner(Networking.LocalPlayer, Disk);

                text.text += "Object Name:" + gameObject.name + " New Owner    :" + Networking.GetOwner(gameObject).playerId + "\n";
                text.text += "Object Name:" + Disk.name       + " New Owner    :" + Networking.GetOwner(Disk).playerId + "\n";

            }
        }
    }
}
