using UnityEngine;
using System.Collections;

public class UnitAnimationController : MonoBehaviour
{
    public enum UnitAnimationState
    {
        Idle,
        CombatIdle,
        Running,
        Flying,
        Falling,
        Dead
    }

    public string idleAnimation = "Idle";
    public string runAnimation = "Running";
    public string combatIdleAnimation = "BattleIdle";
    public string fallAnimation = "Falling";
    public string flyAnimation = "Upwards";
    public string jumpAnimation = "JumpStart";
    public string landAnimation = "Landing";
    public string deathAnimation = "Death";
    public string attackAnimation;
    public string[] abilityAnimations;

    public float speed = 0f;
    public float ySpeed = 0f;
    public bool jump = false;
    public bool attack = false;
    public bool die = false;
    public bool revive = false;
    public bool inCombat = false;

    public int castAbility = -1;

    public float runThreshold = 0.2f;
    public float flyThreshold = 0.2f;
    public float fallThreshold = -0.2f;

    public float animationFadeTime = 0.2f;

    public UnitAnimationState currentState = UnitAnimationState.Idle;

    public void Start()
    {
        animation[idleAnimation].wrapMode = WrapMode.Loop;
        animation[idleAnimation].layer = 0;
        animation[runAnimation].wrapMode = WrapMode.Loop;
        animation[runAnimation].layer = 0;
        animation[combatIdleAnimation].wrapMode = WrapMode.Loop;
        animation[combatIdleAnimation].layer = 0;
        animation[deathAnimation].wrapMode = WrapMode.ClampForever;
        animation[deathAnimation].layer = 0;
        animation[attackAnimation].wrapMode = WrapMode.Once;
        animation[attackAnimation].layer = 1;

        // jumping
        if (!string.IsNullOrEmpty(fallAnimation))
        {
            animation[fallAnimation].wrapMode = WrapMode.Loop;
            animation[fallAnimation].layer = 0;
        }
        if (!string.IsNullOrEmpty(flyAnimation))
        {
            animation[flyAnimation].wrapMode = WrapMode.Loop;
            animation[flyAnimation].layer = 0;
        }
        if (!string.IsNullOrEmpty(jumpAnimation))
        {
            animation[jumpAnimation].wrapMode = WrapMode.Once;
            animation[jumpAnimation].layer = 1;
        }
        if (!string.IsNullOrEmpty(landAnimation))
        {
            animation[landAnimation].wrapMode = WrapMode.Once;
            animation[landAnimation].layer = 1;
        }

        // abilities
        for (int i = 0; i < abilityAnimations.Length; i++)
        {
            animation[abilityAnimations[i]].wrapMode = WrapMode.Once;
            animation[abilityAnimations[i]].layer = 1;
        }
    }

    public void SetAttackCastTime(float castTime)
    {
        animation[attackAnimation].speed = animation[attackAnimation].length / castTime;
    }

    public void LateUpdate()
    {
        switch (currentState)
        {
            case UnitAnimationState.Idle:
                animation.CrossFade(idleAnimation, animationFadeTime);
                if (speed > runThreshold)
                {
                    currentState = UnitAnimationState.Running;
                }
                else if (inCombat)
                {
                    currentState = UnitAnimationState.CombatIdle; 
                }
                break;
            case UnitAnimationState.CombatIdle:
                animation.CrossFade(combatIdleAnimation, animationFadeTime);
                if (speed > runThreshold)
                {
                    currentState = UnitAnimationState.Running;
                }
                else if (!inCombat)
                {
                    currentState = UnitAnimationState.Idle; 
                }
                break;
            case UnitAnimationState.Running:
                animation.CrossFade(runAnimation, animationFadeTime);
                if (speed < runThreshold)
                {
                    if (inCombat)
                    {
                        currentState = UnitAnimationState.CombatIdle;
                    }
                    else
                    {
                        currentState = UnitAnimationState.Idle;
                    }
                }
                break;
            case UnitAnimationState.Flying:
                animation.CrossFade(flyAnimation, animationFadeTime);
                if (ySpeed < fallThreshold)
                {
                    currentState = UnitAnimationState.Falling;
                }
                break;
            case UnitAnimationState.Falling:
                animation.CrossFade(fallAnimation, animationFadeTime);
                if (ySpeed > fallThreshold)
                {
                    currentState = UnitAnimationState.Idle;
                    animation.CrossFade(landAnimation, animationFadeTime);
                }
                break;
            default:
                //do nothin'
                break;
        }

        if (die)
        {
            die = false;
            currentState = UnitAnimationState.Dead;
            animation.CrossFade(deathAnimation, animationFadeTime);
        }
        if (revive)
        {
            revive = false;
            currentState = UnitAnimationState.Idle;
        }
        if (attack)
        {
            attack = false;
            animation.CrossFade(attackAnimation, animationFadeTime);
        }
        if (jump)
        {
            jump = false;
            currentState = UnitAnimationState.Flying;
            animation.CrossFade(jumpAnimation, animationFadeTime);
        }
        if (castAbility >= 0)
        {
            animation.CrossFade(abilityAnimations[castAbility], animationFadeTime);
            castAbility = -1;
        }
    }
}
