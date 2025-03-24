using UnityEngine;

public interface I_Damageable
{
    void TakeDamage(SO_DamageData data);
}

public enum DamageType
{
    Structural,
    Electrical,
    Fire,
    Explosion
}