using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class AircraftSystemButton :  MonoBehaviour, I_Interactable
{
    [Header("Events Triggered")] 
    [SerializeField]
    private SystemButton_GameEvent _interactionEvent;
    
    [Header("Settings")]
    [SerializeField] private bool _isButton;
    
    [Header("Non-Button Settings")]
    [SerializeField] private Vector3 _axisToRotate;
    [SerializeField] private float _maxRotationOnAxis;
    [SerializeField] private InputAction _look;
    [SerializeField] private GameObject _draggable;
    [SerializeField] private GameObject _base;

    private ulong _playerId;
    private float _val;
    private Vector3 _lookPoint;
    private bool isBeingUsed = false;
    private Plane plane; 
    public ulong PlayerId => _playerId; 
    public float Value => _val;
    
    public void Interact(ulong netObjId)
    {
        _playerId = netObjId;
        
        
        isBeingUsed = true;
        _interactionEvent?.TriggerEvent(this);
    }

    private void Update()
    {
        if (isBeingUsed)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //See where a ray from mouse position hits our invisible lever plane, and have lever look at that point
            if (Physics.Raycast(ray, out hit, 10))
            {
                Vector3 lookPosition = hit.point - transform.position;
                lookPosition.z = 0; //We can zero out one of the axis because the lever cannot move in all 3 axis
                Quaternion lookRotation = Quaternion.LookRotation(lookPosition);
                transform.rotation = lookRotation; 
            }

        }
    }

    public void RotateAircraftSystemButton(float deg)
    {
        //_handle.transform.localRotation = Quaternion.AngleAxis(_maxRotationOnAxis, _axisToRotate);
        //_handle.transform.localRotation = _look.
    }
}
