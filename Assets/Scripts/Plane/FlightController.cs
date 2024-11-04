using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FlightController : NetworkBehaviour
{

  [Header("Components")]
  [SerializeField] private MouseFlightController controller = null;

  [Header("Physics")]
  [Tooltip("Force to push plane forwards with")] public float thrust = 100f;
  [Tooltip("Rate at which pitch adjusts from Altimeter")] public float pitchModifier = 25f;
  [Tooltip("Max speed plane can reach")] public float maxThrust = 100f;
  [Tooltip("Force to accelerate thrust")] public float acceleration = 50f;
  [Tooltip("Pitch, Yaw, Roll")] public Vector3 turnTorque = new Vector3(90f, 25f, 45f);
  [Tooltip("Multiplier for all forces")] public float forceMult = 1000f;

  [Header("Turbulence")]
  [Tooltip("Slight rumble effect intensity")] public float rumbleForce = 0.05f;
  [Tooltip("Turbulence intensity")] public float turbulenceForce = 2.5f;
  [Tooltip("Rotational turbulence intensity")] public float turbulenceTorqueForce = 5.0f;
  [Tooltip("Probability of turbulence to occur (between 0 and 1)")] public float turbulenceChance = 0.01f;


  [Header("Autopilot")]
  [Tooltip("Sensitivity for autopilot flight.")] public float sensitivity = 5f;
  [Tooltip("Angle at which airplane banks fully into target.")] public float aggressiveTurnAngle = 10f;

  [Header("Input")]
  [SerializeField] [Range(-1f, 1f)] private float pitch = 0f;
  [SerializeField] [Range(-1f, 1f)] private float yaw = 0f;
  [SerializeField] [Range(-0.25f, 0.25f)] private float roll = 0f;

  public float Pitch { set { pitch = Mathf.Clamp(value, -1f, 1f); } get { return pitch; } }
  public float Yaw { set { yaw = Mathf.Clamp(value, -1f, 1f); } get { return yaw; } }
  public float Roll { set { roll = Mathf.Clamp(value, -1f, 1f); } get { return roll; } }

  private Rigidbody rigid;

  private float currentPitch = 0f;
  private float currentRoll = 0f;
  private float curThrust;

  //Whether or not to recieve input
  private bool _isControlling = false;
  [Header("Input")]
  [SerializeField]
  //Container of all rigidbodies inside plane
  private HashSet<Rigidbody> _passengers = new();

  private Vector3 previousLocalVelocity;

  private bool _isMoving => rigid.velocity.magnitude > 0f;
  private bool _hasStartedMoving = false;
  private bool _turbulenceFlag = false;


  public void TriggerController(bool r)
  {
    _isControlling = r;
  }

  public bool GetIsMoving()
  {
    return _isMoving;
  }
  public bool GetController()
  {
    return _isControlling;
  }

  public HashSet<Rigidbody> GetPassengers()
  {
    return _passengers;
  }

  private void Awake()
  {
    curThrust = 0f;
    rigid = GetComponent<Rigidbody>();

    if (controller == null)
      Debug.LogError(name + ": Plane - Missing reference to MouseFlightController!");
  }

  private void Update()
  {
    // Update altitude based on plane interface tool.

    UpdateAltimeter();

    // Calculate the autopilot inputs.

    float autoYaw = 0f;
    float autoPitch = 0f;
    float autoRoll = 0f;
    if (controller != null)
      RunAutopilot(controller.MouseAimPos, out autoYaw, out autoPitch, out autoRoll);

    yaw = autoYaw;
    pitch = autoPitch;
    roll = autoRoll;

  }
  private void FixedUpdate()
  {
    // Calculate thrust
    curThrust = Mathf.Clamp(curThrust + acceleration * Time.fixedDeltaTime, 0f, maxThrust);
    // Smoothing for pitch and roll
    currentPitch = Mathf.Lerp(currentPitch, pitch, Time.deltaTime * 2f);  // Smooth over 2 seconds
    currentRoll = Mathf.Lerp(currentRoll, roll, Time.deltaTime * 2f);

    // Apply forces with smoothed values
    rigid.AddRelativeForce(Vector3.forward * curThrust * forceMult, ForceMode.Force);
    rigid.AddRelativeTorque(new Vector3(turnTorque.x * currentPitch,
                                        turnTorque.y * yaw,
                                        -turnTorque.z * currentRoll) * forceMult,
                            ForceMode.Force);


    ClampRotation();

    // Check if the aircraft has started moving
    if (!_hasStartedMoving && _isMoving)
    {
      _hasStartedMoving = true;
      SetPassengersInAircraft(true);
    }
    AddRumbleEffect();

    // Occasionally add larger random forces to simulate turbulence (5% chance)
    if (Random.value < turbulenceChance)
    {
      AddTurbulenceEffect();
    }

    UpdatePassengers();
    previousLocalVelocity = transform.InverseTransformDirection(rigid.velocity);

  }


  /// <summary>
  /// Calculate the yaw, pitch, and roll based off the aircraft position relative to target.
  /// </summary>
  private void RunAutopilot(Vector3 flyTarget, out float yaw, out float pitch, out float roll)
  {
    // Convert destination to local space.
    var localFlyTarget = transform.InverseTransformPoint(flyTarget).normalized * sensitivity;
    var angleOffTarget = Vector3.Angle(transform.forward, flyTarget - transform.position);

    // ====================
    // PITCH AND YAW
    // ====================

    // Yaw/Pitch into the target so as to put it directly in front of the aircraft.
    // A target is directly in front the aircraft if the relative X and Y are both
    // zero. Note: this does not handle for the case where the target is directly behind.
    yaw = Mathf.Clamp(localFlyTarget.x, -1f, 1f);
    pitch = -Mathf.Clamp(localFlyTarget.y, -0.5f, 0.5f);

    // ====================
    // ROLL
    // ====================

    // Roll is a little special because there are two different roll commands depending
    // on the situation. When the target is off axis, then the plane should roll into it.
    // When the target is directly in front, the plane should fly wings level.

    // An "aggressive roll" is input such that the aircraft rolls into the target so
    // that pitching up (handled above) will put the nose onto the target. This is
    // done by rolling such that the X component of the target's position is zeroed.
    var agressiveRoll = Mathf.Clamp(localFlyTarget.x, -0.35f, 0.35f);

    // A "wings level roll" is a roll commands the aircraft to fly wings level.
    // This can be done by zeroing out the Y component of the aircraft's right.
    var wingsLevelRoll = Mathf.Clamp(transform.right.y, -1f, 1f);

    // Blend between auto level and banking into the target.
    var wingsLevelInfluence = Mathf.InverseLerp(0f, aggressiveTurnAngle, angleOffTarget);
    roll = Mathf.Lerp(wingsLevelRoll, agressiveRoll, wingsLevelInfluence);
  }

  /// <summary>
  /// Set desired altitude of target position.
  /// </summary>
  private void UpdateAltimeter()
  {
    if (_isControlling)
    {
      float keyboardPitch = Input.GetAxis("Vertical");
      {
        if (Mathf.Abs(keyboardPitch) > .25f)
        {
          float roundedPitch = -Mathf.Sign(keyboardPitch);
          float currentAltitude = controller.GetDestinationAltitude();
          float newAltitude = currentAltitude + (roundedPitch * pitchModifier * Time.deltaTime);
          controller.SetDestinationAltitude(newAltitude);
        }
      }
    }
  }

  /// <summary>
  /// Adds small random forces to simulate rumble effect .
  /// </summary>
  private void AddRumbleEffect()
  {
    Vector3 rumble = new Vector3(
        Random.Range(-1f, 1f),
        Random.Range(-1f, 1f),
        Random.Range(-1f, 1f)
    ) * rumbleForce;

    rigid.AddForce(rumble, ForceMode.Acceleration);
  }

  /// <summary>
  /// Calculate the yaw, pitch, and roll based off the aircraft position relative to target.
  /// </summary>
  private void AddTurbulenceEffect()
  {
    _turbulenceFlag = true;
    Vector3 turbulence = new Vector3(
        Random.Range(-1f, 1f),
        Random.Range(-1f, 1f),
        0f
    ) * turbulenceForce;

    rigid.AddForce(turbulence, ForceMode.Acceleration);

    Vector3 turbulenceTorque = new Vector3(
        Random.Range(-1f, 1f),
        Random.Range(-1f, 1f),
        Random.Range(-1f, 1f)
    ) * turbulenceTorqueForce;

    rigid.AddTorque(turbulenceTorque, ForceMode.Acceleration);
  }


  private void SetPassengersInAircraft(bool inAircraft)
  {
    foreach (Rigidbody passenger in _passengers)
    {
      passenger.drag = 0f;
      if (passenger.GetComponent<SP_PlayerController>())
      {
        passenger.GetComponent<SP_PlayerController>().SetInAircraft(inAircraft);
      }
    }
  }

  private void UpdatePassengers()
  {
    // Calculate the local velocity and its change compared to previous frame
    Vector3 currentLocalVelocity = transform.InverseTransformDirection(rigid.velocity);
    Vector3 localAcceleration = (currentLocalVelocity - previousLocalVelocity) / Time.fixedDeltaTime;

    foreach (Rigidbody passenger in _passengers)
    {
      Vector3 counteractingForce = transform.TransformDirection(localAcceleration);
      if (!_turbulenceFlag)
      {
        // Apply acceleration compensation as a force to simulate realistic movement
        passenger.AddForce(counteractingForce, ForceMode.Acceleration);
      }
      else
      {
        Vector3 turbulenceJerk = new Vector3(
        Random.Range(-1f, 1f),
        Random.Range(-1f, 1f),
        Random.Range(-1f, 1f)) * turbulenceForce * 2f;
        passenger.AddForce(counteractingForce + turbulenceJerk, ForceMode.Acceleration);
        _turbulenceFlag = false;
      }
    }

    // Update the previous velocity to the current one for the next frame
    previousLocalVelocity = currentLocalVelocity;
  }

  // Convert angles greater than 180 to a negative equivalent angle
  // Clamp to keep it from exceeding desired values
  private void ClampRotation()
  {
    Vector3 currentRotation = transform.localEulerAngles;

    // Handle pitch 
    float pitch = currentRotation.x > 180 ? currentRotation.x - 360 : currentRotation.x;
    pitch = Mathf.Clamp(pitch, -15f, 15f);

    // Handle roll 
    float roll = currentRotation.z > 180 ? currentRotation.z - 360 : currentRotation.z;
    roll = Mathf.Clamp(roll, -25f, 25f);

    // Update the rotation
    transform.localEulerAngles = new Vector3(pitch, currentRotation.y, roll);
  }


  private void OnTriggerEnter(Collider other)
  {
    if (other.GetComponent<Rigidbody>())
    {
      _passengers.Add(other.GetComponent<Rigidbody>());
      if (_isMoving && other.GetComponent<SP_PlayerController>())
      {
        other.GetComponent<Rigidbody>().drag = 0;
        Debug.Log(rigid.velocity.magnitude);
        other.GetComponent<SP_PlayerController>().SetInAircraft(true);
      }
    }
  }
  private void OnTriggerExit(Collider other)
  {
    if (other.GetComponent<Rigidbody>())
    {
      _passengers.Remove(other.GetComponent<Rigidbody>());
      other.transform.parent = null;
      if (other.GetComponent<SP_PlayerController>())
      {
        other.GetComponent<SP_PlayerController>().SetInAircraft(false);
      }
    }
  }
}
