using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character Database", menuName = "Characters/Database")]
public class SO_CharacterDatabase : ScriptableObject
{
    [SerializeField] private SO_Character[] _characters = new SO_Character[0];

    public SO_Character[] GetAllCharacters() => _characters;

    public SO_Character GetCharacterByID(int id)
    {
        foreach (var character in _characters)
        {
            if (character.Id == id)
            {
                return character;
            }
        }

        return null;
    }
}
