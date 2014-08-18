using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public Transform target;
    private Vector3 currentLookAt = Vector3.zero;
    public float lerpSpeed = 1f;
    public Vector3 offset;
    public float offsetYMin = 10f;
    public float offsetYMax = 100f;
    public float zoomSpeed = 10f;
    public bool locked = true;
    public float scrollSpeed = 10f;

    public Transform topLeftCorner;
    public Transform bottomRightCorner;

    public Vector3 standbyPos;

    void Update () {
        if (Input.GetButtonUp("Camera lock"))
        {
            locked = !locked;
        }

        offset.y -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        offset.y = Mathf.Clamp(offset.y, offsetYMin, offsetYMax);

        if (target != null && locked)
        {
            var desiredLookAt = target.position;

            if (currentLookAt != desiredLookAt)
            {
                currentLookAt = Vector3.Lerp(currentLookAt, desiredLookAt, Time.deltaTime * lerpSpeed);
            }

        }
        else
        {
            if (Input.mousePosition.x < 10)
            {
                currentLookAt += Vector3.left * scrollSpeed * Time.deltaTime;
            }
            if (Input.mousePosition.x > (Screen.width - 10))
            {
                currentLookAt += Vector3.right * scrollSpeed * Time.deltaTime;
            }
            if (Input.mousePosition.y < 10)
            {
                currentLookAt += Vector3.back * scrollSpeed * Time.deltaTime;
            }
            if (Input.mousePosition.y > (Screen.height - 10))
            {
                currentLookAt += Vector3.forward * scrollSpeed * Time.deltaTime;
            }
        }

        currentLookAt = new Vector3(
            Mathf.Clamp(currentLookAt.x, topLeftCorner.position.x, bottomRightCorner.position.x),
            currentLookAt.y,
            Mathf.Clamp(currentLookAt.z, bottomRightCorner.position.z, topLeftCorner.position.z));

        transform.position = currentLookAt + offset;
        transform.LookAt(currentLookAt);

    }
}
