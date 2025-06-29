using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchDetectManager : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Transform _cameraMovePosition;
    [SerializeField] private Transform _lookAtTarget;

    private CameraController _cameraController;

    public event Action OnTouchDetectedEvent;

    private void Start()
    {
        _cameraController = Camera.main.GetComponent<CameraController>();

        if (_cameraMovePosition == null)
        {
            _cameraMovePosition = transform;
        }

        if (_lookAtTarget == null)
        {
            _lookAtTarget = transform;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"{name} 오브젝트가 클릭 또는 터치되었습니다!");

        if (_cameraController.CurrentState == CameraController.MovementType.ToDefault)
        {
            _cameraController.MoveToPosition(_cameraMovePosition, _lookAtTarget);
        }
        else if (_cameraController.CurrentState == CameraController.MovementType.ToTarget)
        {
            OnTouchDetectedEvent?.Invoke();
        }
    }
}
