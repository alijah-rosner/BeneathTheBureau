using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextChanger : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] SceneChanger scene_;
    public string password;
    void Start()
    {
        password = scene_.GetJoinDetails();
        GetComponent<TextMeshPro>().text = password;
    }
}
