using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Behavior;
using UnityEngine.SceneManagement;
using Unity.Netcode;
public class EnemyAIController : MonoBehaviour
{
    [SerializeField] private BehaviorGraphAgent m_Agent;
    private BlackboardReference m_AgentBlackboard;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    private void InitAgent()
    {
        m_Agent.transform.parent = GameObject.FindGameObjectWithTag("SimulatedAircraft").transform;
        List<GameObject> turbList = new List<GameObject>(GameObject.FindGameObjectsWithTag("Engine"));

        turbList.RemoveAll(go => IsChildOf(go.transform, "Aircraft"));

        m_AgentBlackboard.SetVariableValue<List<GameObject>>("Turbines", turbList);

    }

    bool IsChildOf(Transform child, string parentName)
    {
        Transform current = child.parent;
        while (current != null)
        {
            if (current.tag == parentName)
                return true;
            current = current.parent;
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Should only be turned on by host's instance.
    public void EnableAI()
    {
        m_Agent.enabled = true;
        m_AgentBlackboard = m_Agent.BlackboardReference;
        InitAgent();
    }

}
