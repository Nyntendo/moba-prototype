using UnityEngine;
using System.Collections;

public class HeroController : MonoBehaviour {
    public float speed = 6.0F;
    public float jumpSpeed = 10F;
    public float gravity = 20.0F;
    public Vector3 lastPosition;

    public GameObject redFlag;
    public GameObject blueFlag;
    public GameObject gameController;

    public Vector3 target = Vector3.zero;
    public Vector3 movement = Vector3.zero;
    public Vector3 serverPos = Vector3.zero;
    public Quaternion serverRot = Quaternion.identity;
    public float posErrorThreshold = 0.2f;
    public float targetReachedThreshold = 0.5f;
    public bool jump = false;
    public bool carryingFlag = false;

    private CharacterController charController;
    private NetworkPlayer owner;
    private Team team;

    void Start()
    {
        charController = GetComponent<CharacterController>();
        redFlag = GameObject.FindWithTag("RedFlag");
        blueFlag = GameObject.FindWithTag("BlueFlag");
        gameController = GameObject.FindWithTag("GameController");
    }

    private void FindTarget()
    {
        var point = Input.mousePosition;
        var hit = new RaycastHit();
        if (Physics.Raycast(Camera.main.ScreenPointToRay(point), out hit, 1000.0f))
        {
            if (hit.collider.name != transform.name)
            {
                if (Network.isClient)
                {
                    networkView.RPC("SetTarget", RPCMode.Server, hit.point);
                }
                else
                {
                    SetTarget(hit.point);
                }
            }
        }
    }

    public void LerpToServerPos()
    {
        var distance = Vector3.Distance(transform.position, serverPos);

        if (distance >= posErrorThreshold)
        {
            var lerp = ((1 / distance) * speed) / 100;

            transform.position = Vector3.Lerp(transform.position, serverPos, lerp);
            transform.rotation = Quaternion.Slerp(transform.rotation, serverRot, lerp);
        }
    }

    void Update()
    {
        if (Network.player == owner && owner != null)
        {
            if (Input.GetButton("Fire1") && GUIUtility.hotControl==0)
            {
                FindTarget();
            }

            if (Input.GetButton("Fire2") && GUIUtility.hotControl==0)
            {
                FindTarget();
                if (Network.isClient)
                {
                    networkView.RPC("Jump", RPCMode.Server);
                }
                else
                {
                    Jump();
                }
            }
        }


        if (lastPosition != transform.position) {
            animation.CrossFade ("Running", 0.2f);
            lastPosition = transform.position;
        } 
        else 
        {
            animation.CrossFade("Idle", 0.1f);
        }

        if (charController.isGrounded)
        {
            if (target != Vector3.zero)
            {
                var distance = (target - transform.position).magnitude;
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
            if (charController.isGrounded)
            {
                movement.y = jumpSpeed;
            }
        }

        movement.y -= gravity * Time.deltaTime;
        charController.Move(movement * Time.deltaTime);
    }

    [RPC]
    public void SetTarget(Vector3 target)
    {
        this.target = target;
    }

    [RPC]
    public void Jump()
    {
        jump = true;
    }

    [RPC]
    public void SetOwner(NetworkPlayer player)
    {
        this.owner = player;

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

	void OnTriggerEnter(Collider other)
    {
        if (Network.isServer)
        {
            if (other.gameObject.tag == "RedFlag" && team == Team.Blue)
            {
                networkView.RPC("PickUpFlag", RPCMode.AllBuffered);
            }
            if (other.gameObject.tag == "BlueFlag" && team == Team.Red)
            {
                networkView.RPC("PickUpFlag", RPCMode.AllBuffered);
            }
            if (other.gameObject.tag == "BlueBase" && team == Team.Blue && carryingFlag)
            {
                networkView.RPC("ReturnFlag", RPCMode.AllBuffered);
                gameController.networkView.RPC("Score", RPCMode.AllBuffered, (int)team);
            }
            if (other.gameObject.tag == "RedBase" && team == Team.Red && carryingFlag)
            {
                networkView.RPC("ReturnFlag", RPCMode.AllBuffered);
                gameController.networkView.RPC("Score", RPCMode.AllBuffered, (int)team);
            }
        }
    }

}
