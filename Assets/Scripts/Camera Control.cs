using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 1000f;

    [Header("References")]
    public Transform orientation;
    public GameObject player;

    private Vector2 mouseMove = Vector2.zero;
    private float xLook;
    private float yLook;
    private Quaternion targetRotation;

    private PlayerController playerController;
    private bool wasInAircraft = false;

    private void Start()
    {
        playerController = player.GetComponent<SimCharacter>().Controller; 
        // Initialize rotation to match current camera orientation
        targetRotation = transform.rotation;
    }

    private void Update()
    {
        if (!playerController.IsOwner)
        {
            return;
        }
        HandleMouseInput();
        UpdateCameraRotation();
        UpdateOrientation();
    }

    private void HandleMouseInput()
    {
        // Get mouse input for looking around
        mouseMove.y = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * mouseSensitivity;
        mouseMove.x = Input.GetAxisRaw("Mouse X") * Time.deltaTime * mouseSensitivity;

        // Update look angles
        xLook -= mouseMove.y;
        yLook += mouseMove.x;

        // Clamp vertical look
        xLook = Mathf.Clamp(xLook, -90f, 90f);
    }

    private void UpdateCameraRotation()
    {
        // Get the player's up vector in world space
        Vector3 playerUp = player.transform.up;

        // Create a rotation that aligns with the player's up direction
        Quaternion alignRotation = Quaternion.FromToRotation(Vector3.up, playerUp);

        // Create the base camera rotation (pitch and yaw)
        Quaternion lookRotation = Quaternion.Euler(xLook, yLook, 0);

        // Combine the rotations in the correct order
        targetRotation = alignRotation * lookRotation;

        // Apply the final rotation
        transform.rotation = targetRotation;

    }

    private void UpdateOrientation()
    {
        if (!playerController)
        {
            return;
        }
        if (playerController.GetInAircraft())
        {
            // When in aircraft, align orientation with aircraft's coordinate system
            FlightController2 aircraft = FindFirstObjectByType<FlightController2>();
            if (aircraft != null)
            {
                // Get the aircraft's up vector and forward vector
                Vector3 aircraftUp = aircraft.transform.up;

                // Create a rotation that points forward in aircraft's local space
                // but respects the player's yLook value
                Quaternion aircraftWorldRotation = aircraft.transform.rotation;
                Quaternion playerLocalRotation = Quaternion.Euler(0, yLook, 0);

                // Combine rotations to get final orientation
                orientation.rotation = aircraftWorldRotation * playerLocalRotation;
            }
        }
        else
        {
            orientation.rotation = Quaternion.Euler(0, yLook, 0);
        }
    }
}
