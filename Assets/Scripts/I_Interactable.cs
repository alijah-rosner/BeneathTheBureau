using System;
using UnityEngine;

public interface I_Interactable
{
    //bool CanInteractWith();
    //void AddObserver(I_Observer observer);
   // void NotifyObservers();
   // void RemoveObserver(I_Observer observer); 
    //void Interact();
    void Interact(ulong netObjId);
}

public interface I_Observer
{
    bool OnInteract(); 
}