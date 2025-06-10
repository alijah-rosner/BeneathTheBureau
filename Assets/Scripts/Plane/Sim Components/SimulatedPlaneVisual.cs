using UnityEngine;
using Unity.Netcode;

public class SimulatedPlaneVisual : NetworkBehaviour
{
    [Header("Visual Prefab")] [SerializeField]
    private GameObject visualPrefab;

    [SerializeField] public GameObject CamPosition;
    private Transform simParent;
    private GameObject SimChar;

    NetworkVariable<Vector3> pos = new(default, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    NetworkVariable<Quaternion> rotation = new(default, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public GameObject GetVisualObj => SimChar;
    private bool started = true; 


    // Maybe need an OnPlayerVisualSpawned to set camera... 

    

    // private void Start()
    // {
    //     CreateVisualRepresentation();
    //     CreateVisualRepresentationRPC();
    //     
    // }

    private void Update()
    {
        if (started)
        {
            if (GameObject.FindGameObjectWithTag("PlayerCamera") && GameObject.FindGameObjectWithTag("SimulatedAircraft"))
            {
                CreateVisualRepresentation();
                CreateVisualRepresentationRPC();
                started = false;
            }
        }

        if (IsOwner)
        {
            pos.Value = transform.localPosition;
            rotation.Value = transform.localRotation;
        }

        UpdateSimMovementRPC();
    }
    
    [Rpc(SendTo.Everyone)]
    private void CreateVisualRepresentationRPC()
    {
        simParent = GameObject.FindGameObjectWithTag("SimulatedAircraft").transform;
        CreateVisualRepresentation();
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateSimMovementRPC()
    {
        if (SimChar)
        {
            SimChar.transform.localPosition = Vector3.Lerp(pos.Value, SimChar.transform.localPosition, 0.6f);
            SimChar.transform.localRotation = rotation.Value;
        }
    }

    private void CreateVisualRepresentation()
    {
        SimChar = Instantiate(visualPrefab, simParent);

        SimChar.transform.localPosition = transform.localPosition;
        SimChar.transform.localRotation = Quaternion.identity;
        if (IsOwner)
        {
            SimChar.GetComponent<SimCharacter>().TriggerInitialCameraSpawn();
            Debug.Log("Triggered camera change");
        }
    }

    public GameObject GetVisual()
    {
        return SimChar;
    }
}