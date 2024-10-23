using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NET_CharacterSelectButton : MonoBehaviour
{

    [SerializeField] private Image _iconImage;

    private NET_CharacterSelectionDisplay _characterSelect;

    private SO_Character _character;

    public void SetCharacter(NET_CharacterSelectionDisplay characterSelect, SO_Character character)
    {
        _iconImage.sprite = character.Icon;

        _characterSelect = characterSelect;
        _character = character;
    }

    public void SelectCharacter()
    {
        _characterSelect.Select(_character);
    }
}
