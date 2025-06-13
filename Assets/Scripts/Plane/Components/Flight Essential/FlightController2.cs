using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FlightController2 : AircraftSystem
{

    [Header("Components")]
    [SerializeField] public MouseFlightController controller = null;
    [SerializeField] public GameObject planeInterface = null;

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

    private bool _isControlling = false;


    //public NetworkVariable<float> CurrentTotalThrust = new NetworkVariable<float> { Value = 0f };
    private NetworkVariable<float> CurrentPitch = new NetworkVariable<float>();
    private NetworkVariable<float> CurrentYaw = new NetworkVariable<float>();
    private NetworkVariable<float> CurrentRoll = new NetworkVariable<float>();

    //private NetworkVariable<bool> _isControlling = new NetworkVariable<bool>(false); 

    private EngineSystem engineSystem;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        rigid = GetComponent<Rigidbody>();
        _isControlling = false;
        //engineSystem.CurrentThrust.OnValueChanged += CalculateTotalCurrentThrust;
    }

    private void Start()
    {
        controller = GameObject.FindGameObjectWithTag("MouseFlightManager").GetComponent<MouseFlightController>();
    }

    private void CalculateTotalCurrentThrust(float oldValue, float newValue)
    {
        //CurrentTotalThrust.Value -= oldValue;
        //CurrentTotalThrust.Value += newValue; 
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        UpdateAltimeter();

        // Calculate the autopilot inputs.

        float autoYaw = 0f;
        float autoPitch = 0f;
        float autoRoll = 0f;
        if (controller != null)
            RunAutopilotRpc(controller.MouseAimPos, out autoYaw, out autoPitch, out autoRoll);

        yaw = autoYaw;
        pitch = autoPitch;
        roll = autoRoll;
    }

    private void FixedUpdate()
    {
        // Server-side physics calculations
        if (IsOwner)
        {
            curThrust = Mathf.Clamp(curThrust + acceleration * Time.fixedDeltaTime, 0f, maxThrust);
            //currentPitch = Mathf.Lerp(currentPitch, netPitch.Value, Time.deltaTime * 2f);
            //currentRoll = Mathf.Lerp(currentRoll, netRoll.Value, Time.deltaTime * 2f);
            currentPitch = pitch;
            currentRoll = roll;

            // Apply forces
            rigid.AddRelativeForce(Vector3.forward * curThrust * forceMult, ForceMode.Force);
            rigid.AddRelativeTorque(new Vector3(
                turnTorque.x * currentPitch,
                turnTorque.y * yaw,
                -turnTorque.z * currentRoll) * forceMult,
                ForceMode.Force);

            ClampRotation();
        }

        AddTurbulenceEffect();
        
        if (!IsServer)
        {    
            return;
        }

        //UpdatePassengers();
    }

    public void RunAutopilotRpc(Vector3 flyTarget, out float yaw, out float pitch, out float roll)
    {
        // Convert destination to local space.
        var localFlyTarget = transform.InverseTransformPoint(flyTarget).normalized * sensitivity;
        var angleOffTarget = Vector3.Angle(transform.forward, flyTarget - transform.position);

        yaw = Mathf.Clamp(localFlyTarget.x, -1f, 1f);
        pitch = -Mathf.Clamp(localFlyTarget.y, -0.5f, 0.5f);
        var relativeRoll = Mathf.Clamp(localFlyTarget.x, -0.35f, 0.35f);

        // Manuevers the aircraft to fly wings level.
        // This can be done by zeroing out the Y component of the aircraft's right.
        var wingsLevelRoll = Mathf.Clamp(transform.right.y, -1f, 1f);

        // Blend between auto level and banking into the target.
        var wingsLevelInfluence = Mathf.InverseLerp(0f, aggressiveTurnAngle, angleOffTarget);
        roll = Mathf.Lerp(wingsLevelRoll, relativeRoll, wingsLevelInfluence);
    }

    private void UpdateAltimeter()
    {
        if (_isControlling)
        {
            float keyboardPitch = Input.GetAxis("Vertical");
            {
                Debug.Log(keyboardPitch);
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

    private void AddTurbulenceEffect()
    {
        // Add consistent rumble
        Vector3 rumble = new Vector3(
                                    Random.Range(-1f, 1f),
                                    Random.Range(-1f, 1f),
                                    Random.Range(-1f, 1f)) * rumbleForce;

        rigid.AddForce(rumble, ForceMode.Acceleration);
        if (!IsServer) return;

        // Occasionally add turbulence
        if (Random.value < turbulenceChance)
        {
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


    public bool GetController()
    {
        return _isControlling;
    }
    public void TriggerController(bool r)
    {
        _isControlling = r;
    }
}

