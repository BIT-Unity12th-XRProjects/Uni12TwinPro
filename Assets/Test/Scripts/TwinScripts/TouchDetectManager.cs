using UnityEngine;
using UnityEngine.EventSystems;

public class TouchDetectManager : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Transform _cameraMovePosition;
    [SerializeField] private Transform _lookAtTarget;

    private CameraController _cameraController;

    private void Start()
    {
        if (_cameraController == null)
        {
            _cameraController = Camera.main.GetComponent<CameraController>();
        }

        if (_cameraMovePosition == null)
        {
            _cameraMovePosition = transform;
        }

        if (_lookAtTarget == null)
        {
            _lookAtTarget = transform;
        }

        _cameraController.OnCameraMovementToTargetComplete += UIManager.Instance.EnableBackButton;
        _cameraController.OnCameraMovementToDefaultComplete += UIManager.Instance.DisableBackButton;
    }

    private void OnDestroy()
    {
        if (_cameraController != null)
        {
            _cameraController.OnCameraMovementToTargetComplete -= UIManager.Instance.EnableBackButton;
            _cameraController.OnCameraMovementToDefaultComplete -= UIManager.Instance.DisableBackButton;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"{name} 오브젝트가 클릭 또는 터치되었습니다!");

        _cameraController.MoveToPosition(_cameraMovePosition, _lookAtTarget);
    }
}
