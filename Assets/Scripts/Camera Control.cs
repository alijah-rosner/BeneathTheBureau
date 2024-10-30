using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
  Vector2 mouseMove = Vector2.zero;
  float xLook;
  float yLook;
  float mouseSensitivity = 1000;
  public Transform orientation;
  public GameObject player; // Reference to the player's Rigidbody to access rotation
  float rollAngle; // Variable to hold roll angle for tilting camera

  private Rigidbody playerRigidbody;

  private void Start()
  {
    playerRigidbody = player.GetComponent<Rigidbody>();
    // Lock the cursor (optional)
    // Cursor.lockState = CursorLockMode.Locked;
    // Cursor.visible = false;
  }

  void Update()
  {
    // Get mouse input for looking around
    mouseMove.y = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * mouseSensitivity;
    mouseMove.x = -Input.GetAxisRaw("Mouse X") * Time.deltaTime * mouseSensitivity;

    // Update pitch and yaw based on mouse movement
    xLook -= mouseMove.y;
    yLook -= mouseMove.x;
    xLook = Mathf.Clamp(xLook, -90f, 90f);

    // Get the current roll angle from the player's Rigidbody
    rollAngle = player.transform.eulerAngles.z;

    // Ensure the roll angle is in the range of -180 to 180 degrees
    if (rollAngle > 180)
    {
      rollAngle -= 360f;
    }

    // Apply rotation to the camera (pitch, yaw, and roll)
    transform.rotation = Quaternion.Euler(xLook, yLook, rollAngle);  // Apply roll to camera with some scaling

    // Update orientation for the player's direction (ignoring pitch and roll)
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