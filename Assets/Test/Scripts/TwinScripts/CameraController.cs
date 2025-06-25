using System;
using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public enum MovementType
    {
        ToTarget,
        ToDefault
    }

    [Header("위치 설정")]
    [SerializeField] private Transform _defaultLookAtTarget;

    [Header("이동 설정")]
    [SerializeField] private float _moveDuration = 1.5f; // 이동 시간
    [SerializeField] private float _rotationSpeed = 5f;  // 회전 속도

    [SerializeField] private Transform _defaultPosition;

    public event Action OnCameraMovementToTargetComplete;
    public event Action OnCameraMovementToDefaultComplete;

    public MovementType CurrentState { get; private set; } = MovementType.ToDefault;

    public void ReturnToDefaultPosition()
    {
        if (_defaultPosition == null)
        {
            Debug.LogError("Default position is not assigned!");
            return;
        }
        UIManager.Instance.DisableBackButton();
        StopAllCoroutines();
        StartCoroutine(C_MoveCamera(Camera.main.transform.position, _defaultPosition.position, _defaultLookAtTarget, MovementType.ToDefault));
    }

    public void MoveToPosition(Transform transform, Transform lookAtTarget = null)
    {
        if (lookAtTarget == null) lookAtTarget = transform;
        UIManager.Instance.DisableArrowUI();

        StopAllCoroutines();
        StartCoroutine(C_MoveCamera(Camera.main.transform.position, transform.position, lookAtTarget, MovementType.ToTarget));
    }

    private IEnumerator C_MoveCamera(Vector3 startPos, Vector3 endPos, Transform lookAtTarget, MovementType type)
    {
        float elapsed = 0f;
        Quaternion startRot = Camera.main.transform.rotation;
        Vector3 lookDirection = lookAtTarget.position - endPos;
        Quaternion endRot = Quaternion.LookRotation(lookDirection);

        while (elapsed < _moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _moveDuration);
            Camera.main.transform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));
            Camera.main.transform.rotation = Quaternion.Slerp(startRot, endRot, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        Camera.main.transform.position = endPos;
        Camera.main.transform.rotation = endRot;

        // 이동 유형에 따라 다른 이벤트 발생
        switch (type)
        {
            case MovementType.ToTarget:
                UIManager.Instance.EnableBackButton();
                CurrentState = MovementType.ToTarget;
                break;
            case MovementType.ToDefault:
                UIManager.Instance.EnableArrowUI();
                CurrentState = MovementType.ToDefault;
                break;
        }
    }
}
