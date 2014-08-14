using UnityEngine;
using System.Collections;

public class FogController : MonoBehaviour
{
    public float mapSize = 1000f;
    public Vector2 mapOffset = new Vector2(1009.906f, 1001.565f);
    public Texture2D visionTexture;
    private Color[] visionPixels;
    public float updateInterval = 1f;
    private float updateTimer = 0f;

    public Texture2D fogTexture;

	void Start()
    {
        visionPixels = visionTexture.GetPixels(0, 0, 32, 32);
	}
	
	void Update()
    {
	   updateTimer += Time.deltaTime;

       if (updateTimer >= updateInterval)
       {
            var visions = GameObject.FindGameObjectsWithTag("Vision");
            UpdateFogTexture(visions);
            updateTimer = 0f;
       }
	}

    private void UpdateFogTexture(GameObject[] visions)
    {
        var texture = new Texture2D(128, 128);

        var fog = fogTexture.GetPixels(0, 0, 128, 128);

        texture.SetPixels(fog);

        for (int i = 0; i < visions.Length; i++)
        {
            var visionPos = visions[i].transform.position;
            Debug.Log("Adding vison to " + visionPos.x + "," + visionPos.z);
            var texturePos = new Vector2(visionPos.x, visionPos.z);
            texturePos += mapOffset;
            texturePos /= mapSize;
            texturePos *= 128;
            texturePos -= new Vector2(16, 16);
            int x = Mathf.FloorToInt(Mathf.Clamp(texturePos.x, 0, 96));
            int y = Mathf.FloorToInt(Mathf.Clamp(texturePos.y, 0, 96));
            Debug.Log("Visionn in fog coords " + x + "," + y);
            texture.SetPixels(x, y, 32, 32, visionPixels);
        }

        texture.Apply();

        Shader.SetGlobalTexture("_FogTex", texture);
    }
}
