using System;
using UnityEngine;

public interface I_Interactable
{
    //bool CanInteractWith();
    //void AddObserver(I_Observer observer);
   // void NotifyObservers();
   // void RemoveObserver(I_Observer observer); 
    void Interact();
}

public interface I_Observer
{
    bool OnInteract(); 
}