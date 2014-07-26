﻿using UnityEngine;
using System.Collections;

public enum UnitAnimationState
{
    Idle = 0,
    Running = 1,
    Attacking = 2,
    BeginJump = 3,
    Jumping = 4,
    Falling = 5,
    Landing = 6,
    Dead = 7,
    TPose = 8
}

public class UnitController : MonoBehaviour {

    public float speed = 30.0F;
    public float jumpSpeed = 60F;
    public float gravity = 80.0F;
    public float maxHealth;
    public float health;
    public bool dead = false;
    public bool following = false;
    public bool jump = false;
    public bool jumping = false;

    public UnitAnimationState lastAnimationState = UnitAnimationState.TPose;
    public UnitAnimationState animationState = UnitAnimationState.Idle;

    public float posErrorThreshold = 2f;
    public float rotErrorThreshold = 2f;
    public float targetReachedThreshold = 2f;
    public float moveAnimationThreshold = 0.1f;
    public float fallAnimationThreshold = 0.1f;

    private bool lastIsGrounded = true;
    private Vector3 lastPosition;
    private Vector3 movement = Vector3.zero;
    public Vector3 target = Vector3.zero;
    public GameObject targetGameObject;
    public Vector3 serverPos = Vector3.zero;
    public Quaternion serverRot = Quaternion.identity;
    public bool serverIsAttacking = false;

    public BaseAttack baseAttack;

    public Texture2D healthbarBG;
    public Texture2D healthbarFG;

    public Vector3 healthbarOffset;
    public Vector2 healthbarSize;
    public float heathbarFrameSize;

    private CharacterController charController;
    public UnitSuperController superController;

	void Start ()
    {
        charController = GetComponent<CharacterController>();
        lastPosition = transform.position;
        serverPos = transform.position;
        serverRot = transform.rotation;
	}
	
    public void OnGUI()
    {
        if (!dead)
        {
            var screenPos = Camera.main.WorldToScreenPoint(transform.position + healthbarOffset);
            screenPos.y = Screen.height - screenPos.y;
            screenPos.x -= healthbarSize.x / 2;

            GUI.BeginGroup(new Rect(screenPos.x, screenPos.y,
                                    healthbarSize.x, healthbarSize.y));
            GUI.DrawTexture(new Rect(0, 0, healthbarSize.x, healthbarSize.y), healthbarBG, ScaleMode.StretchToFill);

            var healthPercent = health / maxHealth;

            GUI.DrawTexture(new Rect(heathbarFrameSize, heathbarFrameSize, (healthbarSize.x - 2 * heathbarFrameSize) * healthPercent, healthbarSize.y - 2 * heathbarFrameSize), healthbarFG, ScaleMode.ScaleAndCrop);

            GUI.EndGroup();
        }
    }

