using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;
using Unity.Networking;
using UnityEngine.UI;
using TMPro;

public class CreateLobbyButton : MonoBehaviour
{
    public string LobbyName;
    public string Password;
    [SerializeField] private Button _startButton;
    [SerializeField] private TMP_InputField _passwordTextObj;
    [SerializeField] private TMP_InputField _lobbyNameObj;
    [SerializeField] private Toggle _togglePrivate;

    protected HostManager Manager
    {
        get
        {
            if (_lobbyManager != null) return _lobbyManager;
            return _lobbyManager = HostManager.Instance;
        }
    }

    HostManager _lobbyManager;


    bool _isPrivate = false; 

    private void Awake()
    {
        _startButton.interactable = false;
    }

    public void SetLobbyName()
    {
        LobbyName = _lobbyNameObj.text;
        //when setting name, if name and password are set (or if its set to public)
            // make the Start Game button work
        _startButton.interactable = true;
    }

    public void SetLobbyPrivate()
    {
        _isPrivate = _togglePrivate.isOn;
    }

    private void CreatePublicLobby()
    {
        CreateLobbyOptions options = new CreateLobbyOptions();
        options.IsPrivate = false;

        //Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(_lobbyName, 4, options);
    }

    public void SetServerPassword()
    {
        if (string.IsNullOrWhiteSpace(_passwordTextObj.text))
        {
            _passwordTextObj.text = null;
        }
        Password = _passwordTextObj.text;
        // same thing down here
    }

    public async void OnCreatePressed()
    {
        await Manager.CreateLobby(LobbyName, _isPrivate, Password);
    }

    private bool ValidateServerName()
    {
        return LobbyName.Length is > 0 and <= 64;
    }
}
