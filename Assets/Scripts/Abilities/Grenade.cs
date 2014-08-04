using UnityEngine;
using System.Collections;

public class Grenade : BaseAbility {

    public float range;
    public float damage = 30f;
    public float castTime;
    public float castTimer = 0f;
    public float cooldown;
    public float cooldownTimer = 0f;
    public Object projectile;
    public Vector3 launchForce;
    public Vector3 launchOffset;
    public Vector3 target;

    public void Update()
    {
        if (castTimer > 0f)
        {
            castTimer -= Time.deltaTime;

            if (castTimer <= 0f)
            {
                cooldownTimer = cooldown;
                if (Network.isServer)
                {
                    var grenade = (GameObject)Network.Instantiate(projectile, transform.position + transform.TransformDirection(launchOffset), Quaternion.identity, 0);
                    var grenadeController = grenade.GetComponent<GrenadeProjectileController>();
                    grenadeController.SetDamage(damage);
                    grenadeController.SetAttacker(gameObject);
                    grenade.networkView.RPC("Throw", RPCMode.AllBuffered, transform.TransformDirection(launchForce));
                }
            }
        }

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    public override bool Activate()
    {
        Debug.Log("Ability activated");

        return true;
    }

    public override void CastAtTarget(Vector3 targetPosition, GameObject targetGameObject)
    {
        Debug.Log("Ability target set to " + targetPosition);
        target = targetPosition;
        castTimer = castTime;
    }
    public override void Cancel()
    {
        castTimer = 0f;
    }

    public override float Range
    {
        get
        {
            return range;
        }
    }

    public override float CastTime
    {
        get
        {
            return castTime;
        }
    }

    public override bool IsOnCooldown
    {
        get
        {
            return cooldownTimer > 0f;
        }
    }
    public override bool IsCasting
    {
        get
        {
            return castTimer > 0f;
        }
    }
}
