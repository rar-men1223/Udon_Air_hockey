
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class wallCollider3 : UdonSharpBehaviour
{
    public GameObject disk;
    public ParticleSystem hitEffect;

    private Rigidbody rb;

    private AudioSource audioSource;
    public AudioClip hitClip;

    void Start()
    {
        rb = disk.GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == disk.name)
        {
            //衝突エフェクト再生
            hitEffect.transform.position = other.ClosestPointOnBounds(this.transform.position);
            hitEffect.Play();

            // 衝突音再生
            audioSource.PlayOneShot(hitClip);

            // 壁反射
            rb.velocity = new Vector3(rb.velocity.x * -1, rb.velocity.y, rb.velocity.z);

            // めり込み対策位置補正
            disk.transform.localPosition = new Vector3(1.899f, disk.transform.localPosition.y, disk.transform.localPosition.z);
        }
    }

}
