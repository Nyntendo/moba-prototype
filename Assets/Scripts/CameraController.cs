using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public Transform target;
    private Vector3 currentLookAt = Vector3.zero;
    public float lerpSpeed = 1f;
    public Vector3 offset;
    public float xMax;
    public float xMin;
    public float zMax;
    public float zMin;
    public float offsetYMin = 10f;
    public float offsetYMax = 100f;
    public float zoomSpeed = 10f;

    public Vector3 standbyPos;

    void Update () {
        if (target != null)
        {
            offset.y -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
            offset.y = Mathf.Clamp(offset.y, offsetYMin, offsetYMax);

            var desiredLookAt = new Vector3(
                Mathf.Clamp(target.position.x, xMin, xMax),
                target.position.y,
                Mathf.Clamp(target.position.z, zMin, zMax));

            if (currentLookAt != desiredLookAt)
            {
                currentLookAt = Vector3.Lerp(currentLookAt, desiredLookAt, Time.deltaTime * lerpSpeed);
            }

            transform.position = currentLookAt + offset;
            transform.LookAt(currentLookAt);
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
            transform.position = standbyPos;
        }
    }
}
