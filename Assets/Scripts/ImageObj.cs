using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ImageObj : MonoBehaviour {

	public Texture2D DefaultImage;
	public GameObject CommunityPrefab;
	public Renderer ImageRenderer;
	public Texture2D WhiteTexture;
	public TextMesh Text;
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

	Vector3 _aspectScale;

	Vector3 _cubeScale;
	Vector3 _savedPos;
	Vector3 _savedRotation;

	Transform _imageBox;
	float _startXScale;

	static Dictionary<string,Texture2D>  _imageCache = new Dictionary<string, Texture2D>();
	static List<ImageObj> _visibleObjs = new List<ImageObj>();
	

	void Awake()
	{
		CubeRotator.SetActive(false);
//		transform.localScale = Vector3.one * .3f;
		_imageBox = transform.FindChild("box");

		_imageBox.localScale = new Vector3(.001f,_imageBox.localScale.y,_imageBox.localScale.z);
		_startXScale = _imageBox.localScale.x;
//		_startRotation = _imagePlane.rotation.eulerAngles;


		Text.gameObject.SetActive(false);

		if (DefaultImage != null)
		{
			Initialize(DefaultImage, null);
		}

	}

	public void Initialize(Texture2D tex, Dictionary<string,object> data)
	{
		if (this == null)
			return;
		

		_data = data;
		
		ImageRenderer.material.mainTexture = tex;
	
		
		float aspectRatio = (float)tex.width / (float)tex.height;

		Vector3 scale = transform.localScale;
		
		_cubeScale = scale;
		
		scale.x *= aspectRatio;
		
		if (scale.x > MaxWidthScale)
		{
			scale *= MaxWidthScale / scale.x;
		}
		
		//	LeanTween.moveLocal(_imageBox.gameObject,Vector3.zero,1);
		transform.localScale = scale;
		
		_aspectScale = scale;
	}

#region community transitions

	public GameObject DoCommunityForwardTransition(Vector3 finalCameraPos)
	{
		if (SceneManager.Instance.GetScene() == Scene.InTransition)
			return null;

		CubeRotator.SendMessage("DoFastRotateToFront");
		SceneManager.Instance.SetTransitioning(true);

		GameObject community = (GameObject)Instantiate(CommunityPrefab,new Vector3(finalCameraPos.x, 0, finalCameraPos.z + 5),Quaternion.identity);

		StartCoroutine(FadeMaterial(ImageRenderer.material,.5f,0,() => {
			Debug.Log("stopped transitioning");
			SceneManager.Instance.SetTransitioning(false);
		}));

		return community;
	}

	public void DoCommunityBackTransition()
	{
		if (SceneManager.Instance.GetScene() == Scene.InTransition)
			return;
		
		SceneManager.Instance.SetTransitioning(true);

		StartCoroutine(FadeMaterial(ImageRenderer.material,2,1,() => {
			Debug.Log("stopped transitioning");
			SceneManager.Instance.SetTransitioning(false);
		}));
		
	}

#endregion

	IEnumerator FadeMaterial(Material mat, float fadeTime, float alpha, Action onComplete )
	{

		float cycleTime = .05f;
		float timeLeft = fadeTime;
		float alphaChange = alpha - mat.color.a;
		float numCycles = fadeTime / cycleTime;

		while (timeLeft > 0)
		{
			Color c = mat.color;
			c.a += alphaChange / numCycles;
			mat.color = c;

			yield return new WaitForSeconds(cycleTime);
			timeLeft -= cycleTime;
		}

		if (onComplete != null)
			onComplete();

	}

	void UpdateRoation()
	{
		if (gameObject == null || SceneManager.Instance.GetScene() != Scene.Browse)
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
	//	ImageRenderer.materials[1].mainTexture = WhiteTexture;
		Text.gameObject.SetActive(false);
		Debug.Log("scale x: " + transform.localScale.x);
		float animTime = .3f;
		Vector3 newPos = Camera.main.ViewportToWorldPoint(new Vector3(.5f,.5f,2));
		LeanTween.move(gameObject,newPos ,animTime).setEase(LeanTweenType.easeOutQuad);
		LeanTween.rotateLocal(gameObject,Vector3.zero, animTime).setEase(LeanTweenType.easeOutQuad).setOnComplete ( () =>
		                                                                                                               {
			SceneManager.Instance.PushScene(Scene.Selected);
		
			Debug.Log("cubescale: " + _cubeScale);
		});

		LeanTween.rotateLocal(ImageRenderer.gameObject,new Vector3(0,270,0), animTime).setEase(LeanTweenType.easeOutQuad);
		ScaleToCube();

		CubeRotator.SetActive(true);
		CubeRotator.SendMessage("OnSelect");

		if (_data != null && _data.ContainsKey("LargeUrl"))
		{
			var textureCache = WebTextureCache.InstantiateGlobal ();

			Debug.Log ("loading large url: " + (string)_data["LargeUrl"]);
			StartCoroutine (textureCache.GetTexture ((string)_data["LargeUrl"], _data, OnGotLargeTexture));
				


		}
	}

	void OnUnselected()
	{
		ScaleToAspect(.3f);
		if (SceneManager.Instance.GetTransitioningToScene() == Scene.Helix)
			Text.gameObject.SetActive(true);
		Debug.Log("imageobj unselect");
		CubeRotator.SendMessage("OnUnselect");
		CubeRotator.SetActive(false);
		ScaleToPlane(.3f);
	//	ImageRenderer.materials[1].mainTexture = ImageRenderer.material.mainTexture;

	}
	
	void OnGotLargeTexture(string url, Dictionary<string,object> data , Texture2D largeTex )
	{
		ImageRenderer.material.mainTexture = largeTex;
	}
	

	public void MoveToSavedPlace(float animTime)
	{
		Vector3 viewportPos = Camera.main.WorldToViewportPoint(_savedPos);
		bool startPosIsVisible = (viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1);

		if (IsVisible() || startPosIsVisible)
		{
			if (_savedPos != transform.position)
			{
			//	transform.position = transform.position + Random.onUnitSphere * 100;
				LeanTween.moveLocal(gameObject,_savedPos,animTime).setEase(LeanTweenType.easeOutExpo);
				LeanTween.rotateLocal(gameObject,_savedRotation,animTime).setEase(LeanTweenType.easeOutExpo);

			}
			else
				Debug.Log("obj pos didn't change");
		}
		else
		{
			transform.localPosition = _savedPos;
			transform.localRotation = Quaternion.Euler(_savedRotation);
		}
	}

	public void FadeOut(float time)
	{
		StartCoroutine(FadeMaterial(ImageRenderer.material,time,0,null));
		
	}
	
	public void FadeIn(float time)
	{
		StartCoroutine(FadeMaterial(ImageRenderer.material,time,1,null));
		
	}
	
	public void SetAlpha(float alpha)
	{
		Color c = ImageRenderer.material.color;
		c.a = alpha;
		ImageRenderer.material.color = c;
		
	}

	public void ScaleToCube()
	{
		LeanTween.scaleX(_imageBox.gameObject,CubeXScale,.3f);
	}
	
	public void ScaleToPlane(float time)
	{
		Debug.Log("scaling to plane " + time);
		LeanTween.scaleX(_imageBox.gameObject,_startXScale,time);
		
	}
	
	void ScaleToBox(float animTime)
	{
		Debug.Log("scaling to Box " + _cubeScale);
		//	transform.localScale = _cubeScale;
		LeanTween.scale(gameObject,_cubeScale,.1f);
	}
	
	void ScaleToAspect(float animTime)
	{
		Debug.Log("scaling to Aspect " + _aspectScale);
		//		transform.localScale = _aspectScale;
		LeanTween.scale(gameObject,_aspectScale,.1f);
		
	}

	public void SavePlace()
	{
		_savedPos = transform.localPosition;
		_savedRotation = transform.localRotation.eulerAngles;
	}
	
	public void HideText()
	{
		Text.gameObject.SetActive(false);
	}
	
	public void SetText(string key)
	{
		//		Debug.Log("setting key: " + key);
		
		Text.gameObject.SetActive(true);
		
		if (key == "Price")
		{
			decimal d = new decimal((float)_data[key]);
			Text.text = "" +  decimal.Round(d,2).ToString ();
		}
		else if (key == "Popularity")
		{
			//			float val = (float)_data[key];
			//			string text = ((int)(1.0f/val)).ToString();
			Text.text = "";
		}
		else if (key == "ExpertRating" || key == "BuyerRating")
		{
			float val = (float)_data[key];
			
			string stars = "";
			
			for (int i=0; i < (int)val; i++)
				stars = stars + "";
			
			float last = val - Mathf.Floor(val);
			//		Debug.Log("last: " + last);
			if (last > .33f)
				stars = stars + "";
			
			Text.text = stars;
		}
		else if (key == "Availability")
		{
			Text.text = "";//((int)(float)_data[key]).ToString();
		}
	}
	
	public T GetData<T>(string key)
	{

		if (_data.ContainsKey(key) == false)
		{
			Debug.Log("key: " + key + " has null value");
		}
		return (T)_data[key];
	}

	public bool HasData()
	{
		return _data != null;
	}
	

	public static List<ImageObj> GetVisibleObjs()
	{
		return _visibleObjs;
	}
	

	public bool IsCommunity()
	{
		return CommunityPrefab != null;

	}
	

	public void SetVisible(bool visible)
	{
		ImageRenderer.enabled = visible;
	}
	
	public bool IsVisible()
	{
		return ImageRenderer.isVisible;
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

}
