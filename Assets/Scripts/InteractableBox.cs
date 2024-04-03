using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableBox : InteractableObject
{
    public Material color;
    public GameObject textDisplay;
    
    public override void ClickAction()
    {
        GetComponent<MeshRenderer>().material = color;
    }

    public override void HoverAction()
    {
        textDisplay.SetActive(true);
    }

    public override void LookAwayAction()
    {
        textDisplay.SetActive(false);
        GetComponent<MeshRenderer>().material = default;
    }


}
