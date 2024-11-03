using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct NET_CharacterSelectState : INetworkSerializable, IEquatable<NET_CharacterSelectState>
{
    public ulong ClientID;
    public int CharacterID;
    public bool IsLockedIn;

    public NET_CharacterSelectState(ulong clientID, int characterID = -1, bool isLockedIn = false)
    {
        ClientID = clientID;
        CharacterID = characterID;
        IsLockedIn = isLockedIn;
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientID);
        serializer.SerializeValue(ref CharacterID);
        serializer.SerializeValue(ref IsLockedIn);
    }

    public bool Equals(NET_CharacterSelectState other)
    {
        return (ClientID == other.ClientID 
            && CharacterID == other.CharacterID
            && IsLockedIn == other.IsLockedIn);
    }
}
