using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
public class NetworkObjectSpawner : MonoBehaviour
{
    public NetworkObject prefabReference;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      
    }

    public void Awake()
    {
        if (NetworkManager.Singleton && NetworkManager.Singleton.IsServer &&
            NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManagerOnOnLoadEventCompleted;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton && NetworkManager.Singleton.IsServer &&
            NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneManagerOnOnLoadEventCompleted;
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }


    void SceneManagerOnOnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        SpawnNetworkObject();
        Destroy(gameObject);
    }

    void SpawnNetworkObject()
    {
        var instantiatedNetObj = Instantiate(prefabReference, transform.position, transform.rotation, null);
        SceneManager.MoveGameObjectToScene(instantiatedNetObj.gameObject,
    SceneManager.GetSceneByName(gameObject.scene.name));
        //instantiatedNetObj.transform.localScale = transform.lossyScale * prefabReference.transform.localScale;
        instantiatedNetObj.Spawn(destroyWithScene: true);
    }
}
