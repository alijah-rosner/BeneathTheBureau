using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlaneInterface : NetworkBehaviour
{
  [Header("Components")]
  [SerializeField]
  private GameObject pilot = null;

  [SerializeField]
  private GameObject pilotCamera = null;

  [SerializeField]
  private GameObject aircraft = null;

  [SerializeField]
  private GameObject aircraftCamera = null;

  private HashSet<GameObject> players = new HashSet<GameObject>();
  private bool _active = false;
  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {
    if (!_active)
    {
      if (!pilot)
      {
        foreach (var player in players)
        {
          if (player.GetComponent<NetworkObject>().IsLocalPlayer && Input.GetKeyDown(KeyCode.E))
          {
            pilotCamera = player.GetComponent<PlayerController>().GetCamera().gameObject;
            pilot = player;
            ActivatePilotController();
            _active = true;
          }
        }
      }
      else
      {
        if (pilot.GetComponent<NetworkObject>().IsLocalPlayer && Input.GetKeyDown(KeyCode.E))
        {
          ActivatePlayerController();
          pilot = null;
          pilotCamera = null;
          _active = false;
        }
      }
    }
  }

  void ActivatePilotController()
  {
    pilot.GetComponent<PlayerController>().SetIsPilot(true);
    pilotCamera.SetActive(false);
    aircraftCamera.SetActive(true);
    aircraft.GetComponent<FlightController>().enabled = true;
    aircraft.GetComponent<FlightController>().TriggerController(true);
    aircraft.GetComponent<FlightController>().NetworkObject.ChangeOwnership(pilot.GetComponent<NetworkObject>().OwnerClientId);
  }

  void ActivatePlayerController()
  {
    pilot.GetComponent<PlayerController>().SetIsPilot(false);
    aircraft.GetComponent<FlightController>().TriggerController(false);
    aircraftCamera.SetActive(false);
    pilotCamera.SetActive(true);
  }

  private void OnTriggerEnter(Collider other)
  {
    if (other.gameObject.tag == "Player")
    {
      players.Add(other.gameObject);
    }
  }
  private void OnTriggerExit(Collider other)
  {
    if (other.gameObject.tag == "Player")
    {
      players.Remove(other.gameObject);
    }
  }
}
