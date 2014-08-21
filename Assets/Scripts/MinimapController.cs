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

    private List<MinimapIcon> icons;

	void Start () {
        icons = new List<MinimapIcon>();
        camera.pixelRect = new Rect(Screen.width - width, 0, width, height);
	}
	
	void Update () {
        foreach (MinimapIcon icon in icons)
        {
            var pos = camera.WorldToViewportPoint(icon.trackedTransform.position);
            icon.iconTexture.transform.position = pos;
        }
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
