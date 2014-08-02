using UnityEngine;
using System.Collections;

public class CreepController : UnitSuperController {

    public AudioClip hitSound;
    public float volume = 2f;
    public UnitController unitController;

    public override void OnHitServer(GameObject attacker)
    {
        unitController.targetGameObject = attacker;
    }

    public override void OnHitClient()
    {
        AudioSource.PlayClipAtPoint(hitSound, transform.position, volume);
    }

    public override void OnDeathServer(GameObject attacker)
    {

    }

    public override void OnDeathClient(Vector3 position)
    {

    }

    public override void OnAbilityActivate(int ability)
    {
    }

    public override void OnAbilityCast(int ability)
    {
    }

    public override void OnAbilityCancel(int ability)
    {
    }
}
