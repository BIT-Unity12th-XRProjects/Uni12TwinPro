using UnityEngine;
using Microsoft.Azure.Devices;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Shared;

public class IoTMonitor : MonoBehaviour
{
    [Header("Azure IoT 설정")]
    public string connectionString = "HostName=Uni12TwinProTest.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=Ez/vrnK5xfmYrosJCMRRkR7wuIDZGSV/BAIoTNvpoiU=";
    public string targetDeviceId = "azure-samples-test";
    public float checkInterval = 1.0f;

    private RegistryManager registryManager;
    private string lastButtonState;

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
                Debug.LogError($"트윈 조회 실패: {twinTask.Exception.Message}");
                yield break;
            }

            Twin twin = twinTask.Result;
            ProcessTwinData(twin);

            yield return new WaitForSeconds(checkInterval);
        }
    }

    void ProcessTwinData(Twin twin)
    {
        if (twin.Properties.Reported.Contains("buttonState"))
        {
            string currentState = twin.Properties.Reported["buttonState"].ToString();

            if (lastButtonState == null)
            {
                lastButtonState = currentState;
                Debug.Log($"초기 상태: {currentState}");
            }
            else if (lastButtonState != currentState)
            {
                Debug.Log($"{System.DateTime.Now} 상태 변경: {currentState}");
                lastButtonState = currentState;

                // Unity 이벤트 시스템과 연동
                //EventManager.TriggerEvent("ButtonStateChanged", currentState);
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
