using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class NET_CharacterSelectionDisplay : NetworkBehaviour
{
    [SerializeField] private SO_CharacterDatabase _characterDatabase;
    [SerializeField] private Transform _charactersHolder;
    [SerializeField] private NET_CharacterSelectButton _selectButtonPrefab;
    [SerializeField] private PlayerCard[] _playerCards;
    [SerializeField] private GameObject _characterInfoPanel;
    [SerializeField] private TMP_Text _characterNameText;
    [SerializeField] private TMP_Text _characterBioText;
    [SerializeField] private Transform _introSpawnPoint;
    [SerializeField] private Button _lockInButton;
    [SerializeField] private string _returnScene;


    private List<NET_CharacterSelectButton> _buttons = new();
    private NetworkList<NET_CharacterSelectState> _players;

    private void Awake()
    {
        _players = new();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            SO_Character[] allCharacters = _characterDatabase.GetAllCharacters();

            foreach (var character in allCharacters)
            {
                var selectButtonInstance = Instantiate(_selectButtonPrefab, _charactersHolder);
                selectButtonInstance.SetCharacter(this, character);
                _buttons.Add(selectButtonInstance);
            }

            _players.OnListChanged += HandlePlayersStateChanged;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                HandleClientConnected(client.ClientId);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            _players.OnListChanged -= HandlePlayersStateChanged;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        _players.Add(new NET_CharacterSelectState(clientId));
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        foreach (var player in _players)
        {
            if (player.ClientID == clientId)
            {
                _players.Remove(player);
                break;
            }
        }
    }

    public void Select(SO_Character character)
    {
        for (int i = 0; i < _players.Count; i++)
        {
            if (_players[i].ClientID != NetworkManager.Singleton.LocalClientId)
            {
                continue;
            }

            if (_players[i].IsLockedIn)
            {
                return;
            }

            if (_players[i].CharacterID == character.Id)
            {
                return;
            }

            if (IsCharacterTaken(character.Id, false))
            {
                return;
            }
        }

        _characterNameText.text = character.DisplayName;
        _characterBioText.text = character.CharacterBio;

        _characterInfoPanel.SetActive(true);

        SelectServerRpc(character.Id);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectServerRpc(int characterID, ServerRpcParams serverRpcParams = default)
    {
        for (int i = 0; i < _players.Count; i++)
        {
            if (_players[i].ClientID == serverRpcParams.Receive.SenderClientId)
            {
                if (!_characterDatabase.IsValidCharacterID(characterID))
                {
                    return;
                }

                if (IsCharacterTaken(characterID, true))
                {
                    return;
                }

                _players[i] = new NET_CharacterSelectState(
                    _players[i].ClientID, characterID, _players[i].IsLockedIn
                );
            }
        }
    }

    private void HandlePlayersStateChanged(NetworkListEvent<NET_CharacterSelectState> changeEvent)
    {
        for (int i = 0; i < _playerCards.Length; i++)
        {
            if (_players.Count > i)
            {
                _playerCards[i].UpdateDisplay(_players[i]);
            }
            else
            {
                _playerCards[i].DisableDisplay();
            }
        }

        foreach (var button in _buttons)
        {
            if (button.IsDisabled)
            {
                continue;
            }

            if (IsCharacterTaken(button.Character.Id, false))
            {
                button.SetDisabled();
            }
        }

        foreach (var player in _players)
        {
            if (player.ClientID != NetworkManager.Singleton.LocalClientId)
            {
                continue;
            }

            if (player.IsLockedIn)
            {
                _lockInButton.interactable = false;
                break;
            }

            if (IsCharacterTaken(player.CharacterID, false))
            {
                _lockInButton.interactable = false;
                break;
            }

            _lockInButton.interactable = true;
            break;
        }
    }

    private bool IsCharacterTaken(int characterID, bool checkAll)
    {
        for (int i = 0; i < _players.Count; i++)
        {
            if (!checkAll)
            {
                if (_players[i].ClientID == NetworkManager.Singleton.LocalClientId)
                {
                    continue;
                }
            }

            if (_players[i].IsLockedIn && _players[i].CharacterID == characterID)
            {
                return true;
            }
        }

        return false;
    }

    public void LockIn()
    {
        //HostManager.Instance.StartGame();

        LockInServerRPC();
    }

    [ServerRpc(RequireOwnership = false)]
    private void LockInServerRPC(ServerRpcParams serverRpcParams = default)
    {
        for (int i = 0; i < _players.Count; i++)
        {
            if (_players[i].ClientID == serverRpcParams.Receive.SenderClientId)
            {
                if (!_characterDatabase.IsValidCharacterID(_players[i].CharacterID))
                {
                    return;
                }

                if (IsCharacterTaken(_players[i].CharacterID, true))
                {
                    return;
                }

                _players[i] = new NET_CharacterSelectState(
                    _players[i].ClientID, _players[i].CharacterID, true
                );
            }
        }

        foreach (var player in _players)
        {
            if (!player.IsLockedIn)
            {
                return;
            }
        }

        foreach (var player in _players)
        {
            HostManager.Instance.SetCharacter(player.ClientID, player.CharacterID);
        }

        HostManager.Instance.StartGame();
    }

    public async void QuitLobby()
    {
        var lobbies = await Lobbies.Instance.QueryLobbiesAsync(new QueryLobbiesOptions());
        if (IsServer)
        {
            HostManager.Instance.Shutdown();
            ReturnPlayersToTitleRpc();
        }
        else
        {
            
            string playerId = AuthenticationService.Instance.PlayerId;
            await LobbyService.Instance.RemovePlayerAsync(lobbies.Results[0].Id, playerId);
            
        } 
        AuthenticationService.Instance.SignOut(true);
        NetworkManager.Shutdown();
        SceneManager.LoadScene(_returnScene);
    }

    [Rpc(SendTo.Everyone)]
    private void ReturnPlayersToTitleRpc()
    {
        AuthenticationService.Instance.SignOut(true);
        NetworkManager.Shutdown();
        SceneManager.LoadScene(_returnScene);
    }
    
}