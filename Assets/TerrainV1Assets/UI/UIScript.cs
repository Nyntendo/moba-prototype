using UnityEngine;
using System.Collections;

public class UIScript : MonoBehaviour {

	public Texture2D Frame;
	public bool ShowUI;
	private int ScreenHeight;
	private int ScreenWidth;

	private int lastScreenWidth;
	private int lastScreenHeight;

	// Use this for initialization
	void Start () {
		ScreenHeight = Screen.height;
		ScreenWidth = Screen.width;
		lastScreenHeight = ScreenHeight;
		lastScreenWidth = ScreenWidth;
	}
	void Update () 
	{
		if (lastScreenWidth != Screen.width || lastScreenHeight != Screen.height) 
		
		{
			ScreenHeight = Screen.height;
			ScreenWidth = Screen.width;
		}
	}

	void OnGUI()
	{
		if (ShowUI == true) 
		{
			GUI.depth = 10;
			GUI.DrawTexture (new Rect (0, 0, ScreenWidth, ScreenHeight), Frame);
		}
	}
}
