using UnityEngine;
using System.Collections;

public class SkillshotMarkerController : MonoBehaviour {

    private Transform trackingTarget;
    public Vector3 offset = Vector3.zero;
    private GameObject model;

	void Start () {
        model = transform.Find("SkillshotMarkerModel").gameObject;
	}
	
	void Update ()
    {
        if (trackingTarget != null)	
        {
            transform.position = trackingTarget.position + offset;
            var targetPlane = new Plane(Vector3.up, transform.position);
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var hitdist = 0.0f;

            if (targetPlane.Raycast (ray, out hitdist)) 
            {
                var lookAt = ray.GetPoint(hitdist);
                transform.LookAt(lookAt);
            }

            model.renderer.enabled = true;
        }
        else
        {
            model.renderer.enabled = false;
        }
	}

    public void SetTrackingTarget(Transform target)
    {
        trackingTarget = target;
    }
}
