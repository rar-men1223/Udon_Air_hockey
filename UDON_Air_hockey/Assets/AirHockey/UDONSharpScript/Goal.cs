
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Goal : UdonSharpBehaviour
{
    public GameEngine engine;
    public GameObject disk;
    public ParticleSystem hitEffect;

    private AudioSource audioSource;
    public AudioClip hitClip;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == disk.name)
        {
            // 衝突エフェクト再生
            hitEffect.transform.position = other.ClosestPointOnBounds(this.transform.position);
            hitEffect.Play();

            // 衝突音再生
            audioSource.PlayOneShot(hitClip);

            // 接触したらVelocityをゼロにする
            disk.GetComponent<Rigidbody>().velocity = Vector3.zero;

            if (Networking.IsOwner(Networking.LocalPlayer, gameObject))
            {
                //ゴール演出
                Networking.SetOwner(Networking.LocalPlayer, disk);
                engine.GoalFuction();
            }
        }
    }

}
