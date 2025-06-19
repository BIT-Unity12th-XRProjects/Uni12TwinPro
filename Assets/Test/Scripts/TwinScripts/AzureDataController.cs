using UnityEngine;
using Microsoft.Azure.Devices;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Shared;
using System.Collections.Generic;
using TMPro;

public class IoTMonitor : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _stateText;
    
    private string connectionString =
        "HostName=Uni12TwinPro.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=ts51E0cBODPGFlLbDyoC7pZiHyzbP3wJ/AIoTODruRw=";
    private string targetDeviceId = "TestDevice";
    public float checkInterval = 1.0f;
    public DoorController doorController;

    private RegistryManager registryManager;
    private bool lastButtonState;

    IEnumerator Start()
    {
        // Azure IoT Hub 연결 초기화
        registryManager = RegistryManager.CreateFromConnectionString(connectionString);

        // 코루틴 시작
        yield return StartCoroutine(CheckDeviceTwinRoutine());
    }

    IEnumerator CheckDeviceTwinRoutine()
    {
        while (true)
        {
            Task<Twin> twinTask = registryManager.GetTwinAsync(targetDeviceId);
            
            yield return new WaitUntil(() => twinTask.IsCompleted);

            if (twinTask.Exception != null)
            {
                Debug.LogError($"트윈 조회 실패: {twinTask.Exception}{twinTask.Exception.Message}");
                _stateText.text = $"트윈 조회 실패: {twinTask.Exception}{twinTask.Exception.Message}";
                yield break;
            }

            Twin twin = twinTask.Result;
            ProcessTwinData(twin);

            yield return new WaitForSeconds(checkInterval);
        }
    }

    void ProcessTwinData(Twin twin)
    {
        if (twin.Properties.Reported.Contains("toggleState"))
        {
            object reported = twin.Properties.Reported["toggleState"];
            if (reported != null)
            {
                bool currentState = reported.ToString() == "True" ? true : false;

                if (lastButtonState != currentState)
                {
                    Debug.Log($"{System.DateTime.Now} 상태 변경: {currentState}");
                    _stateText.text = $"{System.DateTime.Now} 상태 변경: {currentState}";
                    lastButtonState = currentState;

                    // Unity 이벤트 시스템과 연동

                    if (currentState == true)
                    {
                        doorController.OpenDoor();
                    }
                    else
                    {
                        doorController.CloseDoor();
                    }
                }
            }
        }
    }

    void OnDestroy()
    {
        if (registryManager != null)
        {
            registryManager.CloseAsync().Wait();
        }
    }
}
