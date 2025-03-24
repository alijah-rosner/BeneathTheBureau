using Unity.Netcode;
using UnityEngine;

public class PlaneButton : MonoBehaviour, I_Interactable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public enum ButtonState
    {
        ToggledOn,
        ToggledOff
    }

    [SerializeField] private GameObject[] _objectToEnable;
    private Animator _animator;
    [SerializeField] string _animationTriggerName;
    private ButtonState _state;
    private bool _performingAction = false; 


    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _state = ButtonState.ToggledOff;
    }

    // Update is called once per frame
    void Update()
    {
        //if (_objectToEnable.Length == 0)
        //{
        //    return;
        //}

        //if (_state == ButtonState.ToggledOn)
        //{
        //    if (IsOwner)
        //    {
        //        if (_performingAction)
        //        {
        //            UpdateAnimationsRpc("Off");
        //            _state = ButtonState.ToggledOff;
        //            _performingAction = false;
        //        }
        //    }
        //}
        //if (_state == ButtonState.ToggledOff)
        //{
        //    if (IsOwner)
        //    {
        //        if (_performingAction)
        //        {
        //            UpdateAnimationsRpc("On");
        //            _state = ButtonState.ToggledOn;
        //            _performingAction = false;
        //        }
        //    }
        //}

    }

    [Rpc(SendTo.Everyone)]
    void UpdateAnimationsRpc(string triggerWord)
    {
        foreach (var obj in _objectToEnable)
        {
            obj.GetComponent<Animator>().SetTrigger(triggerWord);
        }
    }

    public bool CanInteractWith()
    {
        return true;
    }

    public void Interact(ulong netObjId)
    {
        Debug.Log("playing animation...");
        _animator.SetTrigger("InteractedWith");
        _performingAction = true;
    }
}
