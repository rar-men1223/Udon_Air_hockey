
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class StartGame_5Point : UdonSharpBehaviour
{
    public GameObject gameEngine;

    // Enumが使えないので無理矢理定数化
    private const int Mode_Practice = 0;
    private const int Mode_5PointMatch = 1;

    void Start()
    {

    }

    public override void Interact()
    {
        gameEngine.GetComponent<GameEngine>().StartGame(Mode_5PointMatch);
    }
}
