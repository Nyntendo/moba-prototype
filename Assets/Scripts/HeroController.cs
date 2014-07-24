using UnityEngine;
using System.Collections;

public class HeroController : MonoBehaviour {
    public float speed = 6.0F;
    public float jumpSpeed = 10F;
    public float gravity = 20.0F;

    public float posErrorThreshold = 0.2f;
    public float rotErrorThreshold = 2f;
    public float targetReachedThreshold = 0.5f;
    public float moveAnimationThreshold = 0.3f;
    public float respawnTimer = 0f;

    public float triggerImuneTime = 5f;
    private float triggerImuneTimer = 0f;
    private bool triggerImune = false;

    public Vector3 target = Vector3.zero;
    public GameObject targetGameObject;
    public Vector3 serverPos = Vector3.zero;
    public Quaternion serverRot = Quaternion.identity;

    public float idleAnimationFade = 0.2f;
    public float runAnimationFade = 0.2f;
    public float attackAnimationFade = 0.2f;

    public BaseAttack baseAttack;

    public Texture2D healthbarBG;
    public Texture2D healthbarFG;

    public Vector3 healthbarOffset;
    public Vector2 healthbarSize;
    public float heathbarFrameSize;

    public float maxHealth;
    public float health;

    private GameObject redFlag;
    private GameObject blueFlag;
    private GameObject gameController;

    private Vector3 lastPosition;
    private Vector3 movement = Vector3.zero;
    private bool jump = false;
    public bool carryingFlag = false;
    public bool serverIsAttacking = false;

    private CharacterController charController;
    private NetworkPlayer owner;
    public Team team;
    public bool dead = false;

