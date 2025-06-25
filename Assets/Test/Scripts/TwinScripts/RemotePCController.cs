using System.ComponentModel;
using UnityEngine;
using UnityEngine.Rendering;

public class RemotePCController : MonoBehaviour
{
    [SerializeField] private AzureDataController _azureDataController;
    [SerializeField] private MeshRenderer _meshRenderer;
    [Header("중복된 ID를 갖는 PC가 있으면 안됩니다.")]
    [SerializeField] private uint _pcId = 0;

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
        _enablePower = false;
        _meshRenderer.enabled = false;
        _touchDetectManager.OnTouchDetectedEvent += OnClickPCPower;
    }

    private void OnDestroy()
    {
        if (_touchDetectManager != null)
        {
            _touchDetectManager.OnTouchDetectedEvent -= OnClickPCPower;
        }
    }

    public async void OnClickPCPower()
    {
        _enablePower = !_enablePower;
        if (_azureDataController != null) await _azureDataController.SendRemotePCStateAsync(_enablePower, _pcId);

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
