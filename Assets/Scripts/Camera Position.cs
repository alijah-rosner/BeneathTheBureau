using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CameraPosition : MonoBehaviour
{
  public Transform cameraPosition;

  void Update()
  {
    Transform playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
    float rollAngle = playerTrans.eulerAngles.z;

    // Ensure the roll angle is in the range of -180 to 180 degrees
    if (rollAngle > 180)
    {
      rollAngle -= 360f;

    }
    //{
    transform.position = Vector3.Lerp(transform.position, cameraPosition.position, .5f);
    //transform.rotation = Quaternion.Euler(new Vector3(playerTrans.rotation.eulerAngles.x, playerTrans.rotation.eulerAngles.y, Mathf.Abs(rollAngle)));
    //}
  }
}
