using UnityEngine;
using Unity.Netcode;
using TMPro;

public class SystemValueText : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private TMP_Text _text;

    [SerializeField] private Float_GameEvent _onValueChanged;
    [SerializeField] private string _titleText; 
    private Float_GameEventListener _listener;
    private NetworkVariable<float> _systemVal = new();


    void Initialize()
    {
        _listener = new Float_GameEventListener(UpdateText);
        _onValueChanged.RegisterListener(_listener);
    }

    void Start()
    {
        _listener = new Float_GameEventListener(UpdateText, _onValueChanged);
        _onValueChanged.RegisterListener(_listener);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void UpdateText(float v)
    {
        _text.text = _titleText + " " + v.ToString();
    }
}