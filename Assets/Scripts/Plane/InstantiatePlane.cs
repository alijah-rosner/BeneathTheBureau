using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
public class InstantiatePlane : NetworkBehaviour
{
    [SerializeField] private SO_CharacterDatabase _dataBase;
    [SerializeField] private GameObject GroundPlane;
    [SerializeField] private GameObject SimPlane;
    [SerializeField] private GameObject[] childPositions;
    [SerializeField] private NetworkObject[] children;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }

        GameObject basePlane = Instantiate(GroundPlane, new Vector3(0f, 0f, 0f), Quaternion.identity);
        basePlane.GetComponent<NetworkObject>().Spawn();

        GameObject simPlane = Instantiate(SimPlane, new Vector3(0f, 10f, 10f), Quaternion.identity);
        simPlane.GetComponent<NetworkObject>().Spawn();
        //simPlane.tag = "SimulatedAircraft";
        foreach (var client in HostManager.Instance.ClientData)
        {
            var player = _dataBase.GetCharacterByID(client.Value.CharacterID);
            if (player != null)
            {
                // Create client and spawn unique player object.
                var playerInstance = Instantiate(player.GameplayPrefab,
                    basePlane.transform.position + new Vector3(0, 0.175f, 0), Quaternion.identity);

                playerInstance.SpawnAsPlayerObject(client.Value.ClientID);
                playerInstance.transform.parent = basePlane.transform;
                playerInstance.transform.localRotation = Quaternion.identity;

                //GameObject clientCam = playerInstance.gameObject.GetComponentInChildren<Camera>().gameObject;
                //clientCam.transform.position += simPlane.transform.position;
                //clientCam.transform.parent = simPlane.transform;
            }

        }


        GameObject mouseFlightManager = GameObject.FindGameObjectWithTag("MouseFlightManager");
        mouseFlightManager.GetComponent<MouseFlightController>().aircraft = simPlane.transform;

    }
}
