using UnityEngine;
using System.Collections;
using Pathfinding;

public enum OldUnitAnimationState
{
    Idle = 0,
    Running = 1,
    Attacking = 2,
    BeginJump = 3,
    Jumping = 4,
    Falling = 5,
    Landing = 6,
    Dead = 7,
    TPose = 8,
    CastingAbility = 9
}

public class UnitController : MonoBehaviour {

    public float speed = 30.0F;
    public float jumpSpeed = 60F;
    public float gravity = 80.0F;
    public float maxHealth;
    public float health;
    public bool dead = false;
    public bool following = false;

    public bool jumping = false;
    public bool jumpActivated = false;

    public Team team;

    public float posErrorThreshold = 2f;
    public float rotErrorThreshold = 2f;
    public float targetReachedThreshold = 2f;

    private bool lastIsGrounded = true;
    private Vector3 lastPosition;
    private Vector3 movement = Vector3.zero;
    public Vector3 target = Vector3.zero;
    public Vector3 targetWaypoint = Vector3.zero;
    public GameObject targetGameObject;
    public Vector3 serverPos = Vector3.zero;
    public Quaternion serverRot = Quaternion.identity;
    public Path path;
    private int currentWaypoint = 0;

    public BaseAttack baseAttack;

    public BaseAbility[] abilities;
    public int activatedAbility = -1;
    public bool isCastingAbility = false;

    public Texture2D healthbarBG;
    public Texture2D healthbarFG;

    public Vector3 healthbarOffset;
    public Vector2 healthbarSize;
    public float heathbarFrameSize;

    private CharacterController charController;
    public UnitSuperController superController;
    public UnitAnimationController animationController;
    public Seeker seeker;
    public LayerMask pathfindingRaycastMask;

    void Start ()
    {
        charController = GetComponent<CharacterController>();
        lastPosition = transform.position;
        serverPos = transform.position;
        serverRot = transform.rotation;

        if (seeker != null)
        {
            seeker.pathCallback += OnPathComplete;
        }

        animationController.SetAttackCastTime(baseAttack.CastTime);
    }
    
