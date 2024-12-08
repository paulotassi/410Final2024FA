using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIndicator : MonoBehaviour
{
    // The position of the target we want to point at
    [SerializeField] private Transform targetPosition;
    // The position of the camera observing the scene
    [SerializeField] private Transform camPosition;
    // Reference to the camera object
    [SerializeField] private Camera camObject;
    // The RectTransform of the UI pointer element
    [SerializeField] private RectTransform pointerRectTransform;
    // The size of the border margin within which the pointer is allowed to move
    [SerializeField] public float borderSize;
    [SerializeField] public GameObject pointImage;

    // Called before the first frame update
    void Start()
    {
        // Locate the UI element named "Pointer" within the hierarchy and get its RectTransform component
        //pointerRectTransform = transform.Find("Pointer").GetComponent<RectTransform>();
    }

    // Called once per frame
    void Update()
    {
        // Get the position of the target in world space
        Vector3 toPosition = targetPosition.position;
        // Get the position of the camera in world space and ignore its Z component
        Vector3 fromPosition = camPosition.position;
        fromPosition.z = 0f; // Ensure the calculation is performed in 2D space

        // Calculate the direction vector from the camera to the target and normalize it
        Vector3 dir = (toPosition - fromPosition).normalized;

        // Calculate the angle (in degrees) of the direction vector relative to the X-axis
        float angle = (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg) % 360;

        // Rotate the pointer UI element to align with the calculated angle
        pointerRectTransform.localEulerAngles = new Vector3(0f, 0f, angle);

        // Convert the target's world position to the camera-specific screen space
        Vector3 targetPositionScreenpoint = GetViewportScreenPosition(camObject, targetPosition.position);

        // If the position is invalid (e.g., behind the camera), skip further processing
        if (targetPositionScreenpoint == Vector3.negativeInfinity)
        {
            pointerRectTransform.gameObject.SetActive(false); // Hide the pointer if the target is not visible
            return;
        }

        pointerRectTransform.gameObject.SetActive(true); // Ensure the pointer is active

        // Determine if the target is outside the camera's specific screen bounds
        bool isOffScreen = targetPositionScreenpoint.x <= camObject.pixelRect.x + borderSize
            || targetPositionScreenpoint.x >= camObject.pixelRect.x + camObject.pixelWidth - borderSize
            || targetPositionScreenpoint.y <= camObject.pixelRect.y + borderSize
            || targetPositionScreenpoint.y >= camObject.pixelRect.y + camObject.pixelHeight - borderSize;

        if (isOffScreen)
        {
            // Clamp position within the camera's specific screen bounds
            Vector3 cappedTargetScreenPosition = targetPositionScreenpoint;
            cappedTargetScreenPosition.x = Mathf.Clamp(cappedTargetScreenPosition.x,
                camObject.pixelRect.x + borderSize,
                camObject.pixelRect.x + camObject.pixelWidth - borderSize);

            cappedTargetScreenPosition.y = Mathf.Clamp(cappedTargetScreenPosition.y,
                camObject.pixelRect.y + borderSize,
                camObject.pixelRect.y + camObject.pixelHeight - borderSize);

            // Position the pointer UI element at the clamped screen position
            pointerRectTransform.position = cappedTargetScreenPosition;
            pointImage.SetActive(true);
        }
        else
        {
            // If the target is on-screen, position the pointer directly at the target's screen position
            pointerRectTransform.position = targetPositionScreenpoint;
            pointImage.SetActive(false);
            
        }
    }

    /// <summary>
    /// Converts a world position to a specific camera's screen space.
    /// </summary>
    private Vector3 GetViewportScreenPosition(Camera camera, Vector3 worldPosition)
    {
        // Convert world position to viewport space (0 to 1 in x and y for the camera's view)
        Vector3 viewportPosition = camera.WorldToViewportPoint(worldPosition);

        // Check if the object is visible in this camera
        if (viewportPosition.z < 0)
        {
            // Object is behind the camera
            return Vector3.negativeInfinity;
        }

        // Translate viewport position to actual screen coordinates
        float screenX = viewportPosition.x * camera.pixelWidth + camera.pixelRect.x;
        float screenY = viewportPosition.y * camera.pixelHeight + camera.pixelRect.y;

        return new Vector3(screenX, screenY, viewportPosition.z);
    }
}
