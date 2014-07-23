using UnityEngine;
using System.Collections;

public class NetworkHeroController : MonoBehaviour
{
    public Transform observedTransform;
    public HeroController heroController;

    void OnSerializeNetworkView (BitStream stream, NetworkMessageInfo info)
    {
        var pos = observedTransform.position;
        var rot = observedTransform.rotation;
        var target = heroController.target;
        var serverIsAttacking = heroController.serverIsAttacking;
        var health = heroController.health;

        if (stream.isWriting)
        {
            serverIsAttacking = heroController.baseAttack.IsAttacking;
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
            stream.Serialize(ref target);
            stream.Serialize(ref serverIsAttacking);
            stream.Serialize(ref health);
        }
        else
        {
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
            stream.Serialize(ref target);
            stream.Serialize(ref serverIsAttacking);
            stream.Serialize(ref health);

            heroController.serverPos = pos;
            heroController.serverRot = rot;
            heroController.target = target;
            heroController.serverIsAttacking = serverIsAttacking;
            heroController.health = health;
        }
    }
}
