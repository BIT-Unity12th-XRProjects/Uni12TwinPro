using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    private Scene _scene;
    [SerializeField] private string _twinSceneName;
    [SerializeField] private string _arSceneName;
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void OpenTwinScene()
    {
        _scene = SceneManager.GetActiveScene();
        if (_scene.name == _twinSceneName)
        {
            return;
        }
        SceneManager.LoadScene(_twinSceneName);
    }
    
    public void OpenARScene()
    {
        _scene = SceneManager.GetActiveScene();
        if (_scene.name == _arSceneName)
        {
            return;
        }
        SceneManager.LoadScene(_arSceneName);
    }
}
