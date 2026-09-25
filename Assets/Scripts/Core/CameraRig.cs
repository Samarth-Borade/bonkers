using UnityEngine;

// Keeps the whole level on screen at any window size, and shakes on a BONK.
[RequireComponent(typeof(Camera))]
public class CameraRig : MonoBehaviour
{
    public static CameraRig I { get; private set; }

    // World area that must always be visible (extra room at the top for the HUD).
    const float Left = -16.4f, Right = 16.4f, Bottom = -9.2f, Top = 11.2f;

    Camera cam;
    float shakeTime, shakeAmount;

    void Awake()
    {
        I = this;
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Palette.Background;
    }

    void LateUpdate()
    {
        float halfW = (Right - Left) / 2f, halfH = (Top - Bottom) / 2f;
        cam.orthographicSize = Mathf.Max(halfH, halfW / cam.aspect);

        Vector3 pos = new Vector3((Left + Right) / 2f, (Bottom + Top) / 2f, -10f);
        if (shakeTime > 0)
        {
            shakeTime -= Time.deltaTime;
            pos += (Vector3)(Random.insideUnitCircle * shakeAmount * Mathf.Clamp01(shakeTime * 4f));
        }
        else shakeAmount = 0;
        transform.position = pos;
    }

    public static void Shake(float amount, float time)
    {
        if (!I) return;
        I.shakeAmount = Mathf.Max(I.shakeAmount, amount);
        I.shakeTime = Mathf.Max(I.shakeTime, time);
    }
}
