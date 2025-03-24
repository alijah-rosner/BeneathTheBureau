using UnityEngine;
using Unity.Netcode;

public class SimulatedPlaneVisual : NetworkBehaviour
{
    [Header("Visual Prefab")]
    [SerializeField] private GameObject visualPrefab;
    private Transform simParent;
    private GameObject obj; 
   
    NetworkVariable<Vector3> pos = new();
    NetworkVariable<Quaternion> rotation = new();
    
    public GameObject GetVisualObj => obj; 


    // Maybe need an OnPlayerVisualSpawned to set camera... 


    public override void OnNetworkSpawn()
    {
        simParent = GameObject.FindGameObjectWithTag("SimulatedAircraft").transform;
        CreateVisualRepresentation();
    }

    private void Update()
    {
        if (IsOwner)
        {
            if (simParent)
            {
                 obj.transform.localPosition = transform.localPosition;
                 obj.transform.localRotation = transform.localRotation;
            }
        }
    }
   
    private void CreateVisualRepresentation()
    {
        obj = Instantiate(visualPrefab, simParent);

        obj.transform.localPosition = transform.localPosition;
        obj.transform.localRotation = Quaternion.identity; 
        // Instantiate object;
        // set ovject local position and local rotation;
        // all other stuff;
        // visual reprsentation would have script depending on type;
    }

    public GameObject GetVisual()
    {
        return obj; 
    }


}
