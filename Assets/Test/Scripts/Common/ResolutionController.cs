using UnityEngine;

public class ResolutionController : MonoBehaviour
{
    private ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;

    void Start()
    {
        lastOrientation = Screen.orientation;
        ApplyResolutionByOrientation(lastOrientation);
        StartCoroutine(CheckOrientationRoutine());
    }

    private System.Collections.IEnumerator CheckOrientationRoutine()
    {
        while (true)
        {
            // 방향이 바뀌었는지 체크
            if (Screen.orientation != lastOrientation)
            {
                lastOrientation = Screen.orientation;
                ApplyResolutionByOrientation(lastOrientation);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void ApplyResolutionByOrientation(ScreenOrientation orientation)
    {
        if (orientation == ScreenOrientation.LandscapeLeft
            || orientation == ScreenOrientation.LandscapeRight)
        {
            Screen.SetResolution(1920, 1080, true); // 가로
            Camera.main.fieldOfView = 60f; // 가로 FOV
        }
        else if (orientation == ScreenOrientation.Portrait
            || orientation == ScreenOrientation.PortraitUpsideDown)
        {
            Screen.SetResolution(1080, 1920, true); // 세로
            Camera.main.fieldOfView = 120f; // 세로 FOV

        }
    }
}
