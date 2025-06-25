using UnityEngine;
using Microsoft.Azure.Devices;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Shared;
using TMPro;

public class AzureDataController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _stateText;
    [SerializeField] private DoorController _doorController;
    [SerializeField] private WindowController _windowController;
    [SerializeField] private WindowController _windowController2;

    private RegistryManager _registryManager;
    private bool _lastDoorState;
    private bool _lastWindowState;
    private bool _lastWindowState2;

    private string _connectionRegistryString =
        "HostName=Uni12TwinPro.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=ts51E0cBODPGFlLbDyoC7pZiHyzbP3wJ/AIoTODruRw=";
    private string _targetDeviceId = "TestDevice";

    public float CheckInterval = 1.0f;
    IEnumerator Start()
    {
        // Azure IoT Hub Send 연결 초기화
        _registryManager = RegistryManager.CreateFromConnectionString(_connectionRegistryString);

        // IoT Hub에서 메시지 수신
        yield return StartCoroutine(CheckDoorCondition());
        // yield return StartCoroutine(CheckWindowCondition());
    }

    IEnumerator CheckDoorCondition()
    {
        while (true)
        {
            Task<Twin> twinTask = _registryManager.GetTwinAsync(_targetDeviceId);

            yield return new WaitUntil(() => twinTask.IsCompleted);

            if (twinTask.Exception != null)
            {
                Debug.LogError($"트윈 조회 실패: {twinTask.Exception}{twinTask.Exception.Message}");
                yield break;
            }

            Twin twin = twinTask.Result;
            ProcessTwinData(twin);

            yield return new WaitForSeconds(CheckInterval);
        }
    }

    // IEnumerator CheckWindowCondition()
    // {
    //     while (true)
    //     {
    //         Task<Twin> twinTask = _registryManager.GetTwinAsync(_targetDeviceId);
    //
    //         yield return new WaitUntil(() => twinTask.IsCompleted);
    //
    //         if (twinTask.Exception != null)
    //         {
    //             Debug.LogError($"트윈 조회 실패: {twinTask.Exception}{twinTask.Exception.Message}");
    //             _stateText.text = $"트윈 조회 실패: {twinTask.Exception}{twinTask.Exception.Message}";
    //             yield break;
    //         }
    //
    //         Twin twin = twinTask.Result;
    //         ProcessWindowTwinData(twin);
    //
    //         yield return new WaitForSeconds(CheckInterval);
    //     }
    // }

    void ProcessTwinData(Twin twin)
    {
        // Door State Apply
        if (twin.Properties.Reported.Contains("doorState"))
        {
            object reported = twin.Properties.Reported["doorState"];
            if (reported != null)
            {
                bool currentState = reported.ToString() == "True" ? true : false;

                if (_lastDoorState != currentState)
                {
                    _lastDoorState = currentState;

                    // Unity 이벤트 시스템과 연동
                    if (currentState == true)
                    {
                        _doorController.OpenDoor();
                    }
                    else
                    {
                        _doorController.CloseDoor();
                    }
                }
            }
        }

        // Window State Apply
        if (twin.Properties.Reported.Contains("windowState"))
        {
            object reported = twin.Properties.Reported["windowState"];
            if (reported != null)
            {
                bool currentState = reported.ToString() == "True" ? true : false;

                if (_lastWindowState != currentState)
                {
                    _lastWindowState = currentState;

                    if (currentState == true)
                    {
                        _windowController.OpenWindow();
                    }
                    else
                    {
                        _windowController.CloseWindow();
                    }
                }
            }
        }
        if (twin.Properties.Reported.Contains("windowState2"))
        {
            object reported = twin.Properties.Reported["windowState2"];
            if (reported != null)
            {
                bool currentState = reported.ToString() == "True" ? true : false;

                if (_lastWindowState2 != currentState)
                {
                    _lastWindowState2 = currentState;

                    if (currentState == true)
                    {
                        _windowController2.OpenWindow();
                    }
                    else
                    {
                        _windowController2.CloseWindow();
                    }
                }
            }
        }
    }
    
    // void ProcessWindowTwinData(Twin twin)
    // {
    //     // 토글 스위치 상태 확인
    //     if (twin.Properties.Reported.Contains("windowState"))
    //     {
    //         object reported = twin.Properties.Reported["windowState"];
    //         if (reported != null)
    //         {
    //             bool currentState = reported.ToString() == "True" ? true : false;
    //
    //             if (_lastDoorState != currentState)
    //             {
    //                 _lastDoorState = currentState;
    //
    //                 // Unity 이벤트 시스템과 연동
    //                 if (currentState == true)
    //                 {
    //                     _windowController.OpenWindow();
    //                 }
    //                 else
    //                 {
    //                     _windowController.CloseWindow();
    //                 }
    //             }
    //         }
    //     }
    // }

    //public async Task SendLEDStateAsync(bool ledState)
    //{
    //    Debug.Log($"current led state : {ledState}");
        
    //    string patch = 
    //    $"{{\"properties\":{{\"desired\":{{\"ledState\":{ledState.ToString().ToLowerInvariant()}}}}}}}";

    //    await _registryManager.UpdateTwinAsync(
    //        _targetDeviceId,
    //        patch,
    //        "*"
    //    );
    //}

    public async Task SendRemotePCStateAsync(bool remotePCState, uint pcId)
    {
        Debug.Log($"current remote pc state : {remotePCState}");

        string patch =
        $"{{\"properties\":{{\"desired\":{{\"remotePCState{pcId}\":{remotePCState.ToString().ToLowerInvariant()}}}}}}}";

        await _registryManager.UpdateTwinAsync(
            _targetDeviceId,
            patch,
            "*"
        );
    }

    void OnDestroy()
    {
        if (_registryManager != null)
        {
            _registryManager.CloseAsync().Wait();
        }
    }
}
