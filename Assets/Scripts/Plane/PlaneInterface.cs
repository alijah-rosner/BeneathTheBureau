using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class PlaneInterface : NetworkBehaviour, I_Interactable
{
    [Header("Components")]
    [SerializeField] private GameObject aircraft = null;
    [SerializeField] private GameObject aircraftCamera = null;

    // Network Variables
    private NetworkVariable<ulong> currentPilotId = new NetworkVariable<ulong>(
        ulong.MaxValue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );


    private HashSet<GameObject> players = new HashSet<GameObject>();
    private NetworkVariable<bool> isActive = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Subscribe to pilot ID changes
        currentPilotId.OnValueChanged += OnPilotChanged;

        if (aircraft == null || aircraftCamera == null)
        {
            Debug.LogError("[PlaneInterface] Missing required component references!");
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        currentPilotId.OnValueChanged -= OnPilotChanged;
    }

    private void Update()
    {

    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPilotControlServerRpc(ulong requestingPlayerId)
    {
        if (currentPilotId.Value != ulong.MaxValue)
            return;

        // Set the new pilot
        currentPilotId.Value = requestingPlayerId;
        isActive.Value = true;

        // Transfer aircraft ownership to the new pilot
        if (aircraft.TryGetComponent<NetworkObject>(out NetworkObject aircraftNetObj))
        {
            aircraftNetObj.ChangeOwnership(requestingPlayerId);
        }

        // Notify all clients about the pilot change
        UpdatePilotStateRpc(requestingPlayerId, true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReleasePilotControlServerRpc(ulong pilotId)
    {
        if (currentPilotId.Value != pilotId)
            return;

        // Clear the pilot
        currentPilotId.Value = ulong.MaxValue;
        isActive.Value = false;



        // Return aircraft ownership to server
        if (aircraft.TryGetComponent<NetworkObject>(out NetworkObject aircraftNetObj))
        {
            aircraftNetObj.ChangeOwnership(NetworkManager.ServerClientId);
        }

        // Notify all clients about the pilot change
        UpdatePilotStateRpc(pilotId, false);
    }

    [Rpc(SendTo.Everyone)]
    private void UpdatePilotStateRpc(ulong pilotId, bool isPilot)
    {
        GameObject pilotObj = GetPlayerObjectById(pilotId);
        if (pilotObj == null) return;

        PlayerController playerController = pilotObj.GetComponent<PlayerController>();
        if (playerController == null) return;

        Camera pilotCamera = playerController.GetCamera();

        if (isPilot)
        {
            // Activate pilot mode
            playerController.SetIsPilot(true);
            if (pilotCamera) pilotCamera.gameObject.SetActive(false);
            if (aircraftCamera) aircraftCamera.SetActive(true);

            if (aircraft.TryGetComponent<FlightController>(out var flightController))
            {
                flightController.enabled = true;
                flightController.TriggerController(true);
            }
        }
        else
        {
            // Deactivate pilot mode
            playerController.SetIsPilot(false);
            //playerController.SetInAircraft(true);
            if (aircraftCamera) aircraftCamera.SetActive(false);
            if (pilotCamera) pilotCamera.gameObject.SetActive(true);

            if (aircraft.TryGetComponent<FlightController>(out var flightController))
            {
                flightController.TriggerController(false);
            }
        }
    }

    private void OnPilotChanged(ulong previousPilotId, ulong newPilotId)
    {
        // Handle any necessary state cleanup when pilot changes
        if (previousPilotId != ulong.MaxValue)
        {
            GameObject previousPilot = GetPlayerObjectById(previousPilotId);
            if (previousPilot != null)
            {
                previousPilot.GetComponent<PlayerController>()?.SetIsPilot(false);
            }
        }
    }

    private GameObject GetPlayerObjectById(ulong clientId)
    {
        foreach (var player in players)
        {
            if (player.TryGetComponent<NetworkObject>(out NetworkObject netObj) &&
                netObj.OwnerClientId == clientId)
            {
                return player;
            }
        }
        return null;
    }


    [ClientRpc]
    private void SyncPlayersClientRpc(ulong networkObjectId, bool isEntering)
    {
        //if (IsServer) return; // Server already handled this

        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject playerNetObj);
        if (playerNetObj != null)
        {
            if (isEntering)
                players.Add(playerNetObj.gameObject);
            else
            {
                players.Remove(playerNetObj.gameObject);
            }
        }
    }

    public void Interact(ulong netObjId)
    {
        bool isEntering = false;
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObjId, out NetworkObject playerNetObj);
        if (playerNetObj.IsLocalPlayer)
        {
            if (currentPilotId.Value == ulong.MaxValue)
            {
                isEntering = true;
                players.Add(playerNetObj.gameObject);

                RequestPilotControlServerRpc(playerNetObj.OwnerClientId);
                SyncPlayersClientRpc(playerNetObj.NetworkObjectId, isEntering);
                playerNetObj.GetComponent<PlayerController>().SetInAircraft(true);
            }
            // If we're the pilot, release control
            else if (currentPilotId.Value == playerNetObj.OwnerClientId)
            {
                ReleasePilotControlServerRpc(playerNetObj.OwnerClientId);
                SyncPlayersClientRpc(playerNetObj.NetworkObjectId, isEntering);
                players.Remove(playerNetObj.gameObject); 
            }
        }
        // If no pilot, try to become pilot
    }
}