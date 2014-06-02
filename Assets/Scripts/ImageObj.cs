using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class ImageObj : MonoBehaviour {


	public Renderer ImageRenderer;
	public float MoveSpeed = 1;
	public float RotateDistance = 3;
	public float MinDistToCamera = 1;
	public float CubeXScale = 1344;
	public GameObject CubeRotator;
	public float MaxWidthScale = 1.2f;

	Dictionary<string,object> _data;
	static float MaxZOffset = .1f;
	bool _isFacingCamera;
	
	bool _loaded;
	
	Vector3 _totalZMovement;
	Vector3 _moveDelta;

	Vector3 _startScale;
	Transform _imageBox;
	float _startXScale;

	static Dictionary<string,Texture2D>  _imageCache = new Dictionary<string, Texture2D>();
	static List<ImageObj> _visibleObjs = new List<ImageObj>();
	

	void Awake()
	{
		CubeRotator.SetActive(false);
//		transform.localScale = Vector3.one * .3f;
		_imageBox = transform.FindChild("box");
		_startXScale = _imageBox.localScale.x;
//		_startRotation = _imagePlane.rotation.eulerAngles;
		_imageBox.localPosition += Random.onUnitSphere * 100;
	
		_startScale = _imageBox.localScale;
	}

	public static List<ImageObj> GetVisibleObjs()
	{
		return _visibleObjs;
	}

	public void ScaleToCube()
	{
		LeanTween.scaleX(_imageBox.gameObject,CubeXScale,.3f);
	}

	public void ScaleToPlane()
	{
		LeanTween.scaleX(_imageBox.gameObject,_startXScale,.3f);

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

	void OnSelected()
	{

		Debug.Log("scale x: " + transform.localScale.x);
		float animTime = .3f;
		Vector3 newPos = Camera.main.ViewportToWorldPoint(new Vector3(.5f,.5f,2));
		LeanTween.move(gameObject,newPos ,animTime).setEase(LeanTweenType.easeOutQuad);
		LeanTween.rotateLocal(gameObject,Vector3.zero, animTime).setEase(LeanTweenType.easeOutQuad).setOnComplete ( () =>
		                                                                                                               {
			SceneManager.Instance.PushScene(Scene.Selected);
		});

		LeanTween.rotateLocal(ImageRenderer.gameObject,new Vector3(0,270,0), animTime).setEase(LeanTweenType.easeOutQuad);

		CubeRotator.SetActive(true);
		CubeRotator.SendMessage("OnSelect");
		ScaleToCube();


		if (_data.ContainsKey("LargeUrl"))
		{
			var textureCache = WebTextureCache.InstantiateGlobal ();

			StartCoroutine (textureCache.GetTexture ((string)_data["LargeUrl"], _data, OnGotLargeTexture));
				


		}
	}

	void OnUnselected()
	{
		CubeRotator.SendMessage("OnUnselect");
		CubeRotator.SetActive(false);
		ScaleToPlane();

	}

	void OnGotLargeTexture(string url, Dictionary<string,object> data , Texture2D largeTex )
	{
		ImageRenderer.material.mainTexture = largeTex;
	}

	public void Initialize(Texture2D tex, Dictionary<string,object> data)
	{
		if (this == null)
			return;

//		if (tex == null)
//			Debug.LogError("texture is null for url " + url);

		_data = data;

		ImageRenderer.material.mainTexture = tex;


		float aspectRatio = (float)tex.width / (float)tex.height;
		Vector3 scale = transform.localScale;
		scale.x *= aspectRatio;

		if (scale.x > MaxWidthScale)
		{
			scale *= MaxWidthScale / scale.x;
		}
//		_imagePlane.localPosition = Vector3.zero;
		LeanTween.moveLocal(_imageBox.gameObject,Vector3.zero,1);
		transform.localScale = scale;
	//	renderer.enabled = true;
	//	LeanTween.rotateX(gameObject,0,1).setEase(LeanTweenType.easeOutElastic);

//		LeanTween.scale(gameObject,Vector3.one,1).setEase(LeanTweenType.easeInOutElastic);
	}

	
}
