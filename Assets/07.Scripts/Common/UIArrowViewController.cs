using System;
using UnityEngine;

public class UIArrowViewController : MonoBehaviour
{
    private Canvas _canvas;

    void Start()
    {
        _canvas = gameObject.GetComponent<Canvas>();
        transform.forward = Camera.main.transform.forward;

        UIManager.Instance.EnableArrowUIEvent += EnableImage;
        UIManager.Instance.DisableArrowUIEvent += DisableImage;
    }

    public void EnableImage()
    {
        if (_canvas != null)
        {
            _canvas.enabled = true;
        }
    }

    internal void DisableImage()
    {
        if (_canvas != null)
        {
            _canvas.enabled = false;
        }
    }
}
