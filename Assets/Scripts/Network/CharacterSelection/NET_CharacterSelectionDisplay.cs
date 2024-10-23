using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class NET_CharacterSelectionDisplay : NetworkBehaviour
{
    [SerializeField] private SO_CharacterDatabase _characterDatabase;
    [SerializeField] private Transform _charactersHolder;
    [SerializeField] private NET_CharacterSelectButton _selectButtonPrefab;
    [SerializeField] private PlayerCard[] _playerCards;
    [SerializeField] private GameObject _characterInfoPanel;
    [SerializeField] private TMP_Text _characterNameText;
    [SerializeField] private TMP_Text _characterBioText;

    private NetworkList<NET_CharacterSelectState> _players;
    private List<NET_CharacterSelectButton> _buttons = new();

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
                _players[i] = new NET_CharacterSelectState(
                    _players[i].ClientID, characterID
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
    }
}
