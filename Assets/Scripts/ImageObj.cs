using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class ImageObj : MonoBehaviour {

	string _url;
	bool _loaded;

	// Use this for initialization
//	void Initialize (string url) {
//
//		_url = url;
////		if (renderer.isVisible)
////		{
////			Debug.Log("adding image");
////
////		}
////		else
////			_invisibleObjs.Add(this);
//		
////		renderer.enabled = false;
//	}

	void Awake()
	{
//		renderer.enabled = false;
	}

	void OnBecameVisible()
	{
		if (!_loaded)
			ImageManager.Instance.AddObjToLoad(this);
	}

	void OnBecameInvisible()
	{
		if (!_loaded)
			ImageManager.Instance.AddObjToQueue(this);

	//	Debug.Log("became invisible: " + transform.position + " viewport" + Camera.main.WorldToViewportPoint(transform.position));
	}


	public IEnumerator LoadTexture(string url)
	{
		
		WWW imageWWW = new WWW (url);
		
//		Debug.Log("loadig url: " + url);
		
		yield return imageWWW;
		
		if (string.IsNullOrEmpty(imageWWW.error))
		{
			renderer.enabled = true;
			renderer.material.mainTexture = imageWWW.texture;
			
			float aspectRatio = imageWWW.texture.width / imageWWW.texture.height;
			Vector3 scale = transform.localScale;
			scale.x *= aspectRatio;
			transform.localScale = scale;
		
			LeanTween.rotateX(gameObject,0,1).setEase(LeanTweenType.easeOutElastic);
			
		}

		_loaded = true;
//		_loadingObjs.Remove(this);

	}

	
}
