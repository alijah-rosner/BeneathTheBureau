using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    public Vector2 move;
    public Vector2 look;
    public bool jump;
    
    private InputSystems_NetworkActions inputActions;
    
    private void Awake()
    {
        inputActions = new InputSystems_NetworkActions();
    }
    
    private void OnEnable()
    {
        inputActions.Player.Enable();
        
        // Subscribe to jump
        inputActions.Player.Jump.performed += ctx => jump = true;
        inputActions.Player.Jump.canceled += ctx => jump = false;
    }
    
    private void OnDisable()
    {
        inputActions.Player.Disable();
    }
    
    private void Update()
    {
        // Update values every frame
        move = inputActions.Player.Movement.ReadValue<Vector2>();
        look = inputActions.Player.Look.ReadValue<Vector2>();
    }
}
