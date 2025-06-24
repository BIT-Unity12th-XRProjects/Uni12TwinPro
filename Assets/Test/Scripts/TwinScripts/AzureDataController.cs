using UnityEngine;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Shared;
using System.Collections.Generic;
using TMPro;
using Test.Scripts.TwinScripts;
using System;

public class AzureDataController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _stateText;
    [SerializeField] private DoorController _doorController;

    private RegistryManager _registryManager;
    private bool _lastButtonState;

    private string _connectionSendString =
        "HostName=Uni12TwinPro.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=ts51E0cBODPGFlLbDyoC7pZiHyzbP3wJ/AIoTODruRw=";
    private string _targetDeviceId = "TestDevice";

    public float CheckInterval = 1.0f;
    IEnumerator Start()
    {
        // Azure IoT Hub Send 연결 초기화
        _registryManager = RegistryManager.CreateFromConnectionString(_connectionSendString);

        // IoT Hub에서 메시지 수신
        yield return StartCoroutine(CheckDoorCondition());
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
                _stateText.text = $"트윈 조회 실패: {twinTask.Exception}{twinTask.Exception.Message}";
                yield break;
            }

            Twin twin = twinTask.Result;
            ProcessTwinData(twin);

            yield return new WaitForSeconds(CheckInterval);
        }
    }

    void ProcessTwinData(Twin twin)
    {
        // 토글 스위치 상태 확인
        if (twin.Properties.Reported.Contains("toggleState"))
        {
            object reported = twin.Properties.Reported["toggleState"];
            if (reported != null)
            {
                bool currentState = reported.ToString() == "True" ? true : false;

                if (_lastButtonState != currentState)
                {
                    Debug.Log($"{System.DateTime.Now} 상태 변경: {currentState}");
                    _stateText.text = $"{System.DateTime.Now} 상태 변경: {currentState}";
                    _lastButtonState = currentState;

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
    }

    public async Task SendLEDStateAsync(bool ledState)
    {
        Twin twin = await _registryManager.GetTwinAsync(_targetDeviceId);
        twin.Properties.Desired["ledState"] = ledState;
        Debug.Log($"current led state : {ledState}");

        await _registryManager.UpdateTwinAsync(
            _targetDeviceId,
            twin,
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
