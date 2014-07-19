using UnityEngine;
using System.Collections;

public class FollowingCamera : MonoBehaviour {

	public Transform target;

	//CameraMovement
	public float offset;
	Vector3 Position;
	public float smooth= 5.0f;
	float yDistance = 70;

	//CameraZoom
	private float distance;
	private float sensitivityDistance = -7.5f;
	private float damping = 2.5f;
	private float min = -15;
	private float max = -2;
	private Vector3 zdistance;
	
	void Start ()
	{
		//distance = -10;//transform.localPosition.z;
		//zdistance.z = -10;
	}

	void  Update (){

		//CameraZoom
		//distance -= Input.GetAxis("Mouse ScrollWheel");//  sensitivityDistance;
		//distance = Mathf.Clamp(distance, min, max);
		//zdistance.z = Mathf.Lerp(transform.localPosition.z, distance, Time.deltaTime * damping);
		//zdistance.z = distance;

		//transform.localPosition = zdistance;

		//CameraMovement
		Position.x = target.position.x;
		//Position.y = transform.position.y;
		Position.y = target.position.y + yDistance;
		Position.z = target.position.z - offset;
		transform.position = Position;










		//transform.position = Vector3.Lerp (
		//	transform.position, target.position,
		//	Time.deltaTime * smooth);
	} 
	
} 
