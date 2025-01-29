using System;
using System.Collections;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerRaycast : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Dictionary<GameObject, System.Action> _raycastInteractables;
    List<I_Interactable> _interactables;
    [SerializeField] private GameObject _cameraGameObj;
    private Camera _camera;
    private Ray _raycast;
    [SerializeField] NetworkBehaviour _localPlayer; 
    void Awake()
    {
        _interactables = new();
        _camera = _cameraGameObj.GetComponent<Camera>();
        //_localPlayer = transform.parent.GetComponent<NetworkBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_localPlayer.IsOwner)
        {
            return;
        }

        RaycastHit hit; 
        if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out hit, 1, 1 << 9))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                hit.collider.gameObject.GetComponent<I_Interactable>().Interact(_localPlayer.NetworkObjectId);
            }
        }
        Debug.DrawRay(_camera.transform.position, _camera.transform.forward * 1);
    }

    void InformListeners()
    {

    }

    public void Register(I_Interactable listener)
    {
        if (_interactables.Contains(listener))
        {
            return;
        }

        _interactables.Add(listener);
    }

    public bool Unregister(I_Interactable listener)
    {
        return _interactables.Remove(listener);
    }
}
