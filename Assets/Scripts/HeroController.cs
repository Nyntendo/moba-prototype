﻿using UnityEngine;
using System.Collections;

public class HeroController : UnitSuperController {
    public float respawnTimer = 0f;

    public float triggerImuneTime = 5f;
    private float triggerImuneTimer = 0f;
    private bool triggerImune = false;

    public float animationFade = 0.2f;
    private bool queueNextAnimation = false;

    public string attackAnimation;

    private GameObject redFlag;
    private GameObject blueFlag;

    private GameController gameController;
    private ScoreController scoreController;
    private LevelController levelController;
    public UnitController unitController;

    public bool carryingFlag = false;

    private NetworkPlayer owner;

    void Start()
    {
        redFlag = GameObject.FindWithTag("RedFlag");
        blueFlag = GameObject.FindWithTag("BlueFlag");

        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        scoreController = GameObject.FindWithTag("ScoreController").GetComponent<ScoreController>();
        levelController = GameObject.FindWithTag("LevelController").GetComponent<LevelController>();

        animation[attackAnimation].wrapMode = WrapMode.PingPong;
        animation["Death"].wrapMode = WrapMode.Once;
        animation["JumpStart"].wrapMode = WrapMode.Once;
        animation["Landing"].wrapMode = WrapMode.Once;
        animation["Landing"].speed = 2f;
    }

    public override void OnHitServer(GameObject attacker)
    {
    }

    public override void OnHitClient()
    {
    }

    public override void OnDeathServer(GameObject attacker)
    {
        respawnTimer = levelController.respawnTime;
    }

