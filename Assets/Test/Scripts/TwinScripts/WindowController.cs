using UnityEngine;

public class WindowController : MonoBehaviour
{
    [Header("중복된 ID를 갖는 Window가 있으면 안됩니다.")]
    [SerializeField] private uint _windowId = 0;

    private Animator  _animator;

    private bool _isOpen = false;
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void OpenWindow()
    {
        if (_isOpen)
        {
            return;
        }

        _isOpen = true;
        _animator.SetTrigger("Open");
    }

    public void CloseWindow()
    {
        if (_isOpen == false)
        {
            return;
        }
        
        _isOpen = false;
        _animator.SetTrigger("Close");
    }
}
