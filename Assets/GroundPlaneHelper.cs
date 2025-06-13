using UnityEngine;

public class GroundPlaneHelper : MonoBehaviour
{
    
    private GameObject _simAircraft;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _simAircraft = GameObject.FindGameObjectWithTag("SimulatedAircraft");
    }

    // Update is called once per frame
    void Update()
    {
        if (_simAircraft)
        {
            transform.rotation = _simAircraft.transform.rotation;
        }
    }
}
