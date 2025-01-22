using UnityEngine;

public class PlaneButton : MonoBehaviour, I_Interactable
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
            foreach (var obj in _objectToEnable)
            {
                obj.GetComponent<Animator>().SetTrigger(_animationTriggerName);
            }
            IsOn = false; 
        }

    }
    public bool CanInteractWith()
    {
        return true; 
    }

    public void Interact()
    {
        Debug.Log("playing animation...");
        _animator.SetTrigger("InteractedWith"); 
        IsOn = !IsOn;
    }
}
