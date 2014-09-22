using UnityEngine;
using System.Collections;

public class MoveMarkerController : MonoBehaviour
{
    private float timer = 0.0f;
    public float displayTime = 2.0f;
    public Vector3 offset;

	void Start ()
    {
	
	}

    public void SetPosition(Vector3 position)
    {
        transform.position = position + offset;
        timer = displayTime;
    }
	
	void FixedUpdate ()
    {
        if (timer > 0f)
        {
            timer -= Time.fixedDeltaTime;

            if (timer < 0f)
            timer = 0f;

            var scale = timer / displayTime;

            transform.localScale = new Vector3(scale, 0f, scale);
        }
	
	}
}
