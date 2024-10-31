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
  private Rigidbody playerRigidbody;
  private Quaternion targetRotation;

  private void Start()
  {
    playerRigidbody = player.GetComponent<Rigidbody>();
    // Initialize rotation to match current camera orientation
    targetRotation = transform.rotation;
  }

  private void Update()
  {
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
    // Update orientation for player direction (yaw only)
    orientation.rotation = Quaternion.Euler(0, yLook, 0);
  }
}

            //if (Physics.Raycast(transform.position, transform.forward, out lookHit, maxDistance))
            //{
            //    intObj = lookHit.collider.GetComponent<InteractableObject>();
            //    if (intObj && Input.GetMouseButtonDown(0))
            //    {
            //        intObj.OnObjectClicked();
            //    }
            //    else if (intObj)
            //    {
            //        intObj.OnObjectHover();
            //    }
            //}
            //else
            //{
            //    if (intObj)
            //    {
            //        timer += Time.deltaTime;
            //        if (timer > 3f)
            //        {
            //            intObj.OnLookAway();
            //            intObj = null;
            //            timer = 0;
            //        }
            //    }
            //}



        //}