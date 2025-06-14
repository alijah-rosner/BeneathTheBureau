using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Behavior;

public class EnemyAIController : MonoBehaviour
{
    [SerializeField] private BehaviorGraphAgent m_Agent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_Agent.enabled = false;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Should only be turned on by host's instance.
    public void EnableAI()
    {
        m_Agent.enabled = true;
    }

}
