using UnityEngine;
using System.Collections;

public class MeleeAttack : BaseAttack {

    public float castTime = 1f;
    public float cooldown = 1f;
    public float damage = 10f;
    public float range = 10f;
    private float castTimer = 0f;
    private float cooldownTimer = 0f;
    private GameObject target;

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

    public override bool IsAttacking
    {
        get
        {
            return castTimer > 0f;
        }
    }

    public override bool IsOnCooldown
    {
        get
        {
            return cooldownTimer > 0f;
        }
    }

    public override void Attack(GameObject target)
    {
        castTimer = castTime;
        this.target = target;
    }

    public override void CancelAttack()
    {
        castTimer = 0f;
    }
    
    void Update ()
    {

        if (castTimer > 0f)
        {
            castTimer -= Time.deltaTime;

            if (castTimer <= 0f)
            {
                cooldownTimer = cooldown;
                var targetUnitController = target.GetComponent<UnitController>();
                targetUnitController.Hit(damage, gameObject);
            }
        }

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }
}
