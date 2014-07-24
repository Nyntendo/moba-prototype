using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour {

    private float speed = 50f;
    private float damage = 0f;
    public Vector3 target = Vector3.zero;
    public Vector3 hitOffset = Vector3.zero;
    public GameObject targetGameObject;
    public GameObject attacker;
    private Vector3 movement = Vector3.zero;

	void Start () {
	
	}
	
	void Update ()
    {
        if (targetGameObject != null)
        {
            target = targetGameObject.transform.position + hitOffset;
        }

        if (target != Vector3.zero)
        {
            transform.LookAt(target);
            movement = transform.TransformDirection(Vector3.forward) * speed;
            transform.position += movement * Time.deltaTime;
        }
	}

    void OnSerializeNetworkView (BitStream stream, NetworkMessageInfo info)
    {
        var target = this.target;

        if (stream.isWriting)
        {
            stream.Serialize(ref target);
        }
        else
        {
            stream.Serialize(ref target);

            this.target = target;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (Network.isServer)
        {
            if (other.gameObject == targetGameObject)
            {
                if (other.gameObject.tag == "Hero")
                {
                    var heroCtrl = other.gameObject.GetComponent<HeroController>();
                    heroCtrl.Hit(damage);
                }
                else if (other.gameObject.tag == "Creep")
                {
                    var creepCtrl = other.gameObject.GetComponent<CreepController>();
                    creepCtrl.Hit(damage, attacker);
                }
                Network.Destroy(networkView.viewID);
            }
        }
    }

    [RPC]
    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    [RPC]
    public void SetDamage(float damage)
    {
        this.damage = damage;
    }
}
