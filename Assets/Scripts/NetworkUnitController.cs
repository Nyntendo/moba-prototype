using UnityEngine;
using System.Collections;

public class NetworkUnitController : MonoBehaviour
{
    public Transform observedTransform;
    public UnitController unitController;

    void OnSerializeNetworkView (BitStream stream, NetworkMessageInfo info)
    {
        var pos = observedTransform.position;
        var rot = observedTransform.rotation;
        var target = unitController.target;
        var serverIsAttacking = unitController.serverIsAttacking;
        var health = unitController.health;

        if (stream.isWriting)
        {
            serverIsAttacking = unitController.baseAttack.IsAttacking;
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

            unitController.serverPos = pos;
            unitController.serverRot = rot;
            unitController.target = target;
            unitController.serverIsAttacking = serverIsAttacking;
            unitController.health = health;
        }
    }
}
