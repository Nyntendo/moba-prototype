using UnityEngine;
using System.Collections;

public class FogController : MonoBehaviour
{
    public Vector3 visionOffset;
    public float mapSize = 1000f;
    public float fogSize = 128f;
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
        var texturePixels = texture.GetPixels();

        for (int i = 0; i < visions.Length; i++)
        {
            var visionPos = visions[i].transform.position;
            var texturePos = new Vector2(visionPos.x, visionPos.z);
            texturePos += mapOffset;
            texturePos /= mapSize;
            texturePos *= fogSize;
            texturePos -= new Vector2(16, 16);
            int x = Mathf.FloorToInt(texturePos.x);
            int y = Mathf.FloorToInt(texturePos.y);
            AddPixels(texturePixels, visionPixels, x, y, GetVisionPoly(visions[i].transform.position));
        }
        texture.SetPixels(texturePixels);

        texture.Apply();

        Shader.SetGlobalTexture("_FogTex", texture);
    }

    private void AddPixels(Color[] bg, Color[] fg, int xOffset, int yOffset, Vector2[] visionPoly)
    {
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                if (ContainsPoint(visionPoly, new Vector2(x, y)))
                {
                    if ((x + xOffset) < 128 && (x + xOffset) >= 0 &&
                        (y + yOffset) < 128 && (y + yOffset) >= 0)
                    {
                        var bgIndex = (y + yOffset) * 128 + x + xOffset;
                        var fgIndex = y * 32 + x;
                        bg[bgIndex] += fg[fgIndex] * fg[fgIndex].a;
                    }
                }
            }
        }
    }

    private static bool ContainsPoint (Vector2[] polyPoints, Vector2 p)
    { 
       var j = polyPoints.Length - 1; 
       var inside = false; 
       for (int i = 0; i < polyPoints.Length; j = i++) { 
          if ( ((polyPoints[i].y <= p.y && p.y < polyPoints[j].y) || (polyPoints[j].y <= p.y && p.y < polyPoints[i].y)) && 
             (p.x < (polyPoints[j].x - polyPoints[i].x) * (p.y - polyPoints[i].y) / (polyPoints[j].y - polyPoints[i].y) + polyPoints[i].x)) 
             inside = !inside; 
       } 
       return inside; 
    }

    private Vector2[] GetVisionPoly(Vector3 pos)
    {
        var poly = new Vector2[18];
        RaycastHit hit;
        for (int i = 0; i < 18; i++)
        {
            var angle = i * 20f;
            var direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            var relPos = Vector3.zero;
            if (Physics.Raycast(pos + visionOffset, direction, out hit, 300f))
            {
                relPos = hit.point - pos;
            }
            else
            {
                relPos = direction * 300f;
            }

            var texPos = new Vector2(relPos.x, relPos.z);
            texPos /= 300f;
            texPos *= 32;
            texPos += new Vector2(16, 16);
            poly[i] = texPos;
        }

        return poly;
    }

}
