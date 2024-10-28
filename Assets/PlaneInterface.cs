using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneInterface : MonoBehaviour
{
    [Header("Components")]
    [Tooltip("Necessary world objects to trigger flight control")]
    [Space]

    [SerializeField]
    private GameObject player = null;

    [SerializeField]
    private GameObject playerCamera = null;

    [SerializeField]
    private GameObject aircraft = null;

    [SerializeField]
    private GameObject aircraftCamera = null;

    private bool _active = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {

            if (!_active)
            {
                ActivateFlightController();
                _active = true;
            }
            else
            {
                ActivatePlayerController();
                _active = false;
            }

        }

    }

    void ActivateFlightController()
    {
        playerCamera.SetActive(false);
        aircraftCamera.SetActive(true);
        aircraft.GetComponent<FlightController>().enabled = true;
        aircraft.GetComponent<FlightController>().TriggerController();
    }

    void ActivatePlayerController()
    {
        aircraft.GetComponent<FlightController>().TriggerController();
        aircraftCamera.SetActive(false);
        playerCamera.SetActive(true);
        //player.GetComponent<TestPlayerMovement>().DeactivatePlanePhysics();
    }
}
