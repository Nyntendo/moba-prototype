using UnityEngine;
using System.Collections;

public abstract class BaseAttack : MonoBehaviour
{
    public abstract void Attack(GameObject target);

    public abstract void CancelAttack();

    public abstract float Range
    {
        get;
    }

    public abstract float CastTime
    {
        get;
    }

    public abstract bool IsOnCooldown
    {
        get;
    }

    public abstract bool IsAttacking
    {
        get;
    }
}

public abstract class UnitSuperController : MonoBehaviour
{
    public abstract void OnHitServer(GameObject attacker);
    public abstract void OnHitClient();

    public abstract void OnDeathServer(GameObject attacker);
    public abstract void OnDeathClient(Vector3 position);
}
