using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTrackingOCR : MonoBehaviour
{
    private string ocrInvokeUrl =
        "https://ihkt8x14s5.apigw.ntruss.com/custom/v1/42961/4d97a29ae6ac8b38f41ff0314cbeea521e8ddd96bee84adffe1bcb7510ad4e12/general";
    private string ocrSecretKey = "VHFlaU9SRGdUR0pxSUNxc2JWZXZraXhFUUttbEhGa0s=";

    [SerializeField] private ARTrackedImageManager _arTrackedImageManager;
    
    private bool isCapturing = false;
    
    private StudentInfo _studentInfo;

    private void Awake()
    {
        _studentInfo = new();
    }

    void OnEnable()
    {
        // _arTrackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    void OnDisable()
    {
        // _arTrackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
        StopAllCoroutines();
    }

    public void StartCapture()
    {
        StartCoroutine(ScreenOcrLoop());
    }
    private IEnumerator ScreenOcrLoop()
    {
        if (isCapturing)
        {
            yield break;
        }
        
        isCapturing = true;
        yield return new WaitForEndOfFrame();

        Texture2D screenTex = ScreenCapture.CaptureScreenshotAsTexture();
        // int captureWidth = 200;
        // int captureHeight = 200;
        // int startX = (Screen.width - captureWidth) / 2; // 화면 중앙 X 좌표
        // int startY = (Screen.height - captureHeight) / 2; // 화면 중앙 Y 좌표
        //
        // screenTex.ReadPixels(new Rect(startX, startY, captureWidth, captureHeight), 0, 0);
        // screenTex.Apply();
        //
        byte[] jpgBytes = screenTex.EncodeToJPG(90);
        Destroy(screenTex);

        StartCoroutine(CallClovaOCR_General(jpgBytes, screenResult => {
            // string screenResultRemoveENG = RemoveEnglish(screenResult);
            // Debug.Log($"OCR Captured Image: {screenResultRemoveENG}");
            if (IsContainStudentName(screenResult, out StudentInfoData student))
            {
                PlacePanel(student);
            }
        }));
    }

    private bool IsContainStudentName(string screenResult, out StudentInfoData info)
    {
        int cnt = 0;
        int studentIndex = -1;
        for (int i = 0; i < _studentInfo.Infos.Count; i++)
        {
            if (screenResult.Contains(_studentInfo.Infos[i].name))
            {
                cnt++;
                studentIndex = i;
                if (cnt >= 2)
                {
                    info = null;
                    return false;
                }
            }
        }

        if (cnt == 1)
        {
            info = _studentInfo.Infos[studentIndex];
            return true;
        }
        
        info = null;
        return false;
    }

    // private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    // {
    //     foreach (ARTrackedImage img in args.added)
    //     {
    //         StartCoroutine(ScreenOcrLoop(img));
    //     }
    //
    //     foreach (ARTrackedImage img in args.updated)
    //     {
    //         StartCoroutine(ScreenOcrLoop(img));
    //     }
    // }

    private IEnumerator CallClovaOCR_General(byte[] img, Action<string> onComplete)
    {
        string base64 = Convert.ToBase64String(img);
        var req = new OcrRequest {
            images    = new[]{ new ImageData{ name = "", format = "jpg", data = base64 } },
            lang      = "ko",
            requestId = Guid.NewGuid().ToString(),
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            version   = "V2"
        };
        string json = JsonUtility.ToJson(req);

        using var uwr = new UnityWebRequest(ocrInvokeUrl, "POST")
        {
            uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        uwr.SetRequestHeader("Content-Type", "application/json");
        uwr.SetRequestHeader("X-OCR-SECRET", ocrSecretKey);

        yield return uwr.SendWebRequest();
        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"OCR 실패: {uwr.error}");
            onComplete?.Invoke(string.Empty);
            isCapturing = false;
            yield break;
        }

        var resp = JsonUtility.FromJson<CLOVAResponse>(uwr.downloadHandler.text);
        var sb = new StringBuilder();
        foreach (var imgRes in resp.images)
            foreach (var field in imgRes.fields)
                sb.Append(field.inferText).Append(' ');

        onComplete?.Invoke(sb.ToString().Trim());
        isCapturing = false;
    }
    
    /// <summary>
    /// 입력 문자열에서 영문자(A–Z, a–z)만 제거하고 반환합니다.
    /// </summary>
    public static string RemoveEnglish(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var sb = new StringBuilder(input.Length);
        foreach (char c in input)
        {
            // 영문자 범위가 아니면 추가
            if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')))
                sb.Append(c);
        }
        return sb.ToString();
    }


    [SerializeField] private GameObject namePanelPrefab;
    private Dictionary<string, GameObject> panels = new Dictionary<string, GameObject>();

    private void PlacePanel(StudentInfoData info)
    {
        if (!panels.TryGetValue(info.name, out var panel))
        {
            panel = Instantiate(namePanelPrefab);
            panel.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.2f;
            panel.transform.rotation = Camera.main.transform.rotation;
            panels[info.name] = panel;
            panel.GetComponent<PersonMarkerController>().SetInfo(info);
            UIManager.Instance.ShowTrackedNameText();
        }
    }

    #region DTO Classes
    [Serializable] private class OcrRequest
    {
        public ImageData[] images;
        public string       lang;
        public string       requestId;
        public long         timestamp;
        public string       version;
    }
    [Serializable] private class ImageData { public string name; public string format; public string data; }
    [Serializable] private class CLOVAResponse { public ImageResult[] images; }
    [Serializable] private class ImageResult  { public Field[] fields; }
    [Serializable] private class Field        { public string inferText; }
    #endregion

}
