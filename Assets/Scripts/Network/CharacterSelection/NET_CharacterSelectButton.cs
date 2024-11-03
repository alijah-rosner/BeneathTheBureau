using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NET_CharacterSelectButton : MonoBehaviour
{

    [SerializeField] private Image _iconImage;
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _disabledOverlay; 

    private NET_CharacterSelectionDisplay _characterSelect;

    public SO_Character Character { get; private set; }

    public bool IsDisabled { get; private set; }

    public void SetCharacter(NET_CharacterSelectionDisplay characterSelect, SO_Character character)
    {
        _iconImage.sprite = character.Icon;

        _characterSelect = characterSelect;
        Character = character;
    }

    public void SelectCharacter()
    {
        _characterSelect.Select(Character);
    }

    public void SetDisabled()
    {
        IsDisabled = true; 
        _disabledOverlay.SetActive(true);
        _button.interactable = false;
    }
}
