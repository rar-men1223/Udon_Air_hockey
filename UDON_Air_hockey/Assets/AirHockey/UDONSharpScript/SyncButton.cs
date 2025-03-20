
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class SyncButton : UdonSharpBehaviour
{
    public Dropdown SyncCycle;
    public Toggle WorldGate;

    public GameObject worldGate_a;
    public GameObject worldGate_b;
    public GameObject disk;
    public Text currentSetting;

    private int syncCycleParam;
    private float[] syncCycleList;

    [UdonSynced(UdonSyncMode.None)]
    private int s_syncCycle;
    [UdonSynced(UdonSyncMode.None)]
    private bool s_worldGate;

    void Start()
    {
        syncCycleList = new float[] { 0f, 0.1f, 0.5f, 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };
        s_worldGate = true;
    }

    public override void Interact()
    {
        SyncConfig();
    }

    public void SyncConfig()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        s_syncCycle = SyncCycle.value;
        s_worldGate = WorldGate.isOn;
    }

    private void Update()
    {
        if(s_worldGate != worldGate_a.activeSelf)
        {
            worldGate_a.SetActive(s_worldGate);
            worldGate_b.SetActive(s_worldGate);
        }
        if(s_syncCycle != syncCycleParam)
        {
            syncCycleParam = s_syncCycle;
            //syncCycle ms変換
            //disk.gameObject.GetComponent<Disk>().syncCycleTime = syncCycleList[syncCycleParam];
        }

        //表示更新
        currentSetting.text = string.Format("[Current Setting]\nSynchronous Cycle:{0:f}\nWorldGate:{1}\n", syncCycleList[syncCycleParam], s_worldGate);
    }
}
