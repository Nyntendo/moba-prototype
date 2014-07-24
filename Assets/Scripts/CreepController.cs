using UnityEngine;
using System.Collections;

public class CreepController : MonoBehaviour {

    public AudioClip hitSound;
    public float volume = 2f;
    public Vector3 target = Vector3.zero;
    public float gravity = 40f;
    private Vector3 movement = Vector3.zero;
    public float speed = 5;
    private CharacterController charController;

    void Start()
    {
        charController = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (target != Vector3.zero)
        {
            var lookAt = new Vector3(target.x, transform.position.y, target.z);
            transform.LookAt(lookAt);
            movement = transform.TransformDirection(Vector3.forward) * speed;
        }
        else
        {
            movement = Vector3.zero;
        }

        movement.y -= gravity * Time.deltaTime;
        charController.Move(movement * Time.deltaTime);
    }

    public void Hit(float damage, GameObject attacker)
    {
        Debug.Log(attacker.name);
        AudioSource.PlayClipAtPoint(hitSound, transform.position, volume);
        target = attacker.transform.position;
    }
}
