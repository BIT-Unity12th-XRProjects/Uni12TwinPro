using UnityEngine;
using UnityEngine.EventSystems;

public class TouchDetectManager : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Transform LookAtObjectPosition;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"{name} 오브젝트가 클릭 또는 터치되었습니다!");

        cameraController.MoveToPosition(LookAtObjectPosition);
    }
}
