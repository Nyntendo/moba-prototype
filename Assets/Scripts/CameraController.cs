using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public Transform target;
    public float lerpSpeed = 1f;
    public Vector3 offset;
    public float xMax;
    public float xMin;
    public float zMax;
    public float zMin;

    public Vector3 standbyPos;

    void Update () {
        if (target != null)
        {
            var newPos = target.position + offset;

            newPos = Vector3.Min(newPos, new Vector3(xMax, newPos.y, zMax));
            newPos = Vector3.Max(newPos, new Vector3(xMin, newPos.y, zMin));

            if (newPos != transform.position)
            {
                newPos = Vector3.Lerp(transform.position, newPos, Time.deltaTime * lerpSpeed);
            }

            transform.position = newPos;
            transform.LookAt(newPos - offset);
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
            transform.position = standbyPos;
        }
    }
}
