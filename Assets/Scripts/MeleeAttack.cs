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

    public void Start()
    {
        animation["MeleeAttack1"].wrapMode = WrapMode.PingPong;
    }

    public override float Range
    {
        get
        {
            return range;
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
        animation["MeleeAttack1"].speed = animation["MeleeAttack1"].length / castTime;

        if (castTimer > 0f)
        {
            castTimer -= Time.deltaTime;

            if (castTimer <= 0f)
            {
                cooldownTimer = cooldown;
                if (target.tag == "Hero")
                {
                    var heroCtrl = target.GetComponent<HeroController>();
                    heroCtrl.Hit(damage);
                }
            }
        }

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }
}
