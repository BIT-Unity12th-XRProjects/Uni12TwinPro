using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Test.Scripts.TwinScripts
{
    public class LEDController : MonoBehaviour
    {
        [SerializeField] private AzureDataController _azureDataController;
        
        private RegistryManager  _registryManager;
        private bool _ledState = false;

        
        public async void OnClickLEDButton()
        {
            _ledState = !_ledState;
            await _azureDataController.SendLEDStateAsync(_ledState);
        }

    }
}