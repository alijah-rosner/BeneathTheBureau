using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Characters/Character")]
public class SO_Character : ScriptableObject
{
    [SerializeField] private int _id = -1;
    [SerializeField] private string _displayName = "Display Name Not Set";
    [SerializeField] private string _characterBio = "Character Bio Not Set";
    [SerializeField] private Sprite _icon;
    [SerializeField] private GameObject _mesh; 

    [SerializeField] private NetworkObject _gameplayPrefab; 

    public int Id => _id;
    public string DisplayName => _displayName;
    public Sprite Icon => _icon;

    public GameObject Mesh => _mesh; 
    public string CharacterBio => _characterBio;

    public NetworkObject GameplayPrefab => _gameplayPrefab; 

}
