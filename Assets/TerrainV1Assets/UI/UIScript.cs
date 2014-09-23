using UnityEngine;
using System.Collections;

public class UIScript : MonoBehaviour {

	public Texture2D miniMapFrame;
	public Texture2D leftSideFrame;

	public bool ShowUI;
	private GameSettings gameSettings;

	void Start()
	{
        var gameSettingsObj = GameObject.FindWithTag("GameSettings");
        gameSettings = gameSettingsObj.GetComponent<GameSettings>();
	}

	void OnGUI()
	{
		if (ShowUI == true) 
		{
			GUI.DrawTexture(
				new Rect(0, Screen.height - leftSideFrame.height * gameSettings.uiScale,
						 leftSideFrame.width * gameSettings.uiScale,
						 leftSideFrame.height * gameSettings.uiScale), leftSideFrame);
			GUI.DrawTexture(
				new Rect(Screen.width - miniMapFrame.width * gameSettings.uiScale,
						 Screen.height - miniMapFrame.height * gameSettings.uiScale,
						 miniMapFrame.width * gameSettings.uiScale,
						 miniMapFrame.height * gameSettings.uiScale ), miniMapFrame);
		}
	}
}
