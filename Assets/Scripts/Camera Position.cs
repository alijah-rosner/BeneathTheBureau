using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    public Transform cameraPosition;

    void Update()
    {
        //if (IsOwner)
        //{
            transform.position = Vector3.Lerp(transform.position, cameraPosition.position, .5f);
        //}
    }
}
