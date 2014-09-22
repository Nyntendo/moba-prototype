using UnityEngine;
using System.Collections;

public class HeroController : UnitSuperController {
    public float respawnTimer = 0f;

    public float triggerImuneTime = 5f;
    private float triggerImuneTimer = 0f;
    private bool triggerImune = false;

    private GameObject redFlag;
    private GameObject blueFlag;

    public LayerMask clickRaycastLayerMask;

    private GameController gameController;
    private ScoreController scoreController;
    private LevelController levelController;
    private SkillshotMarkerController skillshotController;
    private MoveMarkerController moveMarkerController;
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

        var targetMarkers = GameObject.FindWithTag("TargetMarkers");
        skillshotController = targetMarkers.transform.Find("SkillshotMarker").GetComponent<SkillshotMarkerController>();
        var moveMarker = GameObject.FindWithTag("MoveMarker");
        moveMarkerController = moveMarker.GetComponent<MoveMarkerController>();
    }

    public override void OnHitServer(GameObject attacker)
    {
    }

    public override void OnHitClient()
    {
    }

    public override void OnAbilityActivate(int ability)
    {
        if (Network.player == owner)
        {
            skillshotController.SetTrackingTarget(transform);
        }
    }

    public override void OnAbilityCast(int ability)
    {
        if (Network.player == owner)
        {
            skillshotController.SetTrackingTarget(null);
        }
    }

    public override void OnAbilityCancel(int ability)
    {
        if (Network.player == owner)
        {
            skillshotController.SetTrackingTarget(null);
        }
    }

    public override void OnDeathServer(GameObject attacker)
    {
        respawnTimer = levelController.respawnTime;
        if (attacker.tag == "Hero")
        {
            scoreController.networkView.RPC("AddKill", RPCMode.AllBuffered, attacker.name, transform.name);
        }
        else
        {
            scoreController.networkView.RPC("AddDeath", RPCMode.AllBuffered, transform.name);
        }
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
        if (Physics.Raycast(Camera.main.ScreenPointToRay(point), out hit, 1000.0f, clickRaycastLayerMask))
        {
            Debug.Log(hit.collider.name);
            if (hit.collider.name != transform.name)
            {
                if (hit.collider.name == "Terrain")
                {
                    moveMarkerController.SetPosition(hit.point);
                }

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
        if (Physics.Raycast(Camera.main.ScreenPointToRay(point), out hit, 1000.0f, clickRaycastLayerMask))
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

    private void FindJumpTarget()
    {
        var point = Input.mousePosition;
        var hit = new RaycastHit();
        if (Physics.Raycast(Camera.main.ScreenPointToRay(point), out hit, 1000.0f, clickRaycastLayerMask))
        {
            if (Network.isClient)
            {
                networkView.RPC("TryJumpToTarget", RPCMode.Server, hit.point, hit.collider.name);
            }
            else
            {
                unitController.TryJumpToTarget(hit.point, hit.collider.name);
            }
        }
    }

    void FixedUpdate()
    {
        if (Network.isServer && unitController.dead)
        {
            respawnTimer -= Time.fixedDeltaTime;
            
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

        if (triggerImuneTimer > 0f)
        {
            triggerImune = true;
            triggerImuneTimer -= Time.fixedDeltaTime;

            if (triggerImuneTimer <= 0f)
            {
                triggerImune = false;
            }
        }

        if (Network.player == owner && owner != null)
        {
            if (Input.GetButtonUp("Move") && GUIUtility.hotControl == 0)
            {
                FindTarget();
            }

            if (Input.GetButtonUp("Target") && GUIUtility.hotControl == 0)
            {
                if (unitController.jumpActivated)
                {
                    FindJumpTarget();
                }
                else if (unitController.activatedAbility >= 0)
                {
                    FindAbilityTarget();
                }
            }

            if (Input.GetButtonUp("Jump"))
            {
                if (Network.isClient)
                {
                    networkView.RPC("TryActivateJump", RPCMode.Server);
                }
                else
                {
                    unitController.TryActivateJump();
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

    [RPC]
    public void Respawn(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        unitController.serverPos = position;
        transform.rotation = rotation;
        unitController.serverRot = rotation;
        unitController.health = unitController.maxHealth;
        unitController.dead = false;
        unitController.target = Vector3.zero;
        unitController.targetGameObject = null;
        unitController.animationController.revive = true;
        triggerImuneTimer = triggerImuneTime;
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
                    scoreController.networkView.RPC("AddReturn", RPCMode.AllBuffered, transform.name);
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
                    scoreController.networkView.RPC("AddReturn", RPCMode.AllBuffered, transform.name);
                }
            }
            if (other.gameObject.tag == "BlueBase" && unitController.team == Team.Blue && carryingFlag && !levelController.blueFlagIsMissing)
            {
                Debug.Log(name + ", unitController.team: " + unitController.team + ", carryingFlag: " + carryingFlag);
                networkView.RPC("ReturnFlag", RPCMode.AllBuffered);
                levelController.redFlagIsMissing = false;
                scoreController.networkView.RPC("AddCapture", RPCMode.AllBuffered, transform.name);
            }
            if (other.gameObject.tag == "RedBase" && unitController.team == Team.Red && carryingFlag && !levelController.redFlagIsMissing)
            {
                Debug.Log(name + ", unitController.team: " + unitController.team + ", carryingFlag: " + carryingFlag);
                networkView.RPC("ReturnFlag", RPCMode.AllBuffered);
                levelController.blueFlagIsMissing = false;
                scoreController.networkView.RPC("AddCapture", RPCMode.AllBuffered, transform.name);
            }
        }
    }
}
