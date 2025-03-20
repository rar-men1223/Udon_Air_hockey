
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class Reset : UdonSharpBehaviour
{
    public GameEngine engine;
    public GameObject disk;
    public GameObject Mallet1;
    public GameObject Mallet2;

    public Text text;

    void Start()
    {
    }

    public override void Interact()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, ("resetPos"));
    }

    public void resetPos()
    {
        //オブジェクトの位置を初期化
        //disk.transform.localPosition = new Vector3(-1f,0.515f,0f);
        disk.transform.rotation = Quaternion.identity;
        //disk.GetComponent<Rigidbody>().velocity = Vector3.zero;
        Mallet1.transform.localPosition = new Vector3(-1.5f, 0.515f, 0f);
        Mallet1.transform.rotation = Quaternion.Euler(-90, -90, 0);
        Mallet1.GetComponent<Rigidbody>().velocity = Vector3.zero;
        Mallet2.transform.localPosition = new Vector3(1.5f, 0.515f, 0);
        Mallet2.transform.rotation = Quaternion.Euler(-90, 90, 0);
        Mallet2.GetComponent<Rigidbody>().velocity = Vector3.zero;

        engine.SetNextServe();
        engine.StartTurn();
    }
}
