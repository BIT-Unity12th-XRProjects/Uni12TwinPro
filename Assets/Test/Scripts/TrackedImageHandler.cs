using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ZXing.Unity;

public class TrackedImageHandler : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager _arTrackedImageManager;
    // private Dictionary<TrackableId, GameObject> _placedMarkers = new Dictionary<TrackableId, GameObject>();
 
    private BarcodeReader _barcodeReader;
    private float _scanInterval = 0.2f;            // 0.2초마다 스캔
    private Coroutine _scanRoutine;

    private Dictionary<TrackableId, GameObject> _personPanels = new Dictionary<TrackableId, GameObject>();
    [SerializeField] private GameObject _namePanelPrefab;
    
    private void Awake()
    {
        _barcodeReader = new BarcodeReader { AutoRotate = false, TryInverted = true };
    }

    private void OnEnable()
    {
        _arTrackedImageManager.trackablesChanged.AddListener(OnTrackablesChanged);
    }

    private void OnDisable()
    {
        _arTrackedImageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
        if (_scanRoutine != null)
        {
            StopCoroutine(_scanRoutine);
        }
    }

    private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (ARTrackedImage image in args.added)
        {
            StartScan(image);
           // HandleTrackedImage(image);
        }
        
        foreach (ARTrackedImage image in args.updated)
        {
            StartScan(image);
            // HandleTrackedImage(image);
        }
        
        // foreach (KeyValuePair<TrackableId,ARTrackedImage> imagePair in args.removed)
        // {
        //     
        // }
    }

    private void StartScan(ARTrackedImage tracked)
    {
        if (_scanRoutine != null)
        {
            StopCoroutine(_scanRoutine);
        }

        _scanRoutine = StartCoroutine(ScanLoop(tracked));
    }
    
    
    
    // private void HandleTrackedImage(ARTrackedImage tracked)
    // {
    //     if (_placedMarkers.TryGetValue(tracked.trackableId, out GameObject marker) == false)
    //     {
    //         _placedMarkers.Add(tracked.trackableId,Instantiate(_placePrefab));
    //     }
    //         
    //     _placedMarkers[tracked.trackableId].transform.position = tracked.transform.position;
    //     _placedMarkers[tracked.trackableId].transform.rotation = tracked.transform.rotation;
    //     Debug.Log($"ID: {tracked.trackableId}");
    // }
    //
    
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
    }
    
    IEnumerator ScanLoop(ARTrackedImage tracked)
    {
        Debug.Log("ScanLoop Start");
        while (tracked.trackingState == TrackingState.Tracking)
        {
            
            // 1) Texture2D 얻기 (Reference Image 에셋)
            Texture2D tex = tracked.referenceImage.texture;
            
            if (tex == null)
            {
                Debug.LogWarning("ReferenceImage.texture가 null입니다.");
                yield break;
            }

            // 2) Color32[] 변환
            Color32[] pixels = tex.GetPixels32();

            // 3) 디코드 시도
            var result = _barcodeReader.Decode(pixels, tex.width, tex.height);
            if (result != null)
            {
                Debug.Log($"QR 디코딩 성공: {result.Text}");
                // TODO: UI 표시 or 네트워크 전송
                PlacePanel(tracked, result.Text);
                
                yield break;  // 스캔 종료
            }
            else
            {
                PlacePanel(tracked, "QR 디코딩 실패");
                Debug.Log("QR 디코딩 실패");
            }
            // 실패 시 0.2초 대기 후 재시도
            yield return new WaitForSeconds(_scanInterval);
        }
    }
}
