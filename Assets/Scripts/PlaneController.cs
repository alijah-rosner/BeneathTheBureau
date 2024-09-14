using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
  Rigidbody rb;
  public float curThrottle = 0.0f;
  public float maxThrottle = 200f;

  private float throttle;
  private float roll;
  private float pitch;
  private float yaw;

  public float changeRate = 10f;
  public float changeModifier
  {
    get
    {
      return (rb.mass / 10f) * changeRate;
    }
  }
  // Start is called before the first frame update
  void Start()
  {
    rb = GetComponent<Rigidbody>();
  }

  private void HandleInputs()
  {
    roll = Input.GetAxis("Horizontal");
    pitch = Input.GetAxis("Vertical");
    yaw = Input.GetAxis("Yaw");
  }
  // Update is called once per frame
  void Update()
  {

  }
}
