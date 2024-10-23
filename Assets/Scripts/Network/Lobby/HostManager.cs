using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Unity.Netcode;
using Unity.Networking;
using UnityEngine.SocialPlatforms.Impl;
using Unity.Netcode.Transports.UTP;

public class HostManager : MonoBehaviour
{
    // Start is called before the first frame update
    private ConcurrentQueue<string> _createdLobbyIds = new ConcurrentQueue<string>();
    public Lobby CurrentLobby => _currentLobby;
    private Lobby _currentLobby;
    public string JoinCode;
    private string _lobbyId;
    LobbyEventCallbacks _lobbyEventCallbacks = new LobbyEventCallbacks();

    private Task _heartbeatTask;
    private static HostManager _hostManagerSingleton;

    public static HostManager Instance
    {
        get
        {
            if (_hostManagerSingleton != null) return _hostManagerSingleton;

            _hostManagerSingleton = FindObjectOfType<HostManager>();

            if (_hostManagerSingleton == null)
            {
                Debug.LogError("No HostSingleton in scene, did you run this from the bootStrap scene?");
                return null;
            }

            return _hostManagerSingleton;
        }
    }

    private async void Start()
    {
        DontDestroyOnLoad(gameObject);
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in:" + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void OnDestroy()
    {
        Shutdown();
    }

    public void StartHost()
    {

    }

    public async Task<Lobby> CreateLobby(string name, bool isPrivate, string password = null, int maxPlayers = 4)
    {
        Allocation allocation;
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
        }
        catch (Exception e)
        {
            Debug.LogError($"Relay create allocation request failed {e.Message}");
            throw;
        }
        try
        {
            JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }
        catch
        {
            Debug.LogError("Relay get join code request failed");
            throw;
        }
        var relayServerData = new RelayServerData(allocation, "dtls");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnServerStarted += OnNetworkReady;

        try
        {
            //Create lobby and also make a player for our host
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = isPrivate;
            options.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: JoinCode
                    )
                }
            };
            _currentLobby = await LobbyService.Instance.CreateLobbyAsync(name, maxPlayers, options);
            _lobbyId = _currentLobby.Id;
            StartCoroutine(HeartbeatLobbyCoroutine(_currentLobby.Id, 15));
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }

        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("Scene_CharacterSelection", UnityEngine.SceneManagement.LoadSceneMode.Single);

        return _currentLobby;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {

    }

    private void OnNetworkReady()
    {

    }

    private IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    private void OnApplicationQuit()
    {
        while (_createdLobbyIds.TryDequeue(out var lobbyId))
        {
            LobbyService.Instance.DeleteLobbyAsync(lobbyId);
        }
    }

    public async void Shutdown()
    {
        StopCoroutine(nameof(HeartbeatLobbyCoroutine));

        if (string.IsNullOrEmpty(_lobbyId)) { return; }

        try
        {
            await Lobbies.Instance.DeleteLobbyAsync(_lobbyId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

        _lobbyId = string.Empty;

        //NetworkServer.OnClientLeft -= OnClientDisconnect;

        //NetworkServer?.Dispose();
    }


}
