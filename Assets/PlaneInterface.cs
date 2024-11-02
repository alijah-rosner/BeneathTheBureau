using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneInterface : MonoBehaviour
{
  [Header("Components")]

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

      if (Vector3.Distance(player.transform.position, transform.position) < 1.5f)
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
    else
    {
      if (_active && Vector3.Distance(player.transform.position, transform.position) > 2f)
      {
        ActivatePlayerController();
        _active = false;
      }
    }
  }

  void ActivateFlightController()
  {
    player.GetComponent<SP_PlayerController>().SetIsPilot(true);
    playerCamera.SetActive(false);
    aircraftCamera.SetActive(true);
    aircraft.GetComponent<FlightController>().enabled = true;
    aircraft.GetComponent<FlightController>().TriggerController(true);
  }

  void ActivatePlayerController()
  {
    player.GetComponent<SP_PlayerController>().SetIsPilot(false);
    aircraft.GetComponent<FlightController>().TriggerController(false);
    aircraftCamera.SetActive(false);
    playerCamera.SetActive(true);
  }
}
