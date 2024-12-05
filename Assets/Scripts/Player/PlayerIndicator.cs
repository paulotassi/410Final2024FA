// Heavily commented by ChatGPT
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

    // Called before the first frame update
    void Start()
    {
        // Locate the UI element named "Pointer" within the hierarchy and get its RectTransform component
        pointerRectTransform = transform.Find("Pointer").GetComponent<RectTransform>();
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

        // Convert the target's world position to a screen space position
        Vector3 targetPositionScreenpoint = camObject.WorldToScreenPoint(targetPosition.position);

        // Determine if the target is outside the screen bounds, defined by the border size
        bool isOffScreen = targetPositionScreenpoint.x <= borderSize
            || targetPositionScreenpoint.x >= Screen.width - borderSize
            || targetPositionScreenpoint.y <= borderSize
            || targetPositionScreenpoint.y >= Screen.height - borderSize;

        if (isOffScreen)
        {
            // If the target is off-screen, clamp its position within the screen bounds
            Vector3 cappedTargetScreenPosition = targetPositionScreenpoint;
            if (cappedTargetScreenPosition.x <= borderSize)
                cappedTargetScreenPosition.x = borderSize;
            if (cappedTargetScreenPosition.x >= Screen.width - borderSize)
                cappedTargetScreenPosition.x = Screen.width - borderSize;
            if (cappedTargetScreenPosition.y <= borderSize)
                cappedTargetScreenPosition.y = borderSize;
            if (cappedTargetScreenPosition.y >= Screen.height - borderSize)
                cappedTargetScreenPosition.y = Screen.height - borderSize;

            // Position the pointer UI element at the clamped screen position
            pointerRectTransform.position = cappedTargetScreenPosition;
        }
        else
        {
            // If the target is on-screen, position the pointer directly at the target's world position
            Vector3 pointerWorldPosition = targetPosition.position;
            pointerRectTransform.position = pointerWorldPosition;
        }
    }
}
