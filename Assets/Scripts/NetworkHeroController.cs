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
        var jump = heroController.jump;

        if (stream.isWriting)
        {
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
            stream.Serialize(ref target);
            stream.Serialize(ref jump);
        }
        else
        {
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
            stream.Serialize(ref target);
            stream.Serialize(ref jump);

            heroController.serverPos = pos;
            heroController.serverRot = rot;
            heroController.target = target;
            heroController.jump = jump;

            heroController.LerpToServerPos();
        }
    }
}
