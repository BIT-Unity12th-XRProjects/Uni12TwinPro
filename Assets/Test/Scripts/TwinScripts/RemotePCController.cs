using UnityEngine;
using UnityEngine.Rendering;

public class RemotePCController : MonoBehaviour
{
    [SerializeField] private AzureDataController _azureDataController;
    [SerializeField] private MeshRenderer _meshRenderer;

    private TouchDetectManager _touchDetectManager;
    private bool _enablePower = false;

    private void Start()
    {
        if (_azureDataController == null)
        {
            _azureDataController = FindAnyObjectByType<AzureDataController>();
        }

        if (_touchDetectManager == null)
        {
            _touchDetectManager = GetComponent<TouchDetectManager>();
        }

        _touchDetectManager.OnTouchDetectedEvent += OnClickPCPower;

        _enablePower = false;
        _meshRenderer.enabled = false;
    }

    public async void OnClickPCPower()
    {
        _enablePower = !_enablePower;
        if (_azureDataController != null) await _azureDataController.SendLEDStateAsync(_enablePower);

        if (_enablePower)
        {
            Debug.Log("PC 전원 켜기");
            _meshRenderer.enabled = true;
        }
        else
        {
            Debug.Log("PC 전원 끄기");
            _meshRenderer.enabled = false;
        }
    }
}
