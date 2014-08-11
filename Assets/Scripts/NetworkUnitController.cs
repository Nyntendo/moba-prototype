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
        var targetWaypoint = unitController.targetWaypoint;
        var health = unitController.health;

        if (stream.isWriting)
        {
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
            stream.Serialize(ref target);
            stream.Serialize(ref targetWaypoint);
            stream.Serialize(ref health);
        }
        else
        {
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
            stream.Serialize(ref target);
            stream.Serialize(ref targetWaypoint);
            stream.Serialize(ref health);

            unitController.serverPos = pos;
            unitController.serverRot = rot;
            unitController.target = target;
            unitController.targetWaypoint = targetWaypoint;
            unitController.health = health;
        }
    }
}
