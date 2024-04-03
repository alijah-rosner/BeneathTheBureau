using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CameraControl : NetworkBehaviour
{
    Vector2 mouseMove = Vector2.zero;
    float xLook;
    float yLook;
    float mouseSensitivity = 1000;
    public Transform orientation;
    RaycastHit lookHit;
    float maxDistance = 2f;
    InteractableObject intObj;
    float timer;

    private void Start()
    {
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        if (IsOwner)
        {
            mouseMove.y = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * mouseSensitivity;
            mouseMove.x = -Input.GetAxisRaw("Mouse X") * Time.deltaTime * mouseSensitivity;
            xLook -= mouseMove.y;
            yLook -= mouseMove.x;
            xLook = Mathf.Clamp(xLook, -90f, 90f);
            transform.rotation = Quaternion.Euler(xLook, yLook, 0);
            orientation.rotation = Quaternion.Euler(0, yLook, 0);
            if (Physics.Raycast(transform.position, transform.forward, out lookHit, maxDistance))
            {
                intObj = lookHit.collider.GetComponent<InteractableObject>();
                if (intObj && Input.GetMouseButtonDown(0))
                {
                    intObj.OnObjectClicked();
                }
                else if (intObj)
                {
                    intObj.OnObjectHover();
                }
            }
            else
            {
                if (intObj)
                {
                    timer += Time.deltaTime;
                    if (timer > 3f)
                    {
                        intObj.OnLookAway();
                        intObj = null;
                        timer = 0;
                    }
                }
            }



        }
    }
}
