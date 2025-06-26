using System;
 using System.Collections;
 using System.Collections.Generic;
 using System.Text;
 using TMPro;
 using UnityEngine;
 using UnityEngine.Networking;
 using UnityEngine.XR.ARFoundation;
 using UnityEngine.XR.ARSubsystems;

 public class ImageTrackingOCR : MonoBehaviour
 {
     [SerializeField] private TextMeshProUGUI _ocrText;
     private string ocrInvokeUrl =
         "https://ihkt8x14s5.apigw.ntruss.com/custom/v1/42961/4d97a29ae6ac8b38f41ff0314cbeea521e8ddd96bee84adffe1bcb7510ad4e12/general";
     private string ocrSecretKey = "VHFlaU9SRGdUR0pxSUNxc2JWZXZraXhFUUttbEhGa0s=";
 
     private StudentInfo _studentInfo;

     private Coroutine _captureCoroutine;
     private void Awake()
     {
         _studentInfo = new();
     }
 
     void OnDisable()
     {
         StopAllCoroutines();
     }
 
     public void StartCapture()
     {
         _ocrText.text = "";
         if (_captureCoroutine != null)
         {
             StopCoroutine(_captureCoroutine);
         }
         Debug.Log("Capture started - OCR Start");
         _captureCoroutine = StartCoroutine(ScreenOcrLoop());
     }
     
     private IEnumerator ScreenOcrLoop()
     {
         yield return new WaitForEndOfFrame();
 
         Texture2D screenTex = ScreenCapture.CaptureScreenshotAsTexture();
         
         byte[] jpgBytes = screenTex.EncodeToJPG(90);
         Destroy(screenTex);
 
         StartCoroutine(CallClovaOCR_General(jpgBytes, screenResult => {
             Debug.Log($"OCR Success: {screenResult}");
             _ocrText.text = $"OCR Text: {screenResult}";
             if (IsContainStudentName(screenResult, out StudentInfoData student))
             {
                 Debug.Log($"Find -> Name: {student.name}, Birth: {student.birth}");
                 PlacePanel(student);
             }
         }));
     }
 
     private bool IsContainStudentName(string ocrString, out StudentInfoData info)
     {
         int cnt = 0;
         int studentIndex = -1;
         for (int i = 0; i < _studentInfo.Infos.Count; i++)
         {
             if (ocrString.Contains(_studentInfo.Infos[i].name))
             {
                 cnt++;
                 studentIndex = i;
                 if (cnt >= 2)
                 {
                     info = null;
                     Debug.LogWarning("Student Name Count Over");
                     return false;
                 }
             }
         }
 
         if (cnt == 1)
         {
             info = _studentInfo.Infos[studentIndex];
             return true;
         }
         
         Debug.LogWarning("Student Name Count Zero");
         info = null;
         return false;
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
             yield break;
         }
 
         var resp = JsonUtility.FromJson<CLOVAResponse>(uwr.downloadHandler.text);
         var sb = new StringBuilder();
         foreach (var imgRes in resp.images)
             foreach (var field in imgRes.fields)
                 sb.Append(field.inferText).Append(' ');
 
         onComplete?.Invoke(sb.ToString().Trim());
     }

     [SerializeField] private GameObject namePanelPrefab;
     private Dictionary<string, GameObject> panels = new Dictionary<string, GameObject>();
 
     private void PlacePanel(StudentInfoData info)
     {
         if (panels.ContainsKey(info.name))
             return;

         // 1) 카메라 앞 위치·회전
         Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward * 0.2f;
         Quaternion rot = Quaternion.LookRotation(Camera.main.transform.forward);

         // 2) 빈 GameObject 만들고 ARAnchor 추가
         var anchorGO = new GameObject($"Anchor_{info.name}");
         anchorGO.transform.SetPositionAndRotation(pos, rot);
         var anchor = anchorGO.AddComponent<ARAnchor>(); 
         // ↳ Awake()/OnEnable() 시 내부적으로 Subsystem에 등록됩니다.

         // 3) TrackingState 확인(Optional)
         if (anchor.trackingState != TrackingState.Tracking)
         {
             Debug.LogWarning("앵커가 아직 추적 중이 아님");
             // 필요하다면 기다리거나 fallback 처리
         }

         // 4) 앵커 자식으로 패널 붙이기
         var panel = Instantiate(namePanelPrefab, anchor.transform);
         panel.transform.localPosition = Vector3.zero;
         panel.transform.localRotation = Quaternion.identity;
         panel.GetComponent<PersonMarkerController>().SetInfo(info);

         panels[info.name] = panel;
         UIManager.Instance.ShowTrackedNameText();
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