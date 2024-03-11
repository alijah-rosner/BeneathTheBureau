using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField]string nextScene_;
    [SerializeField] TestRelay relay_;
    public static string joinDetails;

    public void ChangeScenes()
    {
        if (relay_ != null)
        {
            joinDetails = relay_.joinCode_;
        }
        Debug.Log("Changing scenes...");
        SceneManager.LoadScene(nextScene_);
    }

    public void SetJoinDetails(string password)
    {
        joinDetails = password;
    }

    public string GetJoinDetails()
    {
        return joinDetails;
    }
}
