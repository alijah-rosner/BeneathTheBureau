using UnityEngine;

public class GameEventTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private bool _triggerOnce;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


public enum TriggerTypes
{
    OnValueChanged,
    OnTriggerEnter,
    OnTriggerExit,
    OnTriggerStay,
    On
}