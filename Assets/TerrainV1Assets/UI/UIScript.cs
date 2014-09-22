using UnityEngine;
using System.Collections;

public class UIScript : MonoBehaviour {

	public Texture2D miniMapFrame;
	public Texture2D leftSideFrame;

	public bool ShowUI;

	private int ScreenHeight;
	private int ScreenWidth;
	
	private int lSFx;
	private int lSFy;
	public  float uiScale;
	private float modifiedPositionX;
	private float modifiedPositionY;

	private int lastScreenWidth;
	private int lastScreenHeight;

	// Use this for initialization
	void Start () {
		ScreenHeight = Screen.height;
		ScreenWidth = Screen.width;
		lastScreenHeight = ScreenHeight;
		lastScreenWidth = ScreenWidth;

		lSFx = 0;
		lSFy = ScreenHeight - leftSideFrame.height;

		modifiedPositionX = Screen.width - miniMapFrame.width * uiScale;
		modifiedPositionY = Screen.height - miniMapFrame.height * uiScale;
	}
	void Update () 
	{
		if (lastScreenWidth != Screen.width || lastScreenHeight != Screen.height) 
		
		{
			ScreenHeight = Screen.height;
			ScreenWidth = Screen.width;
			lSFy = ScreenHeight - leftSideFrame.height;
		}
	}

	void OnGUI()
	{
		if (ShowUI == true) 
		{
			//GUI.depth = 10;
			GUI.DrawTexture (new Rect(modifiedPositionX, modifiedPositionY, miniMapFrame.width * uiScale, miniMapFrame.height * uiScale ), miniMapFrame);
			GUI.DrawTexture (new Rect(lSFx, lSFy, leftSideFrame.width * uiScale, leftSideFrame.height * uiScale), leftSideFrame);
		}
	}
}
