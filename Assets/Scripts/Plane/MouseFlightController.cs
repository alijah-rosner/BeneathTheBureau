using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseFlightController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    [Tooltip("Transform of the aircraft the rig follows and references")]
    public Transform aircraft = null;
    [SerializeField]
    [Tooltip("Transform of the object the mouse rotates to generate MouseAim position")]
    private Transform mouseAim = null;
    [SerializeField]
    [Tooltip("Transform of the object on the rig which the camera is attached to")]
    private Transform cameraRig = null;
    [SerializeField]
    [Tooltip("Transform of the camera itself")]
    private Transform cam = null;

    [Header("Options")]
    [SerializeField]
    [Tooltip("Follow aircraft using fixed update loop")]
    private bool useFixed = true;

    [SerializeField]
    [Tooltip("How quickly the camera tracks the mouse aim point.")]
    private float camSmoothSpeed = 1f;

    [SerializeField]
    [Tooltip("Mouse sensitivity for the mouse flight target")]
    private float mouseSensitivity = 3f;

    [SerializeField]
    [Tooltip("How far the boresight and mouse flight are from the aircraft")]
    private float aimDistance = 500f;

    [SerializeField]
    [Tooltip("Altitude set by pilot")]
    private float destinationAltitude = 50; // Default height



    [Space]
    [SerializeField]
    [Tooltip("How far the boresight and mouse flight are from the aircraft")]
    private bool showDebugInfo = false;

    private Vector3 frozenDirection = Vector3.forward;
    private bool isMouseAimFrozen = true;  // Initially frozen
    private bool isMouseClicked = false;   // Track if left mouse button has been clicked
    public bool _isEnabled = false; // Track if someone is operating the aircraft

    public void SetDestinationAltitude(float newAltitude)
    {
        Vector3 currentPos = mouseAim.position;
        if (Mathf.Abs((destinationAltitude) - aircraft.position.y) <= 120f)
        {

            destinationAltitude = newAltitude;

            // If we're currently frozen, update the mouseAim's position to the new height
            if (isMouseAimFrozen && mouseAim != null)
            {
                currentPos.y = destinationAltitude;
                mouseAim.position = currentPos;

                // Update the frozen direction to maintain the same horizontal direction
                frozenDirection = (GetFrozenMouseAimPos() - mouseAim.position).normalized;
            }
        }
    }

    public float GetDestinationAltitude()
    {
        return destinationAltitude;
    }

    // Get a point along the aircraft's boresight projected out to aimDistance meters.
    public Vector3 BoresightPos
    {
        get
        {
            return aircraft == null
                 ? transform.forward * aimDistance
                 : (aircraft.transform.forward * aimDistance) + aircraft.transform.position;
        }
    }

    /// <summary>
    /// Get the position that the mouse is indicating the aircraft should fly, projected
    /// out to aimDistance meters. Also meant to be used to draw a mouse cursor.
    /// </summary>
    public Vector3 MouseAimPos
    {
        get
        {
            if (mouseAim != null)
            {
                return isMouseAimFrozen
                    ? GetFrozenMouseAimPos()
                    : mouseAim.position + (mouseAim.forward * aimDistance);
            }
            else
            {
                return transform.forward * aimDistance;
            }
        }
    }

    private void Awake()
    {
        if (aircraft == null)
            Debug.LogError(name + "MouseFlightController - No aircraft transform assigned!");
        if (mouseAim == null)
            Debug.LogError(name + "MouseFlightController - No mouse aim transform assigned!");
        if (cameraRig == null)
            Debug.LogError(name + "MouseFlightController - No camera rig transform assigned!");
        if (cam == null && _isEnabled)
            Debug.LogError(name + "MouseFlightController - No camera transform assigned!");

        // To work correctly, the entire rig must not be parented to anything.
        transform.parent = null;
    }

    private void Update()
    {
        if (aircraft == null)
        {
            aircraft = GameObject.FindGameObjectWithTag("Aircraft").transform;
        }
        _isEnabled = aircraft.GetComponent<FlightController>().GetController();
        if (useFixed == false)
            UpdateCameraPos();

        RotateRig();
    }

    private void FixedUpdate()
    {
        if (useFixed == true)
            UpdateCameraPos();
    }


    private void RotateRig()
    {
        if (mouseAim == null || cam == null || cameraRig == null)
            return;

        // Check if the left mouse button is clicked.
        if (Input.GetMouseButtonDown(0) && _isEnabled)
        {
            // Toggle the mouse aim freezing state.
            isMouseAimFrozen = false;  // Enable aim update
            UpdateMouseAimPosition();  // Instantly set the aim to the cursor position
        }

        // If the mouse aim is not frozen, proceed with normal update.
        if (isMouseAimFrozen)
        {
            // The up vector of the camera normally is aligned to the horizon.
            Vector3 upVec = (Mathf.Abs(mouseAim.forward.y) > 0.9f) ? cameraRig.up : Vector3.up;

            // Smoothly rotate the camera to face the mouse aim.
            cameraRig.rotation = Damp(cameraRig.rotation,
                                      Quaternion.LookRotation(mouseAim.forward, upVec),
                                      camSmoothSpeed,
                                      Time.deltaTime);
        }
    }

    // Update the mouseAim position instantly to the cursor position in the world.
    private void UpdateMouseAimPosition()
    {
        if (_isEnabled)
        {

            // Get the Camera component from the cam transform
            Camera cameraComponent = cam.GetComponent<Camera>();

            if (cameraComponent == null)
            {
                Debug.LogError("No Camera component found on the assigned cam transform.");
                return;
            }

            // Get the mouse position in screen space
            Vector3 mousePosition = Input.mousePosition;

            // Cast a ray from the camera through the mouse position
            Ray ray = cameraComponent.ScreenPointToRay(mousePosition);

            // Use the ray to get the point in front of the aircraft at the aimDistance
            Vector3 newAimPosition = ray.GetPoint(aimDistance);

            // Lock the Y-axis to the current height of the mouseAim position
            newAimPosition.y = mouseAim.position.y;

            // Set the mouseAim's forward direction to point towards this new position
            Vector3 newForward = (newAimPosition - mouseAim.position).normalized;
            mouseAim.forward = newForward;

            // Freeze the new direction until next click
            frozenDirection = mouseAim.forward;
            isMouseAimFrozen = true;
        }
    }


    private Vector3 GetFrozenMouseAimPos()
    {
        if (mouseAim != null)
            return mouseAim.position + (frozenDirection * aimDistance);
        else
            return transform.forward * aimDistance;
    }

    private void UpdateCameraPos()
    {
        if (aircraft != null)
        {
            // Move the whole rig to follow the aircraft.
            transform.position = aircraft.position;
        }
    }

    // Frame-rate independent damping
    private Quaternion Damp(Quaternion a, Quaternion b, float lambda, float dt)
    {
        return Quaternion.Slerp(a, b, 1 - Mathf.Exp(-lambda * dt));
    }

    private void OnDrawGizmos()
    {
        if (showDebugInfo == true)
        {
            Color oldColor = Gizmos.color;

            // Draw the boresight position.
            if (aircraft != null)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(BoresightPos, 10f);
            }

            if (mouseAim != null)
            {
                // Draw the position of the mouse aim position.
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(MouseAimPos, 10f);

                // Draw axes for the mouse aim transform.
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(mouseAim.position, mouseAim.forward * 50f);
                Gizmos.color = Color.green;
                Gizmos.DrawRay(mouseAim.position, mouseAim.up * 50f);
                Gizmos.color = Color.red;
                Gizmos.DrawRay(mouseAim.position, mouseAim.right * 50f);
            }

            Gizmos.color = oldColor;
        }
    }
}
