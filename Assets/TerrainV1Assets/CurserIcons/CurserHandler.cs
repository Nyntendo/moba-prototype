using UnityEngine;
using System.Collections;

public class CurserHandler : MonoBehaviour {


	public Texture2D normalCursor;
	public Texture2D clickedCursor;
	private Texture2D currentCursor;
	public int cursorSize = 32;

	public Camera rayCamera;
	public GameObject MoveMarker;
	private Ray ray;
	private RaycastHit hit;
	
	void Start () {
		Screen.showCursor = false;
		currentCursor = normalCursor;
	}

	void Update()
	{
		if (Input.GetMouseButtonUp (0)) 
		{
			OnLeftMouseUp();
		}

		if (Input.GetMouseButtonDown (0)) 
		{
			OnLeftMouseDown();
		}

		if (Input.GetMouseButtonUp (1)) 
		{
			OnLeftMouseUp();
		}

		if (Input.GetMouseButtonDown (1)) 
		{
			OnRightMouseDown();
		}
	}

	void OnGUI() {
		GUI.DrawTexture(new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y, cursorSize, cursorSize), currentCursor);
	}

	void OnLeftMouseUp()
	{
		currentCursor = normalCursor;
		cursorSize = 32;
	}

	void OnLeftMouseDown()
	{
		cursorSize = 24;
	}

	void onRightMouseUp()
	{
		currentCursor = normalCursor;
		cursorSize = 32;
	}

	void OnRightMouseDown()
	{
		cursorSize = 24;
		ray = rayCamera.ScreenPointToRay (Input.mousePosition);
		if (Physics.Raycast (ray, out hit)) 
		{
			GameObject obj = Instantiate (MoveMarker, new Vector3(hit.point.x, hit.point.y + 1, hit.point.z), Quaternion.identity) as GameObject;
		}
	}


}
