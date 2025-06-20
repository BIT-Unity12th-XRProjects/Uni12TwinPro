using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using TransportType = Microsoft.Azure.Devices.Client.TransportType;

namespace Test.Scripts.TwinScripts
{
    public class LEDController : MonoBehaviour
    {
        private DeviceClient _client;
        private RegistryManager  _registryManager;
        private bool _ledState = false;

        private string _connectionString =
            "HostName=Uni12TwinPro.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=ts51E0cBODPGFlLbDyoC7pZiHyzbP3wJ/AIoTODruRw=";
        private string _targetDeviceId = "TestDevice";
        
        private bool _buttonState;

        void Start()
        {
            try
            {
                // var mqttSettings =  new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
                // _client = DeviceClient.CreateFromConnectionString(_connectionString,
                //     new ITransportSettings[]{mqttSettings});
                
                _registryManager =  RegistryManager.CreateFromConnectionString(_connectionString);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
        
        public async void OnClickLEDButton()
        {
            _ledState = !_ledState;
            await SendLEDStateAsync();
        }

        private async Task SendLEDStateAsync()
        {
            Twin twin = await _registryManager.GetTwinAsync(_targetDeviceId);
            twin.Properties.Desired["ledState"] = _ledState;
            Debug.Log($"current led state : {_ledState}");
            
            await _registryManager.UpdateTwinAsync(
                _targetDeviceId,
                twin,
                "*"
                );
        }

        private async void OnDisable()
        {
            if (_client != null)
            {
                await _client.CloseAsync();
            }
        }
    }
}