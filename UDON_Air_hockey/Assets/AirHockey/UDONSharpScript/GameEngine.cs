
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class GameEngine : UdonSharpBehaviour
{
    public GameObject Mallet_a;
    public GameObject Mallet_b;
    public Text PlayerName_a;
    public Text PlayerName_b;
    public Text Score_a;
    public Text Score_b;
    public Text GameModeText;
    public GameObject disk;
    public GameObject effectObject;

    // Enumが使えないので無理矢理定数化
    private const int Mode_Practice = 0;
    private const int Mode_5PointMatch = 1;

    private int localGameMode;
    private int localScore_a;
    private int localScore_b;
    private int matchPoint;

    private bool syncServerisRed;

    //Game管理用同期
    [UdonSynced(UdonSyncMode.None)]
    private int syncGameMode;
    [UdonSynced(UdonSyncMode.None)]
    private int syncScore_a;
    [UdonSynced(UdonSyncMode.None)]
    private int syncScore_b;

    void Start()
    {
        syncGameMode = 0;
        syncScore_a = 0;
        syncScore_b = 0;
        matchPoint = int.MaxValue;
        syncServerisRed = true;
        localGameMode = Mode_Practice;
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (Networking.LocalPlayer == player)
        {
            matchPoint = int.MaxValue;
            syncServerisRed = true;
            localGameMode = syncGameMode;
        }
    }

    private void Update()
    {
        // 同期変数に変化があれば更新
        if (syncGameMode != localGameMode)
        {
            localGameMode = syncGameMode;
            switch (localGameMode)
            {
                case Mode_5PointMatch:
                    GameModeText.text = "- 5 Point Match -";
                    matchPoint = 5;
                    break;
                default:
                    GameModeText.text = "- Practice Mode -";
                    matchPoint = int.MaxValue;
                    break;
            }
        }

        if(syncScore_a != localScore_a)
        {
            localScore_a = syncScore_a;
            Score_a.text = string.Format("{0}", localScore_a);
        }

        if (syncScore_b != localScore_b)
        {
            localScore_b = syncScore_b;
            Score_b.text = string.Format("{0}", localScore_b);
        }
    }

    // MainScreenのプレイヤー名を更新
    public void UpdatePlayerName()
    {
        VRCPlayerApi player;
        player = Networking.GetOwner(Mallet_a);
        PlayerName_a.text = player.displayName;
        player = Networking.GetOwner(Mallet_b);
        PlayerName_b.text = player.displayName;
    }

    public void StartGame(int gameMode)
    {
        //GameModeを設定
        //同期情報更新用にOwner権取得
        Networking.SetOwner(Networking.LocalPlayer, gameObject);

        syncGameMode = gameMode;
        syncScore_a = 0;
        syncScore_b = 0;

        //とりあえず赤からスタート(後でランダム化)
        syncServerisRed = true;

        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, ("PlayStartGameEffect"));

        // disk位置がワープゾーンだとフリーズするので位置を調整
        disk.transform.localPosition = Vector3.zero;
    }

    public void PlayStartGameEffect()
    {
        // ゲーム開始演出
        // effectObjectをプレイヤーに向ける
        VRCPlayerApi player = Networking.LocalPlayer;
        effectObject.transform.LookAt(player.GetPosition());
        // アニメーション再生
        effectObject.GetComponent<Animator>().SetTrigger("GameStart");
    }

    // ゲーム開始演出後の最初のサーブ
    public void FirstServe()
    {
        // 赤側にサーブ(Mallet_aのOwnerが実行)
        if (Networking.IsOwner(Networking.LocalPlayer, Mallet_a))
        {
            Networking.SetOwner(Networking.LocalPlayer, disk);
            disk.GetComponent<Disk>().ServeDisk();
        }
        // Diskを有効化して再開
        disk.GetComponent<Disk>().StartTurn();
    }

    // ゴール接触時にコールされる
    public void GoalFuction()
    {
        if (Networking.IsOwner(Networking.LocalPlayer, Mallet_a))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            syncScore_b++;
            // 規定点数以上になったら終了演出
            if (syncScore_b >= matchPoint)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, ("GameSetNetworkEvent"));
            }
            else
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, ("GoalNetworkEventRed"));
            }
        }
        else if(Networking.IsOwner(Networking.LocalPlayer, Mallet_b))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            syncScore_a++;
            // 規定点数以上になったら終了演出
            if (syncScore_a >= matchPoint)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, ("GameSetNetworkEvent"));
            }
            else
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, ("GoalNetworkEventBlue"));
            }
        }
    }

    public void GoalNetworkEventRed()
    {
        syncServerisRed = true;
        GoalEffect();
    }

    public void GoalNetworkEventBlue()
    {
        syncServerisRed = false;
        GoalEffect();
    }

    public void GoalEffect()
    {
        // disk位置がワープゾーンだとフリーズするので位置を調整
        disk.transform.localPosition = Vector3.zero;
        disk.GetComponent<Rigidbody>().velocity = Vector3.zero;
        // diskを無効化
        disk.GetComponent<Disk>().chargeEffect.Stop();
        disk.GetComponent<Disk>().isMoving = false;

        //次のサーブを仕込む(アニメーション中に通信するためこのタイミング)
        SetNextServe();

        // effectObjectをプレイヤーに向ける
        VRCPlayerApi player = Networking.LocalPlayer;
        effectObject.transform.LookAt(player.GetPosition());
        // アニメーション再生
        effectObject.GetComponent<Animator>().SetTrigger("Goal");
    }

    public void GameSetNetworkEvent()
    {
        // disk位置がワープゾーンだとフリーズするので位置を調整
        disk.transform.localPosition = Vector3.zero;
        // diskを無効化
        disk.GetComponent<Disk>().chargeEffect.Stop();
        disk.GetComponent<Disk>().isMoving = false;
        // effectObjectをプレイヤーに向ける
        VRCPlayerApi player = Networking.LocalPlayer;
        effectObject.transform.LookAt(player.GetPosition());
        // アニメーション再生
        effectObject.GetComponent<Animator>().SetTrigger("Game");
    }

    public void SetNextServe()
    {
        // 次のサーブの前にターンを変更
        disk.GetComponent<Disk>().localTurn++;

        // サーブ権を持っている人がサーブ
        if (Networking.IsOwner(Networking.LocalPlayer, Mallet_a) && syncServerisRed == true)
        {
            Networking.SetOwner(Networking.LocalPlayer, disk);
            disk.GetComponent<Disk>().ServeDisk();
        }
        else if (Networking.IsOwner(Networking.LocalPlayer, Mallet_b) && syncServerisRed == false)
        {
            Networking.SetOwner(Networking.LocalPlayer, disk);
            disk.GetComponent<Disk>().ServeDisk();
        }
    }

    public void StartTurn()
    {
        // 再開
        disk.GetComponent<Disk>().StartTurn();
    }
}
