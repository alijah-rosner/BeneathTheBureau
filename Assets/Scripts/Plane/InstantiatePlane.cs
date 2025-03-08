using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
public class InstantiatePlane : NetworkBehaviour
{
    [SerializeField] private SO_CharacterDatabase _dataBase;
    [SerializeField] private GameObject PlanePrefab;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }
        GameObject basePlane = Instantiate(PlanePrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
        GameObject simPlane = Instantiate(PlanePrefab, new Vector3(0f, 10f, 10f), Quaternion.identity);

        float xOffset = 0;
        float zOffset = 0;
        foreach (var client in HostManager.Instance.ClientData)
        {
            var player = _dataBase.GetCharacterByID(client.Value.CharacterID);
            if (player != null)
            {
                // Create client and spawn unique player object.
                var playerInstance = Instantiate(player.GameplayPrefab, 
                    basePlane.transform.position + new Vector3(xOffset, 0f, zOffset), Quaternion.identity, basePlane.transform);

                playerInstance.SpawnAsPlayerObject(client.Value.ClientID);

                GameObject clientCam = playerInstance.gameObject.GetComponentInChildren<Camera>().gameObject;
                clientCam.transform.position += simPlane.transform.position;
                //clientCam.transform.parent = simPlane.transform;
            }

        }
        FlightController simController = simPlane.AddComponent<FlightController>();
        GameObject mouseFlightManager = GameObject.FindGameObjectWithTag("MouseFlightManager");
        mouseFlightManager.GetComponent<MouseFlightController>().aircraft = simPlane.transform;

        simController.controller = mouseFlightManager.GetComponent<MouseFlightController>();
        simController.planeInterface = simPlane.GetComponentInChildren<PlaneInterface>().gameObject;

        simController.thrust = 200;
        simController.pitchModifier = 25;
        simController.maxThrust = 100;
        simController.acceleration = 10;
        simController.turnTorque = new Vector3(90, 45, 60);
        simController.forceMult = 800f;
        simController.rumbleForce = 2f;
        simController.turbulenceForce = 5f;
        simController.turbulenceTorqueForce = 2f;
        simController.turbulenceChance = 0.005f;

        simController.sensitivity = 3f;
        simController.aggressiveTurnAngle = 10f;
        simPlane.tag = "SimulatedAircraft";
    }

    private void InitPlane()
    {

    }
}
