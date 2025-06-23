using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private Scene _scene;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private string _twinSceneName;
    [SerializeField] private string _arSceneName;
    [SerializeField] private Button _backButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }

        _backButton.gameObject.SetActive(false);
    }

    public void EnableBackButton()
    {
        _backButton.gameObject.SetActive(true);
    }

    public void DisableBackButton()
    {
        _backButton.gameObject.SetActive(false);
    }

    public void OnBackButtonClicked()
    {
        _cameraController.ReturnToDefaultPosition();
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