    public void OnGUI()
    {
        if (!dead)
        {
            var screenPos = Camera.main.WorldToScreenPoint(transform.position + healthbarOffset);
            screenPos.y = Screen.height - screenPos.y;
            screenPos.x -= healthbarSize.x / 2;

            if (gameObject.tag == "Hero")
            {
                GUI.Label(new Rect(screenPos.x, screenPos.y - 20, 100, 30), gameObject.name);
            }

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
    public void SetTeam(int team)
    {
        this.team = (Team)team;
        
        transform.Find("Vision").GetComponent<VisionController>().team = this.team;

        var minimap = GameObject.FindWithTag("Minimap").GetComponent<MinimapController>();
        if (this.team == Team.Red)
            minimap.Track(transform, MinimapIconType.RedPlayer);
        else if (this.team == Team.Blue)
            minimap.Track(transform, MinimapIconType.BluePlayer);
    }

    [RPC]
    public void OnAttack()
    {
        animationController.attack = true;
    }

    [RPC]
    public void OnGotTarget()
    {
        animationController.inCombat = true;
    }

    [RPC]
    public void OnLostTarget()
    {
        animationController.inCombat = false;
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
        animationController.die = true;
        dead = true;
        targetGameObject = null;
        target = Vector3.zero;
        superController.OnDeathClient(position);
    }

    [RPC]
    public void SetTarget(Vector3 target, string targetName)
    {
        baseAttack.CancelAttack();

        if (seeker != null && Network.isServer)
        {
            var direction = target - transform.position;

            if (Physics.Raycast(transform.position, direction, direction.magnitude, pathfindingRaycastMask))
            {
                seeker.StartPath(transform.position, target);
            }
            else
            {
                path = null;
            }

            this.target = target;
        }

        if (targetName != "Terrain")
        {
            var targetGO = GameObject.Find(targetName);

            if (targetGO != null && (targetGO.tag == "Hero" || targetGO.tag == "Creep"))
            {
                this.targetGameObject = targetGO;
                networkView.RPC("OnGotTarget", RPCMode.AllBuffered);
                return;
            }
        }

        this.targetGameObject = null;
        networkView.RPC("OnLostTarget", RPCMode.AllBuffered);
    }

    public void OnPathComplete (Path p) {
        if (!p.error) {
            path = p;
            currentWaypoint = 0;
        }
    }

    [RPC]
    public void JumpToTarget(Vector3 target, string targetName)
    {
        var lookAt = new Vector3(target.x, transform.position.y, target.z);
        transform.LookAt(lookAt);

        movement = transform.TransformDirection(Vector3.forward) * speed;
        movement.y = jumpSpeed;

        this.target = Vector3.zero;

        jumping = true;
        jumpActivated = false;
        animationController.jump = true;
        superController.OnAbilityCast(-1);
    }

    [RPC]
    public void TryJumpToTarget(Vector3 target, string targetName)
    {
        if (!dead && !jumping && charController.isGrounded)
        {
            networkView.RPC("JumpToTarget", RPCMode.AllBuffered, target, targetName);
        }
        else
        {
            networkView.RPC("CancelJump", RPCMode.AllBuffered);
        }
    }

    [RPC]
    public void ActivateJump()
    {
        jumpActivated = true;
        superController.OnAbilityActivate(-1);
    }

    [RPC]
    public void CancelJump()
    {
        jumpActivated = false;
        superController.OnAbilityCancel(-1);
    }

    [RPC]
    public void TryActivateJump()
    {
        if (!dead && !jumping && charController.isGrounded)
        {
            networkView.RPC("ActivateJump", RPCMode.AllBuffered);
        }
    }

    [RPC]
    public void ActivateAbility(int ability)
    {
        if (abilities[ability].Activate())
        {
            activatedAbility = ability;
            superController.OnAbilityActivate(ability);
        }
    }

    [RPC]
    public void TryActivateAbility(int ability)
    {
        if (!abilities[ability].IsOnCooldown && !dead)
        {
            networkView.RPC("ActivateAbility", RPCMode.AllBuffered, ability);
        }
    }

    [RPC]
    public void CastAbilityAtTarget(Vector3 target, string targetName)
    {
        GameObject abilityTargetGO = null;

        if (Network.isServer && targetName != "terrain")
        {
            var targetGO = GameObject.Find(targetName);

            if (targetGO != null && (targetGO.tag == "Hero" || targetGO.tag == "Creep"))
            {
                abilityTargetGO = targetGO;
            }
        }

        var lookAt = new Vector3(target.x, transform.position.y, target.z);
        transform.LookAt(lookAt);
        abilities[activatedAbility].CastAtTarget(target, abilityTargetGO);
        animationController.castAbility = activatedAbility;
        activatedAbility = -1;
        superController.OnAbilityCast(activatedAbility);
    }

    [RPC]
    public void TryCastAbilityAtTarget(Vector3 target, string targetName)
    {
        var distance = Vector3.Distance(transform.position, target);

        if (distance <= abilities[activatedAbility].Range)
        {
            networkView.RPC("CastAbilityAtTarget", RPCMode.AllBuffered, target, targetName);
        }
        else
        {
            networkView.RPC("CancelAbility", RPCMode.AllBuffered);
        }
    }

    [RPC]
    public void CancelAbility()
    {
        for (int i= 0; i < abilities.Length; i++)
        {
            abilities[i].Cancel();
        }
        activatedAbility = -1;
        superController.OnAbilityCancel(activatedAbility);
    }

    void Update()
    {
        if (dead)
        {
            return;
        }

        isCastingAbility = false;
        for (int i = 0; i < abilities.Length; i++)
        {
            if (abilities[i].IsCasting)
            {
                isCastingAbility = true;
                break;
            }
        }

        if (Network.isServer)
        {
            if (path != null)
            {
                var distance = Vector3.Distance(targetWaypoint, transform.position);

                if (targetWaypoint == Vector3.zero || distance < targetReachedThreshold)
                {
                    if (currentWaypoint >= path.vectorPath.Count) {
                        path = null;
                        targetWaypoint = Vector3.zero;
                    }
                    else
                    {
                        targetWaypoint = path.vectorPath[currentWaypoint++];
                    }
                }
            }
            else
            {
                targetWaypoint = Vector3.zero;
            }
        }

        if (targetGameObject != null)
        {
            var targetUnitCtrl = targetGameObject.GetComponent<UnitController>();

            if (targetUnitCtrl.dead)
            {
                targetGameObject = null;
                networkView.RPC("OnLostTarget", RPCMode.AllBuffered);
                target = Vector3.zero;
            }
        }

        if (targetGameObject != null && !isCastingAbility)
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
                    networkView.RPC("OnAttack", RPCMode.AllBuffered);
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
            if (targetWaypoint != Vector3.zero && !isCastingAbility && !jumping)
            {
                var distance = Vector3.Distance(targetWaypoint, transform.position);

                if (distance > targetReachedThreshold)
                {
                    var lookAt = new Vector3(targetWaypoint.x, transform.position.y, targetWaypoint.z);
                    transform.LookAt(lookAt);
                    movement = transform.TransformDirection(Vector3.forward) * speed;
                }
            }
            else if (target != Vector3.zero && !isCastingAbility && !jumping)
            {
                var distance = Vector3.Distance(target, transform.position);

                if (distance > targetReachedThreshold)
                {
                    var lookAt = new Vector3(target.x, transform.position.y, target.z);
                    transform.LookAt(lookAt);
                    movement = transform.TransformDirection(Vector3.forward) * speed;
                }
            }
        }

        movement.y -= gravity * Time.deltaTime;
        charController.Move(movement * Time.deltaTime);
        var actualMovement = transform.position - lastPosition;
        animationController.speed = new Vector2(actualMovement.x, actualMovement.z).magnitude;
        animationController.ySpeed = actualMovement.y;

        if (charController.isGrounded)
        {
            movement = Vector3.zero;
        }

        if (Network.isClient)
        {
            LerpToServerPos();
        }

        if (jumping && charController.isGrounded)
        {
            jumping = false;
        }

        lastPosition = transform.position;
    }
}
