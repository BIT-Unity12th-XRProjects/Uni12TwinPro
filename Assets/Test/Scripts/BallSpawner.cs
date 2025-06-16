using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 탭 치면 공 발사
/// </summary>
public class BallSpawner : MonoBehaviour
{
    [SerializeField] private InputActionReference _tapInputAction;
    [SerializeField] private InputActionReference _tapPositionInputAction;
    [SerializeField] private GameObject _ballPrefab;
    [SerializeField] private Camera _xrCamera;
    [SerializeField] private float _ballSpawnOffsetZ = 0.3f;
    [SerializeField] private float _fireForce = 10f;
    private GameObject _ball;
    private Vector2 _cachedTapPosition;

    private void Start()
    {
        _tapInputAction.action.started += OnTapStarted;
        _tapInputAction.action.canceled += OnTapPositionCanceled;

        _tapPositionInputAction.action.performed += OnTapPositionPerformed;
    }

    void OnTapStarted(InputAction.CallbackContext context)
    {
        Vector3 spawnPos = _xrCamera.transform.position + _xrCamera.transform.forward * _ballSpawnOffsetZ;
        Quaternion spawnRot = _xrCamera.transform.rotation;
        
        _ball = Instantiate(_ballPrefab,spawnPos, spawnRot);
        Rigidbody rigidbody = _ball.GetComponent<Rigidbody>();
        rigidbody.isKinematic = true;
    }

    void OnTapPositionPerformed(InputAction.CallbackContext context)
    {
        _cachedTapPosition = context.ReadValue<Vector2>();
        PlaceBall();        
    }

    void PlaceBall()
    {
        if (_ball == null)
        {
            return;
        }
        
        Ray ray = _xrCamera.ScreenPointToRay(_cachedTapPosition);

        Vector3 planeNormal = _xrCamera.transform.forward;
        Plane plane = new Plane(planeNormal, _ballSpawnOffsetZ);

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            _ball.transform.position = hitPoint;
            _ball.transform.rotation = Quaternion.LookRotation(ray.direction);
        }
    }

    void OnTapPositionCanceled(InputAction.CallbackContext context)
    {
        Rigidbody rigidbody = _ball.GetComponent<Rigidbody>();
        rigidbody.isKinematic = false;
        rigidbody.AddForce(_xrCamera.transform.forward * _fireForce, ForceMode.Impulse);
        _ball = null;
    }
}