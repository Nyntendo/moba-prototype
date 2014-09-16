using UnityEngine;
using System.Collections;

public class ScrollingTexture : MonoBehaviour {

	public float scrollSpeed = 0.5f;
	public float offset;
	// Use this for initialization

	// Update is called once per frame
	void Update () {
		offset += (Time.deltaTime*scrollSpeed)/10.0f;
		renderer.material.SetTextureOffset("_MainTex", new Vector2(0, offset));
	}
}
