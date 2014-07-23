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

    public abstract bool IsOnCooldown
    {
        get;
    }

    public abstract bool IsAttacking
    {
        get;
    }
}
