using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Data Object", menuName = "DamageData/Data")]
public class SO_DamageData : ScriptableObject
{
    public GameObject Source;
    public GameObject Target;
    public float Amount;
    public DamageType Type;
    public float Duration;
    public DamageTag Tag; 
  //  public Vector3 DirectionVector;
   // public Vector3 ImpactPoint; 


}

public enum DamageTag
{
    Default,
    Stun,
    Burn,
    Insanity
}