    public override void OnDeathClient(Vector3 position)
    {
        if (carryingFlag)
        {
            DropFlag(position);
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
                    unitController.SetTarget(hit.point, hit.collider.name);
                }
            }
        }
    }

    private void FindAbilityTarget()
    {
        var point = Input.mousePosition;
        var hit = new RaycastHit();
        if (Physics.Raycast(Camera.main.ScreenPointToRay(point), out hit, 1000.0f))
        {
            if (Network.isClient)
            {
                networkView.RPC("TryCastAbilityAtTarget", RPCMode.Server, hit.point, hit.collider.name);
            }
            else
            {
                unitController.TryCastAbilityAtTarget(hit.point, hit.collider.name);
            }
        }
    }

    void Update()
    {
        if (Network.isServer && unitController.dead)
        {
            respawnTimer -= Time.deltaTime;
            
            if (respawnTimer <= 0f)
            {
                Vector3 spawnPoint = Vector3.zero;
                if (unitController.team == Team.Red)
                    spawnPoint = levelController.spawnPointRed.position;
                else if (unitController.team == Team.Blue)
                    spawnPoint = levelController.spawnPointBlue.position;

                networkView.RPC("Respawn", RPCMode.AllBuffered, spawnPoint, Quaternion.identity);
            }
        }

        animation[attackAnimation].speed = animation[attackAnimation].length / unitController.baseAttack.CastTime;

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

            if (Input.GetButtonUp("Jump") && GUIUtility.hotControl == 0)
            {
                if (unitController.activatedAbility == -1)
                {
                    FindTarget();
                    if (Network.isClient)
                    {
                        networkView.RPC("TryJump", RPCMode.Server);
                    }
                    else
                    {
                        unitController.TryJump();
                    }
                }
                else
                {
                    FindAbilityTarget();
                }
            }

            if (Input.GetButtonUp("Q Ability"))
            {
                if (Network.isClient)
                {
                    networkView.RPC("TryActivateAbility", RPCMode.Server, 0);
                }
                else
                {
                    unitController.TryActivateAbility(0);
                }
            }
        }
    }

    public void LateUpdate()
    {
        if (unitController.lastAnimationState != unitController.animationState)
        {
            switch (unitController.animationState)
            {
                case UnitAnimationState.CastingAbility:
                    FadeOrQueue(attackAnimation);
                    break;
                case UnitAnimationState.Attacking:
                    FadeOrQueue(attackAnimation);
                    break;
                case UnitAnimationState.BeginJump:
                    FadeOrQueue("JumpStart");
                    queueNextAnimation = true;
                    break;
                case UnitAnimationState.Landing:
                    FadeOrQueue("Landing");
                    queueNextAnimation = true;
                    break;
                case UnitAnimationState.Jumping:
                    FadeOrQueue("Upwards");
                    break;
                case UnitAnimationState.Falling:
                    FadeOrQueue("Falling");
                    break;
                case UnitAnimationState.Running:
                    FadeOrQueue("Running");
                    break;
                case UnitAnimationState.Dead:
                    unitController.lastAnimationState = UnitAnimationState.Dead;
                    FadeOrQueue("Death");
                    break;
                case UnitAnimationState.Idle:
                default:
                    FadeOrQueue("Idle");
                    break;
            }
        }
    }

    private void FadeOrQueue(string animationName)
    {
        if (queueNextAnimation)
        {
            animation.CrossFadeQueued(animationName, animationFade);
            queueNextAnimation = false;
        }
        else
        {
            animation.CrossFade(animationName, animationFade);
        }
    }


    [RPC]
    public void Respawn(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        unitController.serverPos = position;
        transform.rotation = rotation;
        unitController.serverRot = rotation;
        unitController.health = unitController.maxHealth;
        unitController.dead = false;
        triggerImuneTimer = triggerImuneTime;
        unitController.animationState = UnitAnimationState.Idle;
    }

    [RPC]
    public void SetOwner(NetworkPlayer player)
    {
        this.owner = player;

        if (Network.player == player)
        {
            Camera.main.GetComponent<CameraController>().target = transform;
            GetComponent<AudioListener>().enabled = true;
        }
    }


    [RPC]
    public void PickUpFlag()
    {
        if (unitController.team == Team.Blue)
        {
            transform.Find("RedFlagModel").gameObject.SetActive(true);
            redFlag.SetActive(false);
        }
        else if (unitController.team == Team.Red)
        {
            transform.Find("BlueFlagModel").gameObject.SetActive(true);
            blueFlag.SetActive(false);
        }
        carryingFlag = true;
    }

    [RPC]
    public void ReturnFlag()
    {
        if (unitController.team == Team.Blue)
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
        if (unitController.team == Team.Blue)
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
        if (Network.isServer && !unitController.dead && !triggerImune)
        {
            if (other.gameObject.tag == "RedFlag")
            {
                if (unitController.team == Team.Blue)
                {
                    networkView.RPC("PickUpFlag", RPCMode.AllBuffered);
                    levelController.redFlagIsMissing = true;
                }
                else if(unitController.team == Team.Red && levelController.redFlagIsMissing)
                {
                    levelController.networkView.RPC("ReturnRedFlagToBase", RPCMode.AllBuffered);
                    levelController.redFlagIsMissing = false;
                }
            }
            if (other.gameObject.tag == "BlueFlag")
            {
                if (unitController.team == Team.Red)
                {
                    networkView.RPC("PickUpFlag", RPCMode.AllBuffered);
                    levelController.blueFlagIsMissing = true;
                }
                else if(unitController.team == Team.Blue && levelController.blueFlagIsMissing)
                {
                    levelController.networkView.RPC("ReturnBlueFlagToBase", RPCMode.AllBuffered);
                    levelController.blueFlagIsMissing = false;
                }
            }
            if (other.gameObject.tag == "BlueBase" && unitController.team == Team.Blue && carryingFlag && !levelController.blueFlagIsMissing)
            {
                Debug.Log(name + ", unitController.team: " + unitController.team + ", carryingFlag: " + carryingFlag);
                networkView.RPC("ReturnFlag", RPCMode.AllBuffered);
                levelController.redFlagIsMissing = false;
                scoreController.networkView.RPC("Score", RPCMode.AllBuffered, (int)unitController.team);
            }
            if (other.gameObject.tag == "RedBase" && unitController.team == Team.Red && carryingFlag && !levelController.redFlagIsMissing)
            {
                Debug.Log(name + ", unitController.team: " + unitController.team + ", carryingFlag: " + carryingFlag);
                networkView.RPC("ReturnFlag", RPCMode.AllBuffered);
                levelController.blueFlagIsMissing = false;
                scoreController.networkView.RPC("Score", RPCMode.AllBuffered, (int)unitController.team);
            }
        }
    }
}