    public void LerpToServerPos()
    {
        var distance = Vector3.Distance(transform.position, serverPos);
        var angle = Quaternion.Angle(transform.rotation, serverRot);

        var lerp = Time.deltaTime;
        
        if (distance >= posErrorThreshold)
        {
            transform.position = Vector3.Lerp(transform.position, serverPos, lerp);
        }

        if (angle >= rotErrorThreshold)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, serverRot, lerp);
        }
    }

    public void Hit(float damage, GameObject attacker)
    {
        if (dead)
            return;

        health -= damage;
        networkView.RPC("GotHit", RPCMode.AllBuffered);

        if (health <= 0f)
        {
            health = 0f;
            superController.OnDeathServer(attacker);
            networkView.RPC("Kill", RPCMode.AllBuffered, transform.position);
        }

        superController.OnHitServer(attacker);
    }

    [RPC]
    public void SetName(string name)
    {
        this.name = name;
    }

    [RPC]
    public void GotHit()
    {
        superController.OnHitClient();
    }

    [RPC]
    public void Kill(Vector3 position)
    {
        baseAttack.CancelAttack();
        lastAnimationState = animationState;
        animationState = UnitAnimationState.Dead;
        dead = true;
        targetGameObject = null;
        target = Vector3.zero;
        superController.OnDeathClient(position);
    }

    [RPC]
    public void SetTarget(Vector3 target, string targetName)
    {
        baseAttack.CancelAttack();

        this.target = target;

        if (targetName != "terrain")
        {
            var targetGO = GameObject.Find(targetName);

            if (targetGO != null && (targetGO.tag == "Hero" || targetGO.tag == "Creep"))
            {
                this.targetGameObject = targetGO;
                return;
            }
        }

        this.targetGameObject = null;
    }

    [RPC]
    public void Jump()
    {
        jump = true;
    }

    [RPC]
    public void TryJump()
    {
        if (charController.isGrounded)
        {
            networkView.RPC("Jump", RPCMode.AllBuffered);
        }
    }

	void Update () {
	
        if (dead)
        {
            return;
        }

        if (targetGameObject != null)
        {
            var targetUnitCtrl = targetGameObject.GetComponent<UnitController>();

            if (targetUnitCtrl.dead)
            {
                targetGameObject = null;
                target = Vector3.zero;
            }
        }

        if (targetGameObject != null)
        {
            var distance = Vector3.Distance(targetGameObject.transform.position, transform.position);

            if ((following && distance <= baseAttack.Range / 2) || (!following && distance <= baseAttack.Range))
            {
                following = false;
                var lookAt = new Vector3(targetGameObject.transform.position.x, transform.position.y, targetGameObject.transform.position.z);
                transform.LookAt(lookAt);

                if (!baseAttack.IsAttacking && !baseAttack.IsOnCooldown)
                {
                    baseAttack.Attack(targetGameObject);
                }

                target = Vector2.zero;
            }
            else
            {
                following = true;
                target = targetGameObject.transform.position;
            }
        }
        else
        {
            following = false;
        }

        if (charController.isGrounded)
        {
            if (target != Vector3.zero)
            {
                var distance = Vector3.Distance(target, transform.position);

                if (distance > targetReachedThreshold)
                {
                    var lookAt = new Vector3(target.x, transform.position.y, target.z);
                    transform.LookAt(lookAt);
                    movement = transform.TransformDirection(Vector3.forward) * speed;
                }
                else
                {
                    target = Vector3.zero;
                }
            }
            else
            {
                movement = Vector3.zero;
            }
            
            if (jumping)
            {
                jumping = false;
            }
        }


        if (jump)
        {
            jump = false;
            jumping = true;
            movement.y = jumpSpeed;
        }

        movement.y -= gravity * Time.deltaTime;
        charController.Move(movement * Time.deltaTime);

        if (Network.isClient)
        {
            LerpToServerPos();
        }

        var movedSinceLast = Vector3.Distance(lastPosition, transform.position);
        var movedYSinceLast = transform.position.y - lastPosition.y;
        lastAnimationState = animationState;

        if (baseAttack.IsAttacking || serverIsAttacking)
        {
            animationState = UnitAnimationState.Attacking;
        }
        else if (lastIsGrounded && !charController.isGrounded  && jumping)
        {
            animationState = UnitAnimationState.BeginJump;
        }
        else if (!lastIsGrounded && charController.isGrounded  && Mathf.Abs(movedYSinceLast) >= fallAnimationThreshold)
        {
            animationState = UnitAnimationState.Landing;
        }
        else if (!charController.isGrounded && movedYSinceLast > 0 && (jumping || Mathf.Abs(movedYSinceLast) >= fallAnimationThreshold))
        {
            animationState = UnitAnimationState.Jumping;
        }
        else if (!charController.isGrounded && movedYSinceLast <= 0 && (jumping || Mathf.Abs(movedYSinceLast) >= fallAnimationThreshold))
        {
            animationState = UnitAnimationState.Falling;
        }
        else if (movedSinceLast > moveAnimationThreshold)
        {
            animationState = UnitAnimationState.Running;
        } 
        else 
        {
            animationState = UnitAnimationState.Idle;
        }

        lastPosition = transform.position;
        lastIsGrounded = charController.isGrounded;
	}
}