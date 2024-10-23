using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using TMPro;


public class MultiplayerDisplayOptions : MonoBehaviour
{
    [SerializeField] private string _nextScene = "Scene_CharacterSelect";
    [SerializeField] TMP_InputField _joinCodeInputField;

    public void StartHost()
    {
        HostManager.Instance.StartHost();
        //NetworkManager.Singleton.SceneManager.LoadScene(_nextScene, LoadSceneMode.Single);
    }

    //public void StartServer()
    //{
    //    HostManager.Instance.StartServer();
    //    NetworkManager.Singleton.SceneManager.LoadScene(_nextScene, LoadSceneMode.Single);

    //}

    public async void StartClient()
    {
        await ClientManager.Instance.StartClient(_joinCodeInputField.text);
    }
}
