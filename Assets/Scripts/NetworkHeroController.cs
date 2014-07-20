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

        if (stream.isWriting)
        {
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
            stream.Serialize(ref target);
        }
        else
        {
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
            stream.Serialize(ref target);

            heroController.serverPos = pos;
            heroController.serverRot = rot;
            heroController.target = target;
        }
    }
}
