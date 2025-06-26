using System;
using System.ComponentModel;
using Test.Scripts.TwinScripts;
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
        UIManager.Instance.OffPCEvent += OffPC;
    }

    private void OnDisable()
    {
        UIManager.Instance.OffPCEvent -= OffPC;
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
        if (_enablePower)
        {
            UIManager.Instance.ShowPCPanel();
            UIManager.Instance.currentPcId = _pcId;
        }
        else
        {
            _enablePower = true;
            _meshRenderer.enabled = true;
            if (_azureDataController != null)
            {
                await _azureDataController.SendRemotePCStateAsync(_enablePower, _pcId);
            }
        }
    }

    public async void OffPC(uint id)
    {
        if (id == _pcId)
        {
            _meshRenderer.enabled = false;
            _enablePower = false;
            await _azureDataController.SendRemotePCStateAsync(false, _pcId);
        }
    }
}
