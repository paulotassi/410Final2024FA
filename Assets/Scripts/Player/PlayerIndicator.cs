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

        // In generated maps the target is an exit on the current tile, not a fixed object
        mapGenerator = FindFirstObjectByType<MapGenerator>();
        if (mapGenerator != null)
        {
            guideTarget = new GameObject("PathGuideTarget").transform;
            targetPosition = guideTarget;
        }
    }

    // True for indicators that point at a scene object (versus mode); false for boss-style ones assigned at runtime
    public bool HasFixedTarget => targetPosition != null;
    public Camera CameraObject => camObject;

    private MapGenerator mapGenerator;
    private Transform guideTarget;
    private Transform trackedPlayer;
    private Vector2Int currentCell;
    private bool hasCell;

    // Which player's tile this indicator follows (set by GameManager)
    public void SetTrackedPlayer(Transform player) { trackedPlayer = player; }

    // Returns false until a valid direction is known
    private bool UpdateGuideTarget()
    {
        if (!mapGenerator.Generated) return false;

        Vector3 pos = trackedPlayer != null ? trackedPlayer.position : camPosition.position;
        Vector2Int cell = mapGenerator.CellOf(pos);

        // Switch tiles only once the player is clearly inside the new one, so the arrow doesn't flicker in doorways
        if (!hasCell || (cell != currentCell && mapGenerator.IsInsideCell(pos, cell, 2f)))
        {
            if (mapGenerator.TryGetGuidePoint(cell, out Vector3 point))
            {
                currentCell = cell;
                hasCell = true;
                guideTarget.position = point;
            }
        }
        return hasCell;
    }

    private Vector2 lastDirection = Vector2.right;

    // Path guide: the arrow always sits on the border of this player's screen and points along the direction
    // from the player toward the exit to take. It never sits on the exit itself, even when the exit is on screen.
    private void SlideAlongScreenEdge()
    {
        Vector3 from = trackedPlayer != null ? trackedPlayer.position : camPosition.position;
        Vector2 toTarget = (Vector2)(guideTarget.position - from);
        Vector2 dir = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : lastDirection;
        lastDirection = dir;

        pointerRectTransform.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

        // Where a ray from the middle of the screen in that direction meets the border
        Rect view = camObject.pixelRect;
        float halfW = Mathf.Max(1f, view.width * 0.5f - borderSize);
        float halfH = Mathf.Max(1f, view.height * 0.5f - borderSize);
        float tx = Mathf.Abs(dir.x) > 0.0001f ? halfW / Mathf.Abs(dir.x) : float.MaxValue;
        float ty = Mathf.Abs(dir.y) > 0.0001f ? halfH / Mathf.Abs(dir.y) : float.MaxValue;
        Vector2 screenPos = view.center + dir * Mathf.Min(tx, ty);

        pointerRectTransform.gameObject.SetActive(true);
        pointerRectTransform.position = new Vector3(screenPos.x, screenPos.y, pointerRectTransform.position.z);
        pointImage.SetActive(true);
    }

    // Called once per frame
    void Update()
    {
        // Generated maps: aim at whichever exit of the current tile leads toward the finish.
        // The aim only changes once the player is properly inside a different tile.
        if (guideTarget != null)
        {
            if (!UpdateGuideTarget())
            {
                pointerRectTransform.gameObject.SetActive(false);
                return;
            }

            SlideAlongScreenEdge();
            return;
        }

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
            // Versus mode hides the arrow while the target is visible; the path guide keeps showing it
            pointImage.SetActive(guideTarget != null);

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
