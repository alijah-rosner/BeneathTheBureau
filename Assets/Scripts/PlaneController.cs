using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
  Rigidbody rb;

  public float throttleRate = 0.1f;
  public float maxThrust = 200f;

  private float throttle;
  private float roll;
  private float pitch;
  private float yaw;

  public float changeRate = 0.005f;
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
    if (Input.GetKey(KeyCode.Q))
    {

    }
    roll = Input.GetAxis("Horizontal");
    pitch = Input.GetAxis("Vertical");
    yaw = Input.GetAxis("Yaw");

    if (Input.GetKey(KeyCode.Space))
    {
      throttle += throttleRate;
    }
    else if (Input.GetKey(KeyCode.LeftControl))
    {
      throttle -= throttleRate;
    }

    throttle = Mathf.Clamp(throttle, 0f, 100f);
  }
  // Update is called once per frame
  void Update()
  {
    if (rb)
    {
      HandleInputs();
      Debug.Log("Roll: " + roll);
      Debug.Log("Pitch: " + pitch);
      Debug.Log("Yaw: " + yaw);
    }
  }

  private void FixedUpdate()
  {
    if (rb)
    {
      rb.AddForce(transform.forward * maxThrust * throttle);
      rb.AddTorque(transform.up * yaw * changeModifier);
      rb.AddTorque(transform.right * pitch * changeModifier);
      rb.AddTorque(transform.forward * roll * changeModifier);
    }
  }
}
