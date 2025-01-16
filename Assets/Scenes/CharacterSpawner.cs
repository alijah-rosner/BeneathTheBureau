using System.Collections;
using System.Collections.Generic;
using Unity.Netcode; 
using UnityEngine;

public class CharacterSpawner : NetworkBehaviour
{
    [SerializeField] private SO_CharacterDatabase _dataBase; 
    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }

        foreach (var client in HostManager.Instance.ClientData)
        {
            var character = _dataBase.GetCharacterByID(client.Value.CharacterID);

            if (character != null)
            {
                var spawnPos = transform.position + new Vector3(Random.Range(-1f, 1f), 3f, Random.Range(-1f, 1f));
                var characterInstance = Instantiate(character.GameplayPrefab, spawnPos, Quaternion.identity);
                characterInstance.SpawnAsPlayerObject(client.Value.ClientID);
            }
        }
    }
}

