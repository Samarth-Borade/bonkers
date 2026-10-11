using UnityEngine;

// Keeps the whole level on screen at any window size.
[RequireComponent(typeof(Camera))]
public class CameraRig : MonoBehaviour
{
    // World area that must always be visible (extra room at the top for the HUD).
    const float Left = -16.4f, Right = 16.4f, Bottom = -9.2f, Top = 12f;

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Palette.Background;
        transform.position = new Vector3((Left + Right) / 2f, (Bottom + Top) / 2f, -10f);
    }

    void LateUpdate()
    {
        float halfW = (Right - Left) / 2f, halfH = (Top - Bottom) / 2f;
        cam.orthographicSize = Mathf.Max(halfH, halfW / cam.aspect);
    }
}
