using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FogController : MonoBehaviour
{
    public Vector3 visionOffset;
    public float mapSize = 1000f;
    public float fogSize = 128f;
    public float visionExpand = 10f;
    public Vector2 mapOffset = new Vector2(1009.906f, 1001.565f);
    public Texture2D visionTexture;
    private Color[] visionPixels;
    public float updateInterval = 1f;
    private float updateTimer = 0f;
    public float visionRange = 100f;

    public Texture2D fogTexture;
    private Texture2D lastFogTexture;
    public Team team;

    public LayerMask visionRaycastMask;
    public LayerMask lineOfSightRaycastMask;

	void Start()
    {
        var gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        team = gameController.MyPlayer.team;
        visionPixels = visionTexture.GetPixels(0, 0, 32, 32);
        lastFogTexture = fogTexture;
        Shader.SetGlobalTexture("_FogTex", fogTexture);
        Shader.SetGlobalTexture("_LastFogTex", fogTexture);
        Shader.SetGlobalFloat("_FogBlend", 0f);
	}
	
	void Update()
    {
	   updateTimer += Time.deltaTime;
       Shader.SetGlobalFloat("_FogBlend", updateTimer / updateInterval);

       if (updateTimer >= updateInterval)
       {
            var visions = new List<GameObject>(GameObject.FindGameObjectsWithTag("Vision"));
            var myVisions = visions.Where(v => v.GetComponent<VisionController>().team == team);
            UpdateFogTexture(myVisions.ToArray<GameObject>());
            updateTimer = 0f;
            UpdateUnitVision();
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
        Shader.SetGlobalTexture("_LastFogTex", lastFogTexture);
        Shader.SetGlobalFloat("_FogBlend", 0f);

        lastFogTexture = texture;
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
            if (Physics.Raycast(pos + visionOffset, direction, out hit, visionRange, visionRaycastMask))
            {
                Debug.DrawLine(pos + visionOffset, hit.point, Color.yellow);
                relPos = hit.point + direction * visionExpand - pos;
            }
            else
            {
                relPos = direction * (visionRange + visionExpand);
            }

            var texPos = new Vector2(relPos.x, relPos.z);
            texPos /= visionRange;
            texPos *= 16;
            texPos += new Vector2(16, 16);
            poly[i] = texPos;
        }

        return poly;
    }

    private void UpdateUnitVision()
    {
        var friendlyVisions = new List<GameObject>();
        var enemyUnits = new List<GameObject>();

        var visions = GameObject.FindGameObjectsWithTag("Vision");

        for (int i=0; i< visions.Length; i++)
        {
            var ctrl = visions[i].GetComponent<VisionController>();
            if (ctrl.team == team)
            {
                friendlyVisions.Add(visions[i]);
            }
        }

        var heroes = GameObject.FindGameObjectsWithTag("Hero");

        for (int i=0; i < heroes.Length; i++)
        {
            enemyUnits.Add(heroes[i]);
            heroes[i].SendMessage("SetVisible", false);
        }

        var creeps = GameObject.FindGameObjectsWithTag("Creep");

        for (int i=0; i< creeps.Length; i++)
        {
            enemyUnits.Add(creeps[i]);
            creeps[i].SendMessage("SetVisible", false);
        }

        RaycastHit hit;
        foreach (GameObject vision in friendlyVisions)
        {
            foreach (GameObject enemy in enemyUnits)
            {
                var direction = enemy.transform.position - vision.transform.position;
                if (direction.magnitude <= visionRange)
                {
                    if (!Physics.Raycast(vision.transform.position + visionOffset, direction, out hit, direction.magnitude, lineOfSightRaycastMask))
                    {
                        enemy.SendMessage("SetVisible", true);
                    }
                }
            }
        }
    }

}
