using UnityEngine;
using System.Collections;

public class MeleeAttack : BaseAttack {

    public float castTime = 1f;
    public float cooldown = 1f;
    public float damage = 10f;
    public float range = 10f;
    public string animationName = "MeleeAttack1";
    private float castTimer = 0f;
    private float cooldownTimer = 0f;
    private GameObject target;

    public void Start()
    {
        animation[animationName].wrapMode = WrapMode.PingPong;
    }

    public override float Range
    {
        get
        {
            return range;
        }
    }

    public override string Animation
    {
        get
        {
            return animationName;
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
        animation[animationName].speed = animation[animationName].length / castTime;

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
                else if (target.tag == "Creep")
                {
                    var creepCtrl = target.GetComponent<CreepController>();
                    creepCtrl.Hit(damage, gameObject);
                }
            }
        }

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }
}
