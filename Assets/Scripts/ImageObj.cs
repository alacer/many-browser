using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class ImageObj : MonoBehaviour {

	public Renderer ImageRenderer;
	public float MoveSpeed = 1;
	public float RotateDistance = 3;
	public float MinDistToCamera = 1;

	static float MaxZOffset = .1f;
	bool _isFacingCamera;

	string _url;
	bool _loaded;
	
	Vector3 _totalZMovement;
	Vector3 _moveDelta;

	Vector3 _startScale;
	Transform _imagePlane;

	static Dictionary<string,Texture2D>  _imageCache = new Dictionary<string, Texture2D>();
	static List<ImageObj> _visibleObjs = new List<ImageObj>();
	

	void Start()
	{
//		transform.localScale = Vector3.one * .3f;
		_imagePlane = transform.Find("plane");
//		_startRotation = _imagePlane.rotation.eulerAngles;
		_imagePlane.localPosition += Random.onUnitSphere * 100;
	
		_startScale = _imagePlane.localScale;
	}

	public static List<ImageObj> GetVisibleObjs()
	{
		return _visibleObjs;
	}

	void OnBecameVisible()
	{
		if (!_loaded)
		{
			ImageManager.Instance.AddObjToLoad(this);
		}

		_visibleObjs.Add(this);
	}

	void OnBecameInvisible()
	{
		if (!_loaded)
			ImageManager.Instance.AddObjToQueue(this);

		if (_visibleObjs.Contains(this))
			_visibleObjs.Remove(this);
	}

	public void SetVisible(bool visible)
	{
		ImageRenderer.enabled = visible;
	}

	public bool IsVisible()
	{
		return ImageRenderer.isVisible;
	}

	void UpdateRoation()
	{
		if (SceneManager.Instance.GetScene() != Scene.Browse)
			return;


		float dist = Vector3.Distance(transform.position, Camera.main.transform.position);

		if (dist < RotateDistance && !_isFacingCamera)
		{
			FaceCamera();
		}
		else if (dist > RotateDistance && _isFacingCamera)
		{
			FaceOriginalDirection();
		}

	}

	void FaceCamera()
	{
		LeanTween.rotate(gameObject,Camera.main.transform.rotation.eulerAngles,.2f);

		_isFacingCamera = true;
	}

	void FaceOriginalDirection()
	{
		LeanTween.rotate(gameObject,Vector3.zero,.2f);
		_isFacingCamera = false;
	}
	

	void Update()
	{

		UpdateRoation();

	}

	public void SetMoveDelta(Vector3 moveDelta)
	{
		_moveDelta = moveDelta;
	}

//	public void GetImage(string url)
//	{
//		
//		var textureCache = WebTextureCache.InstantiateGlobal ();
//		StartCoroutine (textureCache.GetTexture (url, SetTexture));
//	}
	

//	public IEnumerator LoadTexture(string url)
//	{
//
//
//		while (ImageManager.Instance.GetFPS() < 40)
//		{
//			// don't load if the framerate is too low
//			yield return new WaitForSeconds(Random.Range(.1f,.3f));
//		}
//
//
//
//		// do we already have this image downloaded? if so Just set it. 
//		if (_imageCache.ContainsKey(url))
//		{
//			SetTexture(_imageCache[url]);
//
//		}
//		else // otherwise, download it and set it
//		{
//			WWW imageWWW = new WWW (url);
//
//		
//			yield return imageWWW;
//			
//			if (string.IsNullOrEmpty(imageWWW.error))
//			{
//				_imageCache[url] = imageWWW.texture;
//				SetTexture(imageWWW.texture);
//				
//			}
//			else
//			{
//				if (_imageCache.Count > 0)
//				{
//					List<string> keyList = new List<string>(_imageCache.Keys);
//					SetTexture(_imageCache[keyList[Random.Range(0,keyList.Count)]]);
//				}
//				else
//					Debug.Log("didnt get image");
//			}
//		}
//
//		_loaded = true;
//
//	}

	public void SetTexture(Texture2D tex)
	{
		if (this == null)
			return;

		ImageRenderer.material.mainTexture = tex;
		
		float aspectRatio = tex.width / tex.height;
		Vector3 scale = transform.localScale;
		scale.x *= aspectRatio;

		Debug.Log("setting local pos");
//		_imagePlane.localPosition = Vector3.zero;
		LeanTween.moveLocal(_imagePlane.gameObject,Vector3.zero,1);
	//	transform.localScale = scale;
	//	renderer.enabled = true;
	//	LeanTween.rotateX(gameObject,0,1).setEase(LeanTweenType.easeOutElastic);

//		LeanTween.scale(gameObject,Vector3.one,1).setEase(LeanTweenType.easeInOutElastic);
	}

	
}
