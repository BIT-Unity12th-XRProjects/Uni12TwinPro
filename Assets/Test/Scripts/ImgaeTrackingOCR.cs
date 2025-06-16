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
    /* ▼ 1. 기존 Client ID/Secret → 도메인 방식 값으로 교체 */
    [Header("OCR Domain Credentials (General)")] [Tooltip("도메인 Invoke URL (https://.../general/v1/...)")]
    public string ocrInvokeUrl =
        "https://ihkt8x14s5.apigw.ntruss.com/custom/v1/42961/4d97a29ae6ac8b38f41ff0314cbeea521e8ddd96bee84adffe1bcb7510ad4e12/general";

    [Tooltip("도메인 Secret Key")] public string ocrSecretKey = "VHFlaU9SRGdUR0pxSUNxc2JWZXZraXhFUUttbEhGa0s=";

    /* ▼ 2. 세션 캐시 선언 (static) */
    private static Dictionary<string, string> ocrCache = new Dictionary<string, string>();

    /* ▼ 3. AR Foundation */
    [SerializeField] private ARTrackedImageManager _trackedImageManager;
    [SerializeField] private GameObject _namePanelPrefab;
    
    private Dictionary<TrackableId, GameObject> _personPanels = new Dictionary<TrackableId, GameObject>();
    void OnEnable()
    {
        _trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    void OnDisable()
    {
        _trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }

    void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (var img in args.added)
        {
            ProcessTrackedImage(img);
        }

        foreach (var img in args.updated)
        {
            ProcessTrackedImage(img);
        }
    }

    void PlacePanel(ARTrackedImage image, string name)
    {
        if (_personPanels.TryGetValue(image.trackableId, out GameObject panel) == false)
        {
            GameObject newPanel = Instantiate(_namePanelPrefab);
            newPanel.GetComponent<PersonMarkerController>().SetName(name);
            _personPanels.Add(image.trackableId, newPanel);
        }
        panel.transform.position = image.transform.position;
        panel.GetComponent<PersonMarkerController>().SetName(name);
        // panel.transform.rotation = image.transform.rotation;
    }
    
    void ProcessTrackedImage(ARTrackedImage tracked)
    {
        if (tracked.trackingState != TrackingState.Tracking) return;

        // ─── 4. 캐시 키 결정 및 조회 ───────────────────────────
        string imageKey = tracked.referenceImage.name;
        if (imageKey == null)
        {
            Debug.Log("imageKey is null");
            return;
        }
        
        if (ocrCache.TryGetValue(imageKey, out var cachedText))
        {
            Debug.Log($"[Cache Hit] 인식된 이름: {cachedText}");
            // TODO: cachedText를 UI나 로직에 즉시 사용
            PlacePanel(tracked, cachedText);
            return;
        }
        else
        {
            Debug.Log($"[Cache Miss] {imageKey}");
        }

        Texture2D tex = tracked.referenceImage.texture;
        if (tex == null)
        {
            Debug.Log("tex is null");
            return;
        }

        byte[] jpgBytes = tex.EncodeToJPG(90);

        // ─── 5. 캐시에 없으면 OCR 호출 ─────────────────────────
        StartCoroutine(CallClovaOCR_General(jpgBytes, result =>
        {
            Debug.Log("인식된 이름: " + result);
            // ─── 6. 호출 완료 시 캐시에 저장 ───────────────────
            ocrCache[imageKey] = result;
            // TODO: result를 UI나 다른 로직에 전달
            PlacePanel(tracked, result);
            
        }));
    }

    IEnumerator CallClovaOCR_General(byte[] img, Action<string> onComplete)
    {
        Debug.Log("Start Call ClovaOCR_General");
        string base64 = Convert.ToBase64String(img);
        
        // 1) DTO 객체 생성
        var req = new OcrRequest {
            images     = new[] {
                new ImageData
                {
                    name = "trackedName",
                    format = "jpg",
                    data = base64
                }
            },
            lang       = "ko",
            requestId  = Guid.NewGuid().ToString(),
            timestamp  = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            version    = "V2"
        };

        // 2) JSON 문자열로 변환
        string json = JsonUtility.ToJson(req);
        Debug.Log("페이로드 JSON: " + json);  // 디버그로 꼭 확인

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
            yield break;
        }

        var resp = JsonUtility.FromJson<CLOVAResponse>(uwr.downloadHandler.text);
        var sb = new StringBuilder();
        foreach (var imgRes in resp.images)
            foreach (var field in imgRes.fields)
                sb.Append(field.inferText).Append(' ');

        onComplete?.Invoke(sb.ToString().Trim());
    }

    #region OCR Response DTO
    [Serializable]
    public class OcrRequest
    {
        public ImageData[] images;
        public string         lang;
        public string         requestId;
        public long           timestamp;
        public string         version;
    }

    [Serializable]
    public class ImageData
    {
        public string name; 
        public string format;
        public string data;
    }

    [Serializable] public class CLOVAResponse { public ImageResult[] images; }
    [Serializable] public class ImageResult  { public Field[] fields; }
    [Serializable] public class Field        { public string inferText; }
    #endregion

    // ─── 8. 향후 확장: ICache 인터페이스 도입 후
    //       MemoryCache / FileCache / PlayerPrefsCache 구현으로 교체 가능 ─────────────────
}
