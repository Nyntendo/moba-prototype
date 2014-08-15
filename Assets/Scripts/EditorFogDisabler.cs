using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class EditorFogDisabler : MonoBehaviour {

    public Texture2D fogTexture;
	
	void Update ()
    {
        if (!Application.isPlaying)
        {
            Shader.SetGlobalTexture("_FogTex", fogTexture);
            Shader.SetGlobalTexture("_LastFogTex", fogTexture);
        }
	}
}
