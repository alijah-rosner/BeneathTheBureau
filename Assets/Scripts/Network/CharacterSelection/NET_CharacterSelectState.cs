using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct NET_CharacterSelectState : INetworkSerializable, IEquatable<NET_CharacterSelectState>
{
    public ulong ClientID;
    public int CharacterID;

    public NET_CharacterSelectState(ulong clientID, int characterID = -1)
    {
        ClientID = clientID;
        CharacterID = characterID;
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientID);
        serializer.SerializeValue(ref CharacterID);
    }
    public bool Equals(NET_CharacterSelectState other)
    {
        return (ClientID == other.ClientID && CharacterID == other.CharacterID);
    }
}
