using UnityEngine;
using System.Collections;

public class CreepController : UnitSuperController {

    public AudioClip hitSound;
    public float volume = 2f;
    public UnitController unitController;
    public float animationFade = 0.2f;
    public string attackAnimation = "MeleeAttack";

    public void Start()
    {
        animation[attackAnimation].wrapMode = WrapMode.PingPong;
        animation["Dead"].wrapMode = WrapMode.Once;
        animation["Idle"].wrapMode = WrapMode.Loop;
        animation[attackAnimation].speed = animation[attackAnimation].length / unitController.baseAttack.CastTime;
    }

    public override void OnHitServer(GameObject attacker)
    {
        unitController.targetGameObject = attacker;
    }

    public override void OnHitClient()
    {
        AudioSource.PlayClipAtPoint(hitSound, transform.position, volume);
    }

    public override void OnDeathServer(GameObject attacker)
    {

    }

    public override void OnDeathClient(Vector3 position)
    {

    }

    public override void OnAbilityActivate(int ability)
    {
    }

    public override void OnAbilityCast(int ability)
    {
    }

    public override void OnAbilityCancel(int ability)
    {
    }

    public void LateUpdate()
    {
        if (unitController.lastAnimationState != unitController.animationState)
        {
            switch (unitController.animationState)
            {
                case UnitAnimationState.Attacking:
                    animation.CrossFade(attackAnimation, animationFade);
                    break;
                case UnitAnimationState.Running:
                    animation.CrossFade("Walking", animationFade);
                    break;
                case UnitAnimationState.Dead:
                    unitController.lastAnimationState = UnitAnimationState.Dead;
                    animation.CrossFade("Dead", animationFade);
                    break;
                case UnitAnimationState.Idle:
                default:
                    animation.CrossFade("Idle", animationFade);
                    break;
            }
        }
    }
}
