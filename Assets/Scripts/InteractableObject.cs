using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractableObject : MonoBehaviour
{
    public bool inUse = false;

    public virtual void OnObjectHover()
    {
        //do something when hovered over
        HoverAction();
    }

    public virtual void OnObjectClicked()
    {
        //do something when clicked
        inUse = !inUse;
        ClickAction();
    }

    public virtual void OnLookAway()
    {
        //do something when looking away
        LookAwayAction();
    }

    public virtual void ClickAction()
    {

    }

    public virtual void HoverAction()
    {

    }

    public virtual void LookAwayAction()
    {

    }

    //raycast from camera control can be changed to OnTriggerEnter() to be owned by interactable object. state machine will handle function calls
}
