using UnityEngine;
using System.Collections;
using Com.Plsr.ImageLoader.Loader;

public class TextureTestWithCallback : MonoBehaviour {

	void Start() {
		LoadImage();
	}

	void Update () {
//		this.transform.Translate(new Vector3(0, 0, Time.deltaTime));
	}

	void SetTexture(Texture2D tex)
	{
		renderer.material.mainTexture = tex;
	}

	private void Callback(WWW www) {
		// Do stuff with www.bytes or anything else?
		this.renderer.material.mainTexture = www.texture;
		Debug.Log("image loaded");
	}

	private void LoadImage() {
		Texture2D tex = Resources.Load<Texture2D>("Test");
		renderer.material.mainTexture = tex;
//		ImageBuilder.Load("https://pbs.twimg.com/profile_images/2289316178/0eq2ey32y5grtnfcnfmn.png").Aspect(ImageBuilder.ASPECT_MODE.FIT).Into(Callback);
	}
}
