using Unity.Netcode;
using UnityEngine;

public class PlaneButton : NetworkBehaviour, I_Interactable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool IsOn;
    [SerializeField] private GameObject[] _objectToEnable;
    private Animator _animator;
    [SerializeField] string _animationTriggerName;



    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_objectToEnable.Length == 0)
        {
            return;
        }

        if (IsOn)
        {
            if (IsOwner)
            {
                UpdateAnimationsRpc();
                IsOn = false;
            }
        }

    }

    [Rpc(SendTo.Everyone)]
    void UpdateAnimationsRpc()
    {
        foreach (var obj in _objectToEnable)
        {
            obj.GetComponent<Animator>().SetTrigger(_animationTriggerName);
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
        IsOn = !IsOn;
    }
}