    void Start()
    {
        charController = GetComponent<CharacterController>();

        redFlag = GameObject.FindWithTag("RedFlag");
        blueFlag = GameObject.FindWithTag("BlueFlag");
        gameController = GameObject.FindWithTag("GameController");
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

    private void FindTarget()
    {
        var point = Input.mousePosition;
        var hit = new RaycastHit();
        if (Physics.Raycast(Camera.main.ScreenPointToRay(point), out hit, 1000.0f))
        {
            Debug.Log(hit.collider.name);
            if (hit.collider.name != transform.name)
            {
                if (Network.isClient)
                {
                    networkView.RPC("SetTarget", RPCMode.Server, hit.point, hit.collider.name);
                }
                else
                {
                    SetTarget(hit.point, hit.collider.name);
                }
            }
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

    void Update()
    {
        if (dead)
        {
            return;
        }

        if (triggerImuneTimer > 0f)
        {
            triggerImune = true;
            triggerImuneTimer -= Time.deltaTime;

            if (triggerImuneTimer <= 0f)
            {
                triggerImune = false;
            }
        }

        if (Network.player == owner && owner != null)
        {
            if (Input.GetButton("Move") && GUIUtility.hotControl == 0)
            {
                FindTarget();
            }

            if (Input.GetButton("Jump") && GUIUtility.hotControl == 0)
            {
                FindTarget();
                if (Network.isClient)
                {
                    networkView.RPC("TryJump", RPCMode.Server);
                }
                else
                {
                    TryJump();
                }
            }
        }

        if (targetGameObject != null)
        {
            if (targetGameObject.tag == "Hero")
            {
                var heroCtrl = targetGameObject.GetComponent<HeroController>();

                if (heroCtrl.dead)
                {
                    targetGameObject = null;
                    target = Vector3.zero;
                }
            }
        }

        if (targetGameObject != null)
        {
            var distance = Vector3.Distance(targetGameObject.transform.position, transform.position);

            if (distance <= baseAttack.Range)
            {
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
                target = targetGameObject.transform.position;
            }
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
        }

        if (jump)
        {
            jump = false;
            movement.y = jumpSpeed;
        }

        movement.y -= gravity * Time.deltaTime;
        charController.Move(movement * Time.deltaTime);

        if (Network.isClient)
        {
            LerpToServerPos();
        }

        var movedSinceLast = Vector3.Distance(lastPosition, transform.position);

        if (baseAttack.IsAttacking || serverIsAttacking)
        {
            animation.CrossFade ("RangedAttack1", attackAnimationFade);
        }
        else if (movedSinceLast > moveAnimationThreshold) {
            animation.CrossFade ("Running", runAnimationFade);
            lastPosition = transform.position;
        } 
        else 
        {
            animation.CrossFade("Idle", idleAnimationFade);
        }
    }

    public void Hit(float damage)
    {
        if (dead)
            return;

        health -= damage;

        if (health <= 0f)
        {
            health = 0f;
            networkView.RPC("Kill", RPCMode.AllBuffered, transform.position);
            gameController.GetComponent<GameController>().ScheduleForRespawn(owner);
        }
    }

    [RPC]
    public void Respawn(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        serverPos = position;
        transform.rotation = rotation;
        serverRot = rotation;
        health = maxHealth;
        dead = false;
        triggerImuneTimer = triggerImuneTime;
        animation.CrossFade("Idle", idleAnimationFade);
    }

    [RPC]
    public void Kill(Vector3 position)
    {
        baseAttack.CancelAttack();
        transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
        animation.Play("TPose");
        dead = true;
        targetGameObject = null;
        target = Vector3.zero;
        if (carryingFlag)
        {
            DropFlag(position);
        }
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

    [RPC]
    public void SetOwner(NetworkPlayer player)
    {
        this.owner = player;
        this.name = "Hero" + player.ToString();

        if (Network.player == player)
        {
            Camera.main.GetComponent<CameraController>().target = transform;
        }
    }

    [RPC]
    public void SetTeam(int team)
    {
        this.team = (Team)team;
    }

    [RPC]
    public void PickUpFlag()
    {
        Debug.Log(name + " of team " + team + " picked up flag " + Time.realtimeSinceStartup);

        if (team == Team.Blue)
        {
            transform.Find("RedFlagModel").gameObject.SetActive(true);
            redFlag.SetActive(false);
        }
        else
        {
            transform.Find("BlueFlagModel").gameObject.SetActive(true);
            blueFlag.SetActive(false);
        }
        carryingFlag = true;
    }

    [RPC]
    public void ReturnFlag()
    {
        if (team == Team.Blue)
        {
            transform.Find("RedFlagModel").gameObject.SetActive(false);
            redFlag.SetActive(true);
        }
        else
        {
            transform.Find("BlueFlagModel").gameObject.SetActive(false);
            blueFlag.SetActive(true);
        }
        carryingFlag = false;
    }

    public void DropFlag(Vector3 position)
    {
        if (team == Team.Blue)
        {
            transform.Find("RedFlagModel").gameObject.SetActive(false);
            redFlag.transform.position = position;
            redFlag.SetActive(true);
        }
        else
        {
            transform.Find("BlueFlagModel").gameObject.SetActive(false);
            blueFlag.transform.position = position;
            blueFlag.SetActive(true);
        }
        carryingFlag = false;
    }

	void OnTriggerEnter(Collider other)
    {
        if (Network.isServer && !dead && !triggerImune)
        {
            var gc = gameController.GetComponent<GameController>();

            if (other.gameObject.tag == "RedFlag")
            {
                if (team == Team.Blue)
                {
                    networkView.RPC("PickUpFlag", RPCMode.AllBuffered);
                    gc.redFlagIsMissing = true;
                }
                else if(team == Team.Red && gc.redFlagIsMissing)
                {
                    gameController.networkView.RPC("ReturnRedFlagToBase", RPCMode.AllBuffered);
                    gc.redFlagIsMissing = false;
                }
            }
            if (other.gameObject.tag == "BlueFlag")
            {
                if (team == Team.Red)
                {
                    networkView.RPC("PickUpFlag", RPCMode.AllBuffered);
                    gc.blueFlagIsMissing = true;
                }
                else if(team == Team.Blue && gc.blueFlagIsMissing)
                {
                    gameController.networkView.RPC("ReturnBlueFlagToBase", RPCMode.AllBuffered);
                    gc.blueFlagIsMissing = false;
                }
            }
            if (other.gameObject.tag == "BlueBase" && team == Team.Blue && carryingFlag && !gc.blueFlagIsMissing)
            {
                Debug.Log(name + ", team: " + team + ", carryingFlag: " + carryingFlag);
                networkView.RPC("ReturnFlag", RPCMode.AllBuffered);
                gc.redFlagIsMissing = false;
                gameController.networkView.RPC("Score", RPCMode.AllBuffered, (int)team);
            }
            if (other.gameObject.tag == "RedBase" && team == Team.Red && carryingFlag && !gc.redFlagIsMissing)
            {
                Debug.Log(name + ", team: " + team + ", carryingFlag: " + carryingFlag);
                networkView.RPC("ReturnFlag", RPCMode.AllBuffered);
                gc.blueFlagIsMissing = false;
                gameController.networkView.RPC("Score", RPCMode.AllBuffered, (int)team);
            }
        }
    }
}
