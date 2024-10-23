using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _lobbyNameText;
    [SerializeField] private TMP_Text _lobbyPlayerText;

    private Lobby _lobby;
    private LobbyList _lobbyList;
    public void Initialize(LobbyList lobbyList, Lobby lobby)
    {
        this._lobby = lobby;
        this._lobbyList = lobbyList;

        _lobbyNameText.text = lobby.Name;
        _lobbyPlayerText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";

    }

    public void Join()
    {
        _lobbyList.JoinAsync(_lobby);
    }

}
