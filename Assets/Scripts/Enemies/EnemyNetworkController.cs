using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyNetworkController : NetworkBehaviour
{
    [SerializeField] EnemyAIController m_Brain;
    [SerializeField] private float MoveSpeed;
    public override void OnNetworkSpawn()
    {
        if (!IsServer) { enabled = false; return; }
        m_Brain.EnableAI();
        //base.OnNetworkSpawn();

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
