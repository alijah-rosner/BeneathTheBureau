using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.UI;
public class JoinServer : MonoBehaviour
{
    // Start is called before the first frame update
    private InputField inputTextField_;
    [SerializeField]TestRelay relay_;
    public string codeToJoin_;
    void Start()
    {
        inputTextField_ = GetComponent<InputField>();
    }
    void Update()
    {
        
    }

    public void Join()
    {
        codeToJoin_ = inputTextField_.ToString();
        relay_.JoinRelay(inputTextField_.ToString());
    }
}
