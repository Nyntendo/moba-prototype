using UnityEngine;
using System.Collections;

public class RangedAttack : BaseAttack {

    public float castTime = 1f;
    public float cooldown = 1f;
    public float damage = 10f;
    public float range = 50f;
    public float projectileSpeed = 100f;
    public Vector3 projectileSpawnOffset = Vector3.zero;
    public GameObject projectile;

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
                Debug.Log("Launch projectile");
                var projObj = (GameObject)Network.Instantiate(projectile, transform.position + transform.TransformDirection(projectileSpawnOffset), transform.rotation, 0);
                var projCtrl = projObj.GetComponent<ProjectileController>();
                projCtrl.targetGameObject = target;
                projObj.networkView.RPC("SetSpeed", RPCMode.AllBuffered, projectileSpeed);
                projObj.networkView.RPC("SetDamage", RPCMode.AllBuffered, damage);
                projCtrl.attacker = gameObject;
            }
        }

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
	}
}
