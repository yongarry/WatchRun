using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class BLEManager : MonoBehaviour
{
    public bool isScanningDevices = false;
    public bool isScanningServices = false;
    public bool isScanningCharacteristics = false;
    public bool isSubscribed = false;
    // public Text subcribeText1;
    // public Text subcribeText2;
    public Text heartBeatText;
    public Text stateManager;
    public GameObject Player;

    public static int stepCount = 0;
    int Heartbeat = 0;
    int stepCountPast = 0;
    // int stepCountPastPast = 0;
    // int HeartbeatPast = 0;
    float velocity = 0.0f;

    private Animator an_Player;

    Dictionary<string, string> characteristicNames = new Dictionary<string, string>();
    // List<string> characteristicNames = new List<string>();

    public string deviceId = "BluetoothLE#BluetoothLEa0:51:0b:6d:ac:f8-bf:72:09:6b:e8:95";
    public string serviceId = "{5db62101-443a-4599-83cd-af276d691eab}";
    public string Character = "{b9da07a9-85a0-4d4a-8ae2-9d05a973f9ab}";

    // Start is called before the first frame update
    void Start()
    {
        heartBeatText.text = "0";
        stateManager.text = "";
        an_Player = Player.GetComponent<Animator>();
        InvokeRepeating("UpdateRunning", 0, 0.5f);
        InvokeRepeating("UpdateVelocity", 0, 2.0f);
    }

    // Update is called once per frame
    void Update()
    {
        BleApi.ScanStatus status;
        if (isScanningDevices)
        {
            stateManager.text = "Watch Connecting...";
            BleApi.DeviceUpdate res = new BleApi.DeviceUpdate();
            do
            {
                status = BleApi.PollDevice(ref res, false);
                if (status == BleApi.ScanStatus.AVAILABLE)
                {
                    if (res.name == "YongWatch")
                    {
                        BleApi.ScanServices(res.id);
                        Debug.Log("Watch scanned.");
                        isScanningDevices = false;
                        isScanningServices = true;
                        Debug.Log("Scan devices finished");
                    }
                }
                else if (status == BleApi.ScanStatus.FINISHED)
                {
                    isScanningDevices = false;
                    Debug.Log("Scan devices finished");
                    stateManager.text = "Watch not Connected!\n Press Connect Again";
                }
            } while (status == BleApi.ScanStatus.AVAILABLE);
        }
        if (isScanningServices)
        {
            BleApi.Service res = new BleApi.Service();
            do
            {
                status = BleApi.PollService(out res, false);
                if (status == BleApi.ScanStatus.AVAILABLE)
                {
                    if (res.uuid == serviceId)
                    {
                        BleApi.ScanCharacteristics(deviceId, res.uuid);
                        Debug.Log("Service scanned.");
                        isScanningServices = false;
                        isScanningCharacteristics = true;
                    }
                }
                else if (status == BleApi.ScanStatus.FINISHED)
                {
                    isScanningServices = false;
                    Debug.Log("Scan service finished");
                }
            } while (status == BleApi.ScanStatus.AVAILABLE);
        }
        if (isScanningCharacteristics)
        {
            BleApi.Characteristic res = new BleApi.Characteristic();
            do
            {
                status = BleApi.PollCharacteristic(out res, false);
                if (status == BleApi.ScanStatus.AVAILABLE)
                {

                    string name = res.userDescription != "no description available" ? res.userDescription : res.uuid;
                    characteristicNames[name] = res.uuid;

                }
                else if (status == BleApi.ScanStatus.FINISHED)
                {
                    if (characteristicNames.ContainsKey(Character))
                    {
                        isScanningCharacteristics = false;
                        isSubscribed = true;
                        Debug.Log("Scan Characteristics finished");
                        stateManager.text = "Watch Connected!";
                        StartCoroutine(RunningStart());
                    }
                    else
                    {
                        Debug.Log("Scan Characteristics not finished");
                        BleApi.ScanCharacteristics(deviceId, res.uuid);
                        characteristicNames = new Dictionary<string, string>();
                    }

                }
            } while (status == BleApi.ScanStatus.AVAILABLE);

        }
        if (isSubscribed)
        {

            BleApi.BLEData res = new BleApi.BLEData();

            BleApi.SubscribeCharacteristic(deviceId, serviceId, Character, false);
            while (BleApi.PollData(out res, false))
            {
                if (res.characteristicUuid == Character)
                {
                    // subcribeText1.text = Encoding.UTF8.GetString(res.buf, 0, res.size);
                    string[] q12 = Encoding.UTF8.GetString(res.buf, 0, res.size).Split(',');
                    stepCount = int.Parse(q12[0]);
                    Heartbeat = int.Parse(q12[1]);
                    // subcribeText1.text = q12[0];
                    heartBeatText.text = q12[1];
                    // subcribeText2.text = stepCountPast.ToString();
                }
            }
        }

        // an_Player.SetFloat("Velocity", PlayerControl.velocity);
        an_Player.SetFloat("Velocity", velocity);
    }

    IEnumerator RunningStart()
    {
        stateManager.text = "Now\n Run!";
        yield return new WaitForSeconds(3);
        stateManager.text = "";
    }

    void UpdateVelocity()
    {
        if (Mathf.Approximately(stepCount, stepCountPast))
        {
            velocity = 0f;
        }
        else if (stepCount - stepCountPast < 3)
        {
            // velocity += 0.1f;
            // if(velocity > 0.5f)
            velocity = 0.5f;
        }
        else
        {
            // velocity += 0.1f;
            // if(velocity > 1.0f)
            velocity = 1.0f;
        }
    }

    void UpdateRunning()
    {
        stepCountPast = stepCount;
        // Debug.Log(velocity);
    }

    private void OnApplicationQuit()
    {
        BleApi.Quit();
    }

    public void StartStopDeviceScan()
    {
        if (!isScanningDevices)
        {
            BleApi.StartDeviceScan();
            isScanningDevices = true;
        }
        else
        {
            // stop scan
            isScanningDevices = false;
            BleApi.StopDeviceScan();
        }
    }

    public void DisconnectWatch()
    {
        isSubscribed = false;
        stateManager.text = "Watch Disconnected";
    }


}


