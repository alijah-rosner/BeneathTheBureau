using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
  // Size of the play area (in your case, 1000 units in each direction)  
  private float boxHalfSize;

  void Start()
  {
    boxHalfSize = transform.localScale.x / 2;
  }

  // Update is called once per frame
  void Update()
  {

  }

  private void OnTriggerExit(Collider other)
  {
    if (other.GetComponent<FlightController>())
    {
      Vector3 offset = CalculateWrapOffset(other.transform.position);
      // Wrap the aircraft position
      other.transform.position += offset;

      // Wrap each carried Rigidbody with the same offset
      foreach (Rigidbody rb in other.GetComponent<FlightController>().GetPassengers())
      {
        if (rb != null)
        {
          rb.transform.position += offset;
        }
      }
    }
    else
    {
      
    }
  }

  private Vector3 CalculateWrapOffset(Vector3 position)
  {
    Vector3 offset = Vector3.zero;

    // Calculate wrap offset for each axis
    if (position.x > boxHalfSize)
      offset.x = -boxHalfSize * 2;
    else if (position.x < -boxHalfSize)
      offset.x = boxHalfSize * 2;

    if (position.y > boxHalfSize)
      offset.y = -boxHalfSize * 2;
    else if (position.y < -boxHalfSize)
      offset.y = boxHalfSize * 2;

    if (position.z > boxHalfSize)
      offset.z = -boxHalfSize * 2;
    else if (position.z < -boxHalfSize)
      offset.z = boxHalfSize * 2;

    return offset;
  }

}


