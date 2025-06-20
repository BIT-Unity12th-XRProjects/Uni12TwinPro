using System;
using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("위치 설정")]
    [SerializeField] private Transform _defaultLookAtTarget;

    [Header("이동 설정")]
    [SerializeField] private float _moveDuration = 1.5f; // 이동 시간
    [SerializeField] private float _rotationSpeed = 5f;  // 회전 속도

    [SerializeField] private Transform _defaultPosition;
    public void ReturnToDefaultPosition()
    {
        StartCoroutine(C_MoveCamera(Camera.main.transform.position, _defaultPosition.position, _defaultLookAtTarget));
    }
    private IEnumerator C_MoveCamera(Vector3 from, Vector3 end, Transform lookAtTarget)
    {
        Debug.Log($"start : {from} \n end : {end}");
        float elapsedTime = 0f;
        Transform camTransform = Camera.main.transform;
        Quaternion startRotation = camTransform.rotation;

        // 최종 위치에서 타겟 바라보는 회전값 미리 계산
        Vector3 finalDirection = lookAtTarget.position - end;
        Quaternion finalRotation = Quaternion.LookRotation(finalDirection);

        while (elapsedTime < _moveDuration)
        {
            float progress = elapsedTime / _moveDuration;

            // 위치 보간 (이징 적용)
            camTransform.position = Vector3.Lerp(
                from,
                end,
                EaseInOutCubic(progress) // 커스텀 이징 함수
            );

            // 회전 보간
            camTransform.rotation = Quaternion.Slerp(
                startRotation,
                finalRotation,
                progress
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 최종 위치/회전 고정
        camTransform.position = end;
        camTransform.rotation = finalRotation;
    }

    // 부드러운 이동을 위한 이징 함수
    private float EaseInOutCubic(float t)
    {
        return t < 0.5f
            ? 4f * t * t * t
            : 1f - Mathf.Pow(-2f * t + 2f, 3) / 2f;
    }

    public void MoveToPosition(Transform transform)
    {
        StartCoroutine(C_MoveCamera(Camera.main.transform.position, transform.position, transform));
    }
}
