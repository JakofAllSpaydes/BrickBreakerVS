using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera cam;
    public float targetFOV = 60f; // Target FOV for zoomed in
    public float defaultFOV = 90f; // Default FOV for zoomed out
    public Vector3 targetPositionOffset = new Vector3(0, -10, 0); // Target position offset for zoomed in
    private Vector3 defaultPosition; // Default position for zoomed out
    private bool isZooming = false;
    private float zoomSpeed = 1f; // Speed of zoom in/out effect

    public float shakeDuration = 0f; // Current duration of the shake effect
    public float shakeIntensity = 0f; // Current intensity of the shake effect
    public float maxShakeDuration = 0.5f; // Maximum duration of the shake effect
    public float maxShakeIntensity = 1f; // Maximum intensity of the shake effect
    private float currentShakeIntensity = 0f; // The starting intensity of the current shake effect

    private void Start()
    {
        if (!cam)
        {
            cam = GetComponent<Camera>();
        }
        defaultPosition = transform.position;
        cam.fieldOfView = defaultFOV;
    }

    void FixedUpdate()
    {
        if (isZooming)
        {
            ZoomIn();
        }
        else
        {
            ZoomOut();
        }

        // Apply screen shake if needed
        if (shakeDuration > 0)
        {
            Vector3 shakeAmount = Random.insideUnitSphere * shakeIntensity;
            cam.transform.localPosition = defaultPosition + shakeAmount;
            shakeDuration -= Time.fixedDeltaTime;

            // Gradually reduce the shake intensity if the duration is nearing its end
            shakeIntensity = Mathf.Lerp(currentShakeIntensity, 0f, 1 - (shakeDuration / maxShakeDuration));
        }
        else
        {
            // Reset shake parameters when the shake effect ends
            shakeDuration = 0f;
            shakeIntensity = 0f;
            cam.transform.localPosition = defaultPosition;
        }
    }

    private void ZoomIn()
    {
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
        transform.position = Vector3.Lerp(transform.position, defaultPosition + targetPositionOffset, Time.deltaTime * zoomSpeed);
    }

    private void ZoomOut()
    {
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, defaultFOV, Time.deltaTime * zoomSpeed);
        transform.position = Vector3.Lerp(transform.position, defaultPosition, Time.deltaTime * zoomSpeed);
    }

    public void StartZoomEffect()
    {
        isZooming = true;
    }

    public void StopZoomEffect()
    {
        isZooming = false;
    }

    // Method to trigger screen shake
    public void TriggerShake(float intensity, float duration)
    {
        shakeIntensity = Mathf.Min(intensity, maxShakeIntensity);
        currentShakeIntensity = shakeIntensity;
        shakeDuration = Mathf.Min(shakeDuration + duration, maxShakeDuration);
    }
}