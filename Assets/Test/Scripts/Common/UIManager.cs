using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum SceneState
{
    Twin,
    AR,
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private Scene _scene;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private string _twinSceneName;
    [SerializeField] private string _arSceneName;
    [SerializeField] private Button _backButton;

    private SceneState _sceneState =  SceneState.Twin;

    [SerializeField] private TextMeshProUGUI _trackedNameText;
    
    public event Action EnableArrowUIEvent;
    public event Action DisableArrowUIEvent;

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

    public void ChangeScene()
    {
        if (_sceneState == SceneState.AR)
        {
            SceneManager.LoadScene(_twinSceneName);
            _sceneState = SceneState.Twin;
        }
        else if (_sceneState == SceneState.Twin)
        {
            SceneManager.LoadScene(_arSceneName);
            _sceneState = SceneState.AR;
        }
    }

    public void ShowTrackedNameText()
    {
        _trackedNameText.enabled = true;
        Invoke("HideTrackedNameText", 0.5f);
    }

    public void HideTrackedNameText()
    {
        _trackedNameText.enabled = false;
    }
    
    public void EnableArrowUI()
    {
        EnableArrowUIEvent?.Invoke();
    }

    public void DisableArrowUI()
    {
        DisableArrowUIEvent?.Invoke();
    }
}
