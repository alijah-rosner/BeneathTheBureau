using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCard : MonoBehaviour
{
    [SerializeField] private SO_CharacterDatabase _characterDatabase;
    [SerializeField] private GameObject _visuals;
    [SerializeField] private Image _characterIconImage;
    [SerializeField] private TMP_Text _playerNameText;
    [SerializeField] private TMP_Text _characterNameText;
    [SerializeField] private TMP_Text _characterBio;

    public void UpdateDisplay(NET_CharacterSelectState state)
    {
        if (state.CharacterID != -1)
        {
            var character = _characterDatabase.GetCharacterByID(state.CharacterID);
            _characterIconImage.sprite = character.Icon;
            _characterIconImage.enabled = true;
            _characterNameText.text = character.DisplayName;
            _characterBio.text = character.CharacterBio;
        }
        else
        {
            _characterIconImage.enabled = false; 
        }
        _playerNameText.text = $"Player {state.ClientID}";
        _visuals.SetActive(true);
    }

    public void DisableDisplay()
    {
        _visuals.SetActive(false);
    }
}
