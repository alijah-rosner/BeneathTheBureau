using UnityEngine;
using Unity.Netcode;
using NUnit.Framework;
using System.Collections.Generic;

public class FlightSystemsHandler : MonoBehaviour
{
    private Dictionary<string, AircraftSystem> _systemsList = new Dictionary<string, AircraftSystem>();
    [Header("")]
    [SerializeField] private float _electricityThroughput;
    [SerializeField] private float _fungalOutput;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        _electricityThroughput = _systemsList["ElectricalPowerSystem"].resourceSetting.Output;
        _fungalOutput = _systemsList["FungusReactorSystem"].resourceSetting.Output;
    }

    public void AddSystem(string sysName, AircraftSystem newSys)
    {
        _systemsList.Add(sysName, newSys.GetComponent<AircraftSystem>());
        Debug.Log("Added new system: " + sysName);
    }

    public Dictionary<string, AircraftSystem> GetFlightSystemList()
    {
        return _systemsList;
    }

    public float GetElectricityThroughput()
    {
        return _electricityThroughput;
    }
}
