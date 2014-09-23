using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MinimapIconType
{
    RedPlayer = 0,
    BluePlayer = 1,
    RedFlag = 2,
    BlueFlag = 3
}

public class MinimapIcon
{
    public Transform trackedTransform;
    public GameObject iconTexture;

    public MinimapIcon(Transform transform, GameObject texture)
    {
        this.trackedTransform = transform;
        this.iconTexture = texture;
    }
}

public class MinimapController : MonoBehaviour
{
    public Object redPlayerIconPrefab;
    public Object bluePlayerIconPrefab;
    public Object redFlagIconPrefab;
    public Object blueFlagIconPrefab;

    public float width = 200f;
    public float height = 200f;
    public float offsetX = 0f;
    public float offsetY = 0f;

    private List<MinimapIcon> icons;
    private GameSettings gameSettings;

    private float lastScreenWidth;
    private float lastUiScale;

	void Start () {
        var gameSettingsObj = GameObject.FindWithTag("GameSettings");
        gameSettings = gameSettingsObj.GetComponent<GameSettings>();
        icons = new List<MinimapIcon>();

        UpdatePixelRect();

        lastScreenWidth = Screen.width;
        lastUiScale = gameSettings.uiScale;
	}

    private void UpdatePixelRect()
    {
        camera.pixelRect = new Rect(Screen.width - (width + offsetX) * gameSettings.uiScale,
                                    0 - offsetY * gameSettings.uiScale,
                                    width * gameSettings.uiScale,
                                    height * gameSettings.uiScale);
    }
	
	void Update () {
        foreach (MinimapIcon icon in icons)
        {
            var pos = camera.WorldToViewportPoint(icon.trackedTransform.position);
            icon.iconTexture.transform.position = pos;
        }

        if (Screen.width != lastScreenWidth || gameSettings.uiScale != lastUiScale)
        {
            UpdatePixelRect();
        }

        lastScreenWidth = Screen.width;
        lastUiScale = gameSettings.uiScale;
	}

    public GameObject Track(Transform transform, MinimapIconType iconType)
    {
        Object prefab = null;
        switch (iconType)
        {
            case MinimapIconType.RedPlayer:
                prefab = redPlayerIconPrefab;
                break;
            case MinimapIconType.BluePlayer:
                prefab = bluePlayerIconPrefab;
                break;
            case MinimapIconType.RedFlag:
                prefab = redFlagIconPrefab;
                break;
            case MinimapIconType.BlueFlag:
                prefab = blueFlagIconPrefab;
                break;
        }
        var icon = (GameObject)Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        icons.Add(new MinimapIcon(transform, icon));
        return icon;
    }
}
