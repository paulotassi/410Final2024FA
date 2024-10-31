using System.Collections;
using System.Collections.Generic;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.InputSystem; // Required for using the Input System

public class PlayerAttack : MonoBehaviour
{
    // Public variables accessible from the Unity Editor
    public Camera mainCam; // Main Camera reference to get mouse position in the world space
    public float horizontalAimInput; // Horizontal aim input value (-1 to 1)
    public float verticalAimInput; // Vertical aim input value (-1 to 1)
    public Vector3 targetPos; // Position on the screen where the player is aiming
    public PlayerController controller; // Reference to the PlayerController script to get input values
    public Transform aimObject;
    public float aimAngle;
    public float controllerDeadzone = 0.1f;

    // Function to remap a value from one range to another
    // Example: Remaps a value from [-1, 1] to [-250, 1500] or [-300, 500]


    // Called every frame
    void Update()
    {
        // Get horizontal and vertical aim input from the controller (e.g., joystick or mouse input)
        horizontalAimInput = controller.aimInput.x; // X-axis aim input, range [-1, 1]
        verticalAimInput = controller.aimInput.y; // Y-axis aim input, range [-1, 1]

        // If the current control scheme is "GamePad", use the remapped values for aiming
        if (controller.GetComponent<PlayerInput>().currentControlScheme == "GamePad")
        {
            Vector2 aimVector = new Vector2(horizontalAimInput, verticalAimInput);
            // Set target position using the remapped aim input (for gamepad aiming)
            if (aimVector.magnitude > controllerDeadzone)
            {
                // Calculate the aim angle in radians and convert it to degrees
                aimAngle = Mathf.Atan2(verticalAimInput, horizontalAimInput) * Mathf.Rad2Deg;

                // Apply the rotation to the aimObject
                aimObject.rotation = Quaternion.Euler(0, 0, aimAngle);
            }
        }
        else
        {
            // For other control schemes (e.g., keyboard and mouse), use the raw input values directly
            targetPos = new Vector3(horizontalAimInput, verticalAimInput, -10); // -10 for Z-axis (camera depth)

            // Convert the screen position (targetPos) into world position using the camera
            // This is useful for determining where the player is aiming in the game world
            Vector3 mousePos = mainCam.ScreenToWorldPoint(targetPos);

            // Calculate the direction from the player's current position to the aim position
            // This helps in determining the angle of rotation based on where the player is aiming
            Vector3 rotation = new Vector3(mousePos.x - transform.position.x, mousePos.y - transform.position.y, transform.position.z);

            // Calculate the angle (in radians) between the player's position and the aim position
            // Then convert that angle to degrees for easier use in Unity (since rotations in Unity are in degrees)
            float rotZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;

            // Rotate the player towards the target position (based on the calculated angle)
            // Quaternion.Euler is used to set the rotation based on the Z-axis angle (rotZ)

            transform.rotation = Quaternion.Euler(0, 0, rotZ);
        }
    }
}