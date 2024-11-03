using System;

[Serializable]
public class ClientData
{
    public ulong ClientID;

    public int CharacterID = -1; 

    public ClientData(ulong clientID)
    {
        ClientID = clientID; 
    }
}
