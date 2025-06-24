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
        _arTrackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
        // screenOcrRoutine = StartCoroutine(ScreenOcrLoop());
    }

    void OnDisable()
    {
        _arTrackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
        StopAllCoroutines();
    }

    private IEnumerator ScreenOcrLoop(ARTrackedImage image)
    {
        if (isCapturing)
        {
            yield break;
        }

        isCapturing = true;
        yield return new WaitForEndOfFrame();

        Texture2D screenTex = ScreenCapture.CaptureScreenshotAsTexture();
        byte[] jpgBytes = screenTex.EncodeToJPG(90);
        Destroy(screenTex);

        StartCoroutine(CallClovaOCR_General(jpgBytes, screenResult => {
            foreach (var  studentInfo in _studentInfo.Infos)
            {
                if (studentInfo.name == screenResult)
                {
                    PlacePanel(image, studentInfo);
                }
            }
        }));

    }

    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (ARTrackedImage img in args.added)
        {
            StartCoroutine(ScreenOcrLoop(img));
        }

        foreach (ARTrackedImage img in args.updated)
        {
            StartCoroutine(ScreenOcrLoop(img));
        }
    }

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

    [SerializeField] private GameObject namePanelPrefab;
    private Dictionary<TrackableId, GameObject> panels = new Dictionary<TrackableId, GameObject>();

    private void PlacePanel(ARTrackedImage image, StudentInfoData info)
    {
        if (!panels.TryGetValue(image.trackableId, out var panel))
        {
            panel = Instantiate(namePanelPrefab);
            panels[image.trackableId] = panel;
        }
        panel.transform.position = image.transform.position;
        panel.transform.rotation = Camera.main.transform.rotation;
        panel.GetComponent<PersonMarkerController>().SetInfo(info);
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

    // ─── 10. 확장: ICache 인터페이스 도입 후 MemoryCache/FileCache 교체 가능 ───
}
