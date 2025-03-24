using UnityEngine;
using Unity.Netcode;

public class SystemControl : MonoBehaviour, I_Interactable
{
    private AircraftSystem targetSystem;
    private bool togglesOnline = true;

    // Optional properties for specialized controls
    private bool adjustsPower = false;
    private float powerAdjustAmount = 10f;

    public void Interact(ulong netObjId)
    {
        // Find the player who interacted
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObjId, out NetworkObject playerObj))
        {
            if (togglesOnline)
            {
                // Request to toggle the system's Online state
                //targetSystem.RequestSystemToggleServerRpc(playerObj.OwnerClientId);
            }
            else if (adjustsPower)
            {
                // Request to adjust the system's power level
                //targetSystem.RequestAdjustPowerServerRpc(playerObj.OwnerClientId, powerAdjustAmount);
            }
        }
    }

    public void SetTargetSystem(AircraftSystem aircraftSystem)
    {
        targetSystem = aircraftSystem;
    }
}