using UnityEngine;

public class PropellerAnimation : MonoBehaviour
{
    [Header("Events To Listen For")]
    [SerializeField] private Float_GameEvent _onThrottleChangeEvent;

    private Float_GameEventListener _onThrottleChangeListener;
    private Animator _animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _animator = GetComponent<Animator>();
        _onThrottleChangeListener = new Float_GameEventListener(UpdateAnimationValues, _onThrottleChangeEvent);
    }

    private void UpdateAnimationValues(float v)
    {
        if (v > 0)
        {
            float val = v / 5;
            _animator.SetFloat("Thrust", val);
        }
        else if (v <= 0f)
        {
            _animator.SetFloat("Thrust", 0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
