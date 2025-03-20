
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class Disk : UdonSharpBehaviour
{

    private const int syncTypeAudience = 0;
    private const int syncTypePlayer1 = 1;
    private const int syncTypePlayer2 = 2;

    public GameObject Mallet_a;
    public GameObject Mallet_b;
    public GameObject worldGate_a;
    public GameObject worldGate_b;
    public Text text;
    public ParticleSystem chargeEffect;
    public ParticleSystem warpEffect;

    private Rigidbody rb;
    private uint P1localCount;
    private uint P2localCount;
    private uint P1maxSyncCount;
    private uint P2maxSyncCount;

    public uint localTurn;

    private AudioSource audioSource;
    public AudioClip warpClip;
    public AudioClip gateClip;
    public AudioClip hitClip;

    // Activeで操作すると動作がおかしくなるので内部でフラグを持つ
    public bool isMoving;

    // gate通過待ち中に触れないようにする
    private bool isGateStop;

    //現在のターン(遅延パケット混入防止のため)
    [UdonSynced(UdonSyncMode.None)]
    private uint syncTurn;

    //P1側同期(P2が受信する)
    [UdonSynced(UdonSyncMode.None)]
    private uint P1syncCount;
    [UdonSynced(UdonSyncMode.None)]
    private float P1pos_x;
    [UdonSynced(UdonSyncMode.None)]
    private float P1pos_y;
    [UdonSynced(UdonSyncMode.None)]
    private float P1pos_z;
    [UdonSynced(UdonSyncMode.None)]
    private float P1vel_x;
    [UdonSynced(UdonSyncMode.None)]
    private float P1vel_y;
    [UdonSynced(UdonSyncMode.None)]
    private float P1vel_z;

    //P2側同期(P1が受信する)
    [UdonSynced(UdonSyncMode.None)]
    private uint P2syncCount;
    [UdonSynced(UdonSyncMode.None)]
    private float P2pos_x;
    [UdonSynced(UdonSyncMode.None)]
    private float P2pos_y;
    [UdonSynced(UdonSyncMode.None)]
    private float P2pos_z;
    [UdonSynced(UdonSyncMode.None)]
    private float P2vel_x;
    [UdonSynced(UdonSyncMode.None)]
    private float P2vel_y;
    [UdonSynced(UdonSyncMode.None)]
    private float P2vel_z;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        P1syncCount = 0;
        P1localCount = 0;
        P2syncCount = 0;
        P2localCount = 0;

        P1maxSyncCount = 0;
        P2maxSyncCount = 0;

        syncTurn = 0;
        localTurn = 0;

        isMoving = true;

        audioSource = GetComponent<AudioSource>();
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (Networking.LocalPlayer == player)
        {
            rb = GetComponent<Rigidbody>();

            P1localCount = 0;
            P2localCount = 0;
            localTurn = syncTurn;

            isMoving = true;

            audioSource = GetComponent<AudioSource>();
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == Mallet_a.name)
        {
            //同期情報更新用にOwner権取得
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            //同期用パラメータ確保
            UpdateSyncParam(syncTypePlayer1);

            // 効果音再生(手元なので0.5)
            audioSource.PlayOneShot(hitClip, 0.5f);
        }
        else if (collision.gameObject.name == Mallet_b.name)
        {
            //同期情報更新用にOwner権取得
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            //同期用パラメータ確保
            UpdateSyncParam(syncTypePlayer2);

            // 効果音再生(手元なので0.5)
            audioSource.PlayOneShot(hitClip, 0.5f);
        }
    }

    public void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.name == Mallet_a.name)
        {
            //同期用パラメータ確保
            UpdateSyncParam(syncTypePlayer1);

        }
        else if (collision.gameObject.name == Mallet_b.name)
        {
            //同期用パラメータ確保
            UpdateSyncParam(syncTypePlayer2);

        }
    }

    public void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name == Mallet_a.name)
        {
            //同期用パラメータ確保
            UpdateSyncParam(syncTypePlayer1);

            text.text = text.text.Remove(0, text.text.IndexOf("\n") + 1);
            text.text += string.Format("Send turn:{0,3} P1syncCount:{1,10} P1localCount:{2,10}  Position({3:f3},{4:f3},{5:f3}) Velocity({6:f3},{7:f3},{8:f3})\n", syncTurn, P1syncCount, P1localCount, P1pos_x, P1pos_y, P1pos_z, P1vel_x, P1vel_y, P1vel_z);
        }
        else if (collision.gameObject.name == Mallet_b.name)
        {
            //同期用パラメータ確保
            UpdateSyncParam(syncTypePlayer2);

            text.text = text.text.Remove(0, text.text.IndexOf("\n") + 1);
            text.text += string.Format("Send turn:{0,3} P2syncCount:{1,10} P2localCount:{2,10}  Position({3:f3},{4:f3},{5:f3}) Velocity({6:f3},{7:f3},{8:f3})\n", syncTurn, P2syncCount, P2localCount, P2pos_x, P2pos_y, P2pos_z, P2vel_x, P2vel_y, P2vel_z);
        }

    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == worldGate_a.name)
        {
            if (Networking.IsOwner(Networking.GetOwner(Mallet_b), gameObject))
            {
                // Gate_aにbのDiskが接触
                if (Networking.IsOwner(Networking.LocalPlayer, other.gameObject))
                {
                    // GateのOwnerの場合、DiskのOwner権取得
                    Networking.SetOwner(Networking.LocalPlayer, gameObject);
                    UpdateSyncParam(syncTypePlayer1);

                    text.text = text.text.Remove(0, text.text.IndexOf("\n") + 1);
                    text.text += string.Format("Gate turn:{0,3} P1syncCount:{1,10} P1localCount:{2,10}  Position({3:f3},{4:f3},{5:f3}) Velocity({6:f3},{7:f3},{8:f3})\n", syncTurn, P1syncCount, P1localCount, P1pos_x, P1pos_y, P1pos_z, P1vel_x, P1vel_y, P1vel_z);
                }
                else
                {
                    // Gate_aのOwner意外は停止して同期待ち
                    rb.velocity = Vector3.zero;
                    chargeEffect.transform.position = other.bounds.ClosestPoint(transform.position);
                    chargeEffect.Play();
                    // 効果音再生
                    audioSource.PlayOneShot(gateClip, 0.7f);
                }
            }
        }
        else if (other.gameObject.name == worldGate_b.name)
        {
            if (Networking.IsOwner(Networking.GetOwner(Mallet_a), gameObject))
            {
                // Gate_bにaのDiskが接触
                if (Networking.IsOwner(Networking.LocalPlayer, other.gameObject))
                {
                    // DiskOwner(最終接触者)ではなく自陣Gateの場合、Owner権取得
                    Networking.SetOwner(Networking.LocalPlayer, gameObject);
                    UpdateSyncParam(syncTypePlayer2);

                    text.text = text.text.Remove(0, text.text.IndexOf("\n") + 1);
                    text.text += string.Format("Gate turn:{0,3} P2syncCount:{1,10} P2localCount:{2,10}  Position({3:f3},{4:f3},{5:f3}) Velocity({6:f3},{7:f3},{8:f3})\n", syncTurn, P2syncCount, P2localCount, P2pos_x, P2pos_y, P2pos_z, P2vel_x, P2vel_y, P2vel_z);
                }
                else
                {
                    // Gate_aのOwner意外は停止して同期待ち
                    rb.velocity = Vector3.zero;
                    chargeEffect.transform.position = other.bounds.ClosestPoint(transform.position);
                    chargeEffect.Play();
                    // 効果音再生
                    audioSource.PlayOneShot(gateClip, 0.7f);
                }
            }
        }
    }

    public void Update()
    {
        // 動作無効なら何もしない
        if (!isMoving)
        {
            return;
        }

        // SyncCountの最大値を確認・更新する(遅延パケットの古い番号を拾って更新不可になるのを防ぐため)
        if (P1maxSyncCount < P1syncCount)
        {
            P1maxSyncCount = P1syncCount;
        }
        if (P2maxSyncCount < P2syncCount)
        {
            P2maxSyncCount = P2syncCount;
        }

        //同期変数の更新を検出したら反映チェック
        // P2(青)側 or 観客はP1情報を受信
        if (Networking.IsOwner(Networking.LocalPlayer, Mallet_b) ||
           ((!Networking.IsOwner(Networking.LocalPlayer, Mallet_a)) &&
            (!Networking.IsOwner(Networking.LocalPlayer, Mallet_b))))
        {
            if ((P1localCount < P1syncCount) && (syncTurn == localTurn))
            {
                //エフェクト出てたら止める
                chargeEffect.Stop();
                audioSource.Stop();

                VelocityOverride(syncTypePlayer1);

            }
        }

        // P1(赤)側 or 観客はP1情報を受信
        if (Networking.IsOwner(Networking.LocalPlayer, Mallet_a) ||
           ((!Networking.IsOwner(Networking.LocalPlayer, Mallet_a)) &&
            (!Networking.IsOwner(Networking.LocalPlayer, Mallet_b))))
        {
            if ((P2localCount < P2syncCount) && (syncTurn == localTurn))
            {
                //エフェクト出てたら止める
                chargeEffect.Stop();
                audioSource.Stop();

                VelocityOverride(syncTypePlayer2);

                // LayerをAirHockeyに変更して確実に触れるようにする
                gameObject.layer = 23;
            }
        }

        // ターンが同じかつスルーした同期は後から拾わない
        if (localTurn == syncTurn)
        {
            P1localCount = P1syncCount;
            P2localCount = P2syncCount;
        }

        // 観戦者対応 localTurn=0の場合のみ強制受信
        if (localTurn < syncTurn && localTurn == 0)
        {
            localTurn = syncTurn;
        }

    }

    // 同期パラメータを更新する(Ownerのみ実行可)
    public void UpdateSyncParam(int syncType)
    {
        if (Networking.IsOwner(Networking.LocalPlayer, gameObject))
        {
            if (syncType == syncTypePlayer1)
            {
                //取得した同期変数が古い場合は手元の最新番号で更新
                if (P1maxSyncCount > P1syncCount)
                {
                    P1syncCount = P1maxSyncCount + (uint)1;
                }
                //同期用パラメータ確保
                P1syncCount++;
                P1pos_x = gameObject.transform.localPosition.x;
                P1pos_y = gameObject.transform.localPosition.y;
                P1pos_z = gameObject.transform.localPosition.z;
                P1vel_x = rb.velocity.x;
                P1vel_y = rb.velocity.y;
                P1vel_z = rb.velocity.z;
                syncTurn = localTurn;

                //自分は同期対象外
                P1localCount = P1syncCount;
            }
            else if(syncType == syncTypePlayer2)
            {
                //取得した同期変数が古い場合は手元の最新番号で更新
                if (P2maxSyncCount > P2syncCount)
                {
                    P2syncCount = P2maxSyncCount + (uint)1;
                }
                //同期用パラメータ確保
                P2syncCount++;
                P2pos_x = gameObject.transform.localPosition.x;
                P2pos_y = gameObject.transform.localPosition.y;
                P2pos_z = gameObject.transform.localPosition.z;
                P2vel_x = rb.velocity.x;
                P2vel_y = rb.velocity.y;
                P2vel_z = rb.velocity.z;
                syncTurn = localTurn;

                //自分は同期対象外
                P2localCount = P2syncCount;
            }
        }
    }

    public void VelocityOverride(int syncType)
    {
        if (syncType == syncTypePlayer1)
        {
            text.text = text.text.Remove(0, text.text.IndexOf("\n") + 1);
            text.text += string.Format("Get turn:{0,3} P1syncCount:{1,10} P1localCount:{2,10}  Position({3:f3},{4:f3},{5:f3}) Velocity({6:f3},{7:f3},{8:f3})\n", syncTurn, P1syncCount, P1localCount, P1pos_x, P1pos_y, P1pos_z, P1vel_x, P1vel_y, P1vel_z);

            Vector3 newPos = new Vector3(P1pos_x, P1pos_y, P1pos_z);
            // 距離が移動距離が一定以上ならワープ演出
            if (Vector3.Distance(transform.localPosition, newPos) > 0.3)
            {
                //エフェクト再生
                warpEffect.transform.localPosition = this.transform.localPosition;
                warpEffect.Play();
                // 効果音再生
                audioSource.PlayOneShot(warpClip,0.8f);
            }

            this.transform.localPosition = newPos;
            rb.velocity = new Vector3(P1vel_x, P1vel_y, P1vel_z);
        }
        else if(syncType == syncTypePlayer2)
        {
            text.text = text.text.Remove(0, text.text.IndexOf("\n") + 1);
            text.text += string.Format("Get turn:{0,3} P2syncCount:{1,10} P2localCount:{2,10}  Position({3:f3},{4:f3},{5:f3}) Velocity({6:f3},{7:f3},{8:f3})\n", syncTurn, P2syncCount, P2localCount, P2pos_x, P2pos_y, P2pos_z, P2vel_x, P2vel_y, P2vel_z);

            Vector3 newPos = new Vector3(P2pos_x, P2pos_y, P2pos_z);
            // 距離が移動距離が一定以上ならワープ演出
            if (Vector3.Distance(transform.localPosition, newPos) > 0.3)
            {
                //エフェクト再生
                warpEffect.transform.localPosition = this.transform.localPosition;
                warpEffect.Play();
                // 効果音再生
                audioSource.PlayOneShot(warpClip,0.8f);
            }

            this.transform.localPosition = newPos;
            rb.velocity = new Vector3(P2vel_x, P2vel_y, P2vel_z);
        }
    }

    public void ServeDisk()
    {
        //指定されたフィールドにサーブを打ち出す
        if (Networking.IsOwner(Networking.LocalPlayer, Mallet_a))
        {
            P1syncCount++;
            P1pos_x = -1.0f;
            P1pos_y = 0.6f;
            P1pos_z = -0.9f;
            P1vel_x = 0.0f;
            P1vel_y = 0.0f;
            P1vel_z = 0.5f;

            P2syncCount++;
            P2pos_x = -1.0f;
            P2pos_y = 0.6f;
            P2pos_z = -0.9f;
            P2vel_x = 0.0f;
            P2vel_y = 0.0f;
            P2vel_z = 0.5f;

            syncTurn = localTurn;

            text.text = text.text.Remove(0, text.text.IndexOf("\n") + 1);
            text.text += string.Format("Start turn:{0,3} P1syncCount:{1,10} P1localCount:{2,10}  Position({3:f3},{4:f3},{5:f3}) Velocity({6:f3},{7:f3},{8:f3})\n", syncTurn, P1syncCount, P1localCount, P1pos_x, P1pos_y, P1pos_z, P1vel_x, P1vel_y, P1vel_z);

        }
        else if (Networking.IsOwner(Networking.LocalPlayer, Mallet_b))
        {
            P1syncCount++;
            P1pos_x = 1.0f;
            P1pos_y = 0.6f;
            P1pos_z = -0.9f;
            P1vel_x = 0.0f;
            P1vel_y = 0.0f;
            P1vel_z = 0.5f;

            P2syncCount++;
            P2pos_x = 1.0f;
            P2pos_y = 0.6f;
            P2pos_z = -0.9f;
            P2vel_x = 0.0f;
            P2vel_y = 0.0f;
            P2vel_z = 0.5f;

            syncTurn = localTurn;

            text.text = text.text.Remove(0, text.text.IndexOf("\n") + 1);
            text.text += string.Format("Start turn:{0,3} P2syncCount:{1,10} P2localCount:{2,10}  Position({3:f3},{4:f3},{5:f3}) Velocity({6:f3},{7:f3},{8:f3})\n", syncTurn, P2syncCount, P2localCount, P2pos_x, P2pos_y, P2pos_z, P2vel_x, P2vel_y, P2vel_z);

        }
    }

    public void DebugSyncPrint()
    {
        text.text = text.text.Remove(0, text.text.IndexOf("\n") + 1);
        text.text += string.Format("Debug Print  P1syncCount:{0,10} P1localCount:{1,10} P1maxSyncCount:{2,10} trun{3,3}\n", P1syncCount, P1localCount, P1maxSyncCount, syncTurn);
        text.text = text.text.Remove(0, text.text.IndexOf("\n") + 1);
        text.text += string.Format("Debug Print  P2syncCount:{0,10} P2localCount:{1,10} P2maxSyncCount:{2,10} turn{3,3}\n", P2syncCount, P2localCount, P2maxSyncCount, syncTurn);
    }

    public void StartTurn()
    {
        // 稀に更新が漏れるのでこのタイミングで補正
        if(localTurn < syncTurn)
        {
            localTurn = syncTurn;
        }
        // Diskを有効化して再開
        isMoving = true;
    }
}
