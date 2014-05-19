using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com.Plsr.ImageLoader.Loader;

public class TextureTest : MonoBehaviour {

	float dir = 1;

	void Start() {
		LoadImage();
	}

	void Update() {
		this.transform.Translate(new Vector3(Time.deltaTime * dir, 0 ));
	}

	void OnBecameInvisible()
	{
		dir = -dir;

	}

	void SetTexture(Texture2D tex)
	{
		renderer.material.mainTexture = tex;
	}

	private void LoadImage() {
		Texture2D tex = Resources.Load<Texture2D>("Test");
		renderer.material.mainTexture = tex;

//		ImageBuilder.Load("https://pbs.twimg.com/profile_images/2289316178/0eq2ey32y5grtnfcnfmn.png").Aspect(ImageBuilder.ASPECT_MODE.FIT).Into(this.gameObject, this.renderer.material);
	}

}