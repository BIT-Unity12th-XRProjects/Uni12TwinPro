using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private Animator _doorAnimator;

    private bool isOpen = false;

    private void Start()
    {
        if (_doorAnimator == null)
        {
            _doorAnimator = GetComponent<Animator>();
        }
    }

    public void OpenDoor()
    {
        if (isOpen == true) return;

        _doorAnimator.SetTrigger("Open");
        isOpen = true;
    }

    public void CloseDoor()
    {
        if (isOpen == false) return;

        _doorAnimator.SetTrigger("Close");
        isOpen = false;
    }
}
