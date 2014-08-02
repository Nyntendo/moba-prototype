using UnityEngine;
using System.Collections;

public abstract class BaseAttack : MonoBehaviour
{
    public abstract void Attack(GameObject target);
    public abstract void CancelAttack();

    public abstract float Range {get;}
    public abstract float CastTime {get;}
    public abstract bool IsOnCooldown {get;}
    public abstract bool IsAttacking {get;}
}

public abstract class BaseAbility : MonoBehaviour
{
    public abstract bool Activate();
    public abstract void CastAtTarget(Vector3 targetPosition, GameObject targetGameObject);
    public abstract void Cancel();

    public abstract float Range {get;}
    public abstract float CastTime {get;}
    public abstract bool IsOnCooldown {get;}
    public abstract bool IsCasting {get;}
}

public abstract class UnitSuperController : MonoBehaviour
{
    public abstract void OnHitServer(GameObject attacker);
    public abstract void OnHitClient();

    public abstract void OnDeathServer(GameObject attacker);
    public abstract void OnDeathClient(Vector3 position);

    public abstract void OnAbilityActivate(int ability);
    public abstract void OnAbilityCast(int ability);
    public abstract void OnAbilityCancel(int ability);
}
