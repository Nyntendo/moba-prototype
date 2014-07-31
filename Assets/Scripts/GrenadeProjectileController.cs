using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrenadeProjectileController : MonoBehaviour {

    public Vector3 serverPos = Vector3.zero;
    public Quaternion serverRot = Quaternion.identity;

    private float damage = 0f;
    private GameObject attacker;

    public float posErrorThreshold = 2f;
    public float rotErrorThreshold = 2f;

    public float detonateTime = 3f;
    public float explodeTime = 0.5f;
    public float timer = 0f;
    public float explodeLightRange = 1000f;

    private Light light;

    private List<GameObject> hitList;

    [RPC]
    public void Throw(Vector3 force)
    {
        rigidbody.AddForce(force, ForceMode.Impulse);
        Debug.Log("Applying force: " + force);
    }

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }

    public void SetAttacker(GameObject attacker)
    {
        this.attacker = attacker;
    }

    void Start()
    {
        hitList = new List<GameObject>();
        light = transform.Find("ManaSphere").transform.Find("Point light").GetComponent<Light>();
        serverPos = transform.position;
        serverRot = transform.rotation;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= detonateTime + explodeTime)
        {
            if (Network.isServer)
            {
                foreach (GameObject go in hitList)
                {
                    var goUnitController = go.GetComponent<UnitController>();
                    goUnitController.Hit(damage, attacker);
                }
            }
            gameObject.SetActive(false);
            Destroy(gameObject, 1f);
        }
        else if (timer >= detonateTime)
        {
            var newRange = ((timer - detonateTime) / explodeTime) * explodeLightRange;
            light.range = newRange;
        }

        if (Network.isClient)
        {
            LerpToServerPos();
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

    void OnTriggerEnter(Collider other)
    {
        if (Network.isServer)
        {
            if (other.gameObject.tag == "Hero" || other.gameObject.tag == "Creep")
            {
                hitList.Add(other.gameObject);
                // var targetUnitController = targetGameObject.GetComponent<UnitController>();
                // targetUnitController.Hit(damage, attacker);
                // Network.Destroy(networkView.viewID);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (Network.isServer)
        {
            if (other.gameObject.tag == "Hero" || other.gameObject.tag == "Creep")
            {
                if (hitList.Contains(other.gameObject))
                {
                    hitList.Remove(other.gameObject);
                }
            }
        }
    }

    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        var pos = transform.position;
        var rot = transform.rotation;

        if (stream.isWriting)
        {
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
        }
        else
        {
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);

            serverPos = pos;
            serverRot = rot;
        }
    }
}
