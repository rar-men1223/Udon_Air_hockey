
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PickUpObject : UdonSharpBehaviour
{
    public GameObject Mallet;
    public float xMin;
    public float xMax;
    public float zMin;
    public float zMax;
    public float GripBorder;

    void Start()
    {
        
    }

    public override void OnPickup()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        Networking.SetOwner(Networking.LocalPlayer, Mallet);
    }

    public override void OnDrop()
    {
        // DropしたらMalletの位置に戻す
        this.transform.localPosition = Mallet.transform.localPosition;
        this.transform.rotation = Quaternion.Euler(-90f,0f,-90f);
    }

    private void Update()
    {
        // 現在位置が有効範囲外なら強制的にDropさせる
        if( (this.transform.localPosition.x < xMin - GripBorder) ||
            (this.transform.localPosition.x > xMax + GripBorder) ||
            (this.transform.localPosition.z < zMin - GripBorder) ||
            (this.transform.localPosition.z > zMax + GripBorder))
        {
            OnDrop();
        }

        // Malletの位置補正
        Vector3 MalletPos = new Vector3(this.transform.localPosition.x, Mallet.transform.localPosition.y, this.transform.localPosition.z);
        if (MalletPos.x < xMin) MalletPos.x = xMin;
        if (MalletPos.x > xMax) MalletPos.x = xMax;
        if (MalletPos.z < zMin) MalletPos.z = zMin;
        if (MalletPos.z > zMax) MalletPos.z = zMax;

        // PickUpObejctの位置にMalletを連動させる
        Mallet.transform.localPosition = MalletPos;
    }
}
