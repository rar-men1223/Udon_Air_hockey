
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class EffectObject : UdonSharpBehaviour
{
    public GameEngine engine;
    public AudioClip dora;
    public AudioClip whistle;
    public AudioClip gong;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

    }

    public void AfterGameStartAnime()
    {
        // 演出後のサーブ処理を呼ぶ(ローカルで全員実行し、engine側で権利者を選別)
        engine.FirstServe();
    }

    public void AfterGoal()
    {
        engine.StartTurn();
    }


    // 効果音再生系メソッド
    public void playDora()
    {
        audioSource.PlayOneShot(dora, 0.8f);
    }

    public void playwhistle()
    {
        audioSource.PlayOneShot(whistle, 0.5f);
    }

    public void playGong()
    {
        audioSource.PlayOneShot(gong, 0.5f);
    }
}
