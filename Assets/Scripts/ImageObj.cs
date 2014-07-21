using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ImageObj : MonoBehaviour {

	public string SearchName;
	public Texture2D DefaultImage;
	public bool IsCommunityItem = false;

	public GameObject FavoritesButtonMesh;
	public Material[] ReplacementCommunityMaterials;
	public Material PageContentMaterial;
	public Shader TransparentShader;
	public Shader OpaqueShader;
	public Shader BlendShader;
	public float CommunityZDepth = 4;
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


	Dictionary<string,object> _data = new Dictionary<string, object>();

	bool _isFacingCamera;
	
	bool _loaded;
	
	Vector3 _totalZMovement;

	Vector3 _bookScale;

	Vector3 _cubeScale;
	Vector3 _savedPos;
	Vector3 _savedRotation;

	Transform _imageBox;
	float _startXScale;
	
	static List<ImageObj> _visibleObjs = new List<ImageObj>();

	Material[] _planeMaterials;
	Material[] _boxMaterials;
	bool _zoomedIn;


	void Awake()
	{
		if (IsCommunityItem)
		{
			ImageRenderer.materials = ReplacementCommunityMaterials;
		}

		if (transform.Find("box").FindChild("FavoritesButton") == null)
		{
			GameObject favoritesButton = (GameObject) Instantiate( Resources.Load("Prefabs/FavoritesButton") );
			favoritesButton.transform.parent = transform.Find("box").transform;

			favoritesButton.transform.localPosition = Vector3.zero;
			favoritesButton.transform.localRotation = Quaternion.identity;
			favoritesButton.transform.localScale = Vector3.one;
			favoritesButton.collider.enabled = false;
		}

		CubeRotator.SetActive(false);
		_boxMaterials = ImageRenderer.materials;
		_planeMaterials = new Material[] { ImageRenderer.materials[0], ImageRenderer.materials[1] };
		ImageRenderer.materials = _planeMaterials;


		_imageBox = transform.FindChild("box");

		_imageBox.localScale = new Vector3(.001f,_imageBox.localScale.y,_imageBox.localScale.z);
		_startXScale = _imageBox.localScale.x;

		if (Text != null)
			Text.gameObject.SetActive(false);

		if (DefaultImage != null)
		{
			Initialize(DefaultImage);
		}

	}

	public void InitData(Dictionary<string,object> data)
	{
		_data = data;


	}

	bool IsInFavorites()
	{
		if (Community.CurrentCommunity == null)
			return false;

		return (Community.CurrentCommunity.Name == "Favorites");
	}

	public void Initialize(Texture2D tex)
	{
		if (this == null)
			return;

		ImageRenderer.material.mainTexture = tex;


		if (IsCommunityItem == false || IsInFavorites())
			ImageRenderer.materials[1].mainTexture = ImageRenderer.material.mainTexture;
		
		float aspectRatio = (float)tex.width / (float)tex.height;

		Vector3 scale = transform.localScale;
		
		_cubeScale = scale;
		
		scale.x *= aspectRatio;
		
		if (scale.x > MaxWidthScale)
		{
			scale *= MaxWidthScale / scale.x;
		}

		transform.localScale = scale;
		_bookScale = scale;

	}

	public void SetTexture(string url)
	{
		// if it's a community item then the url is just the path in "resources"
		if (IsCommunityItem)
		{
			Texture2D tex = (Texture2D) Resources.Load(url,typeof(Texture2D));
			Initialize(tex);
		//	ImageRenderer.material.mainTexture = (Texture2D) Resources.Load(url,typeof(Texture2D));
			return;
		}

		var textureCache = WebTextureCache.InstantiateGlobal ();
		if (PlayerPrefs.HasKey("LargeImage" + url))
		{
			Debug.Log("got large url: " + PlayerPrefs.GetString(url));
			_data["LargeUrl"] = PlayerPrefs.GetString(url);
		}

		StartCoroutine( textureCache.GetTexture (url, null, OnGotTexture) );

	}
	
	void OnGotTexture(string url, Dictionary<string,object> data, Texture2D tex)
	{

		Initialize(tex);
		
	}


#region community transitions

	public virtual bool CanGoThrough()
	{
		return CommunityPrefab != null;
	}

	public virtual GameObject DoCommunityForwardTransition(Vector3 finalCameraPos)
	{
		if (SceneManager.Instance.GetScene() == Scene.InTransition)
			return null;

		CubeRotator.SendMessage("DoFastRotateToFront",SendMessageOptions.DontRequireReceiver);
		SceneManager.Instance.SetTransitioning(true);

		GameObject community = (GameObject)Instantiate(CommunityPrefab,new Vector3(finalCameraPos.x, finalCameraPos.y, finalCameraPos.z + CommunityZDepth),Quaternion.identity);

		StartCoroutine(FadeMaterial(.5f,0,() => {

			SceneManager.Instance.SetTransitioning(false);
		}));

		return community;
	}

	public void GoInto()
	{
		if (IsCommunity())
			SelectionManager.Instance.GoIntoSelected();
		else
			StartCoroutine(DoZoomIn());

	}

	public IEnumerator DoZoomIn()
	{
		if (_zoomedIn == false)
		{
			float animTime = .3f;
			LeanTween.move(gameObject,transform.position + Vector3.back * .05f,animTime);
			_zoomedIn = true;

	//		Texture bookCover = ImageRenderer.materials[0].GetTexture("_MainTex");
	//		Texture bookPreview = PageContentMaterial.GetTexture("_MainTex");
	//
	//		_boxMaterials[0].shader = BlendShader;
	//
	//		_boxMaterials[0].SetTexture("_TexMat1",bookCover);
	//		_boxMaterials[0].SetTexture("_TexMat2",bookPreview);
	//
	//
	//		float numFrames = 30;
	//		for (float i=1; i <= numFrames; i++)
	//		{
	//			float percent = i / numFrames;
	//			Debug.Log("percent: " + percent);
	//
	//
	//			_boxMaterials[0].SetFloat("_Blend",percent);
	//
	//			yield return new WaitForSeconds(animTime / numFrames);
	//		}
	//
	//		_boxMaterials[0].SetFloat("_Blend",1);

			yield return new WaitForSeconds(animTime);

			if (PageContentMaterial != null)
			{
				Material[] newMats = _boxMaterials;
				newMats[0] = PageContentMaterial;
				ImageRenderer.materials = newMats;
			}
		}
		yield return null;
	}

	public void DoCommunityBackTransition()
	{
		if (SceneManager.Instance.GetScene() == Scene.InTransition)
			return;
		
	//	SceneManager.Instance.SetTransitioning(true);

		StartCoroutine(FadeMaterial(1,1,() => {
			Debug.Log("stopped transitioning");
//			SceneManager.Instance.SetTransitioning(false);
		}));
		
	}

#endregion

	IEnumerator FadeMaterial(float fadeTime, float alpha, Action onComplete )
	{
//		if (TransparentShader != null)
//			ImageRenderer.material.shader = TransparentShader;

		float cycleTime = .05f;
		float timeLeft = fadeTime;
		float alphaChange = alpha - ImageRenderer.materials[0].color.a;
		float numCycles = fadeTime / cycleTime;
		Color c = Color.white;

	
		while (timeLeft > 0)
		{
			c = ImageRenderer.materials[0].color;
			c.a += alphaChange / numCycles;

			ImageRenderer.materials[0].color = c;
			ImageRenderer.materials[1].color = c;
			SetAlphaOnText(c.a);
			yield return new WaitForSeconds(cycleTime);
			timeLeft -= cycleTime;
		}

		c.a = alpha;
		ImageRenderer.materials[0].color = c;
		ImageRenderer.materials[1].color = c;
		SetAlphaOnText(c.a);

//		if (alpha == 1 && OpaqueShader != null)
//			ImageRenderer.material.shader = OpaqueShader;

		if (onComplete != null)
			onComplete();

	}

	protected virtual void SetAlphaOnText(float alpha)
	{
		// override 
		if (Text == null)
			return;

		Color c = Text.color;
		
		Text.color = new Color(c.r,c.g,c.b,alpha);
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


	protected virtual void Update()
	{
		UpdateRoation();

	}

	public void UpdateFavoritesButton(string url)
	{


		if (ImageRenderer.materials.Length < 5 || FavoritesButtonMesh == null)
		{
			Debug.Log( " mat length: " + ImageRenderer.materials.Length);
			return;
		}

		Debug.Log("setting favorite or url: " + url + " : " + (PlayerPrefs.GetInt("IsFavorite" + url,0) == 1));

		if (IsCommunityItem)
		{
			if (PlayerPrefs.GetInt("IsFavorite" + url,0) == 1)
				ImageRenderer.materials[4].mainTexture =  (Texture2D) Instantiate( Resources.Load("Textures/Action-FavActiveCom", typeof(Texture2D)) );
			else
				ImageRenderer.materials[4].mainTexture =  (Texture2D) Instantiate( Resources.Load("Textures/Com-ActionItems", typeof(Texture2D)) );
		}
		else
		{
			if (PlayerPrefs.GetInt("IsFavorite" + url,0) == 1)
				FavoritesButtonMesh.renderer.material.mainTexture = (Texture2D) Resources.Load("Textures/Actions/action-favorite-active", typeof(Texture2D) );
			else
				FavoritesButtonMesh.renderer.material.mainTexture = (Texture2D) Resources.Load("Textures/Actions/action-favorite", typeof(Texture2D) );

		}
	}

	public string GetResourceUrl()
	{
		return "Textures/" + Community.CurrentCommunity.Name + "/" + ImageRenderer.material.mainTexture.name;
	}
	
	protected virtual void OnSelected()
	{
		ImageRenderer.materials = _boxMaterials;
		if (IsCommunityItem == false || IsInFavorites())
			ImageRenderer.materials[1].mainTexture = WhiteTexture;

		if (_data.ContainsKey("Url"))
			UpdateFavoritesButton(GetData<string>("Url"));
		else
		{

			UpdateFavoritesButton(GetResourceUrl());
		}

		if (Text != null)
			Text.gameObject.SetActive(false);

		float animTime = .3f;
		Vector3 newPos = Camera.main.ViewportToWorldPoint(new Vector3(.5f,.5f,1.8f));
		LeanTween.move(gameObject,newPos ,animTime).setEase(LeanTweenType.easeOutQuad);
		LeanTween.rotateLocal(gameObject,Vector3.zero, animTime).setEase(LeanTweenType.easeOutQuad).setOnComplete ( () =>
		                                                                                                               {
			SceneManager.Instance.PushScene(Scene.Selected);
		
			if (this is PastSearchObj)
				SelectionManager.Instance.GoIntoSelected();
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

		if (IsCommunityItem == false && HasData())
			GameObject.Find("DetailedDescription").SendMessage("SetData",this);
	}

	void OnUnselected()
	{	

		_boxMaterials[0] = _planeMaterials[0];
		ImageRenderer.materials = _planeMaterials;
		Debug.Log("aspect : " + _bookScale + " _cube: " + _cubeScale);
		ScaleToBook(.3f);
		if (SceneManager.Instance.GetTransitioningToScene() == Scene.Helix)
			if (Text != null)
				Text.gameObject.SetActive(true);

		CubeRotator.SendMessage("OnUnselect",SendMessageOptions.DontRequireReceiver);
		CubeRotator.SetActive(false);
		ScaleToPlane(.3f);
		if (IsCommunityItem == false || IsInFavorites())
			ImageRenderer.materials[1].mainTexture = ImageRenderer.material.mainTexture;

		_zoomedIn = false;

	}
	
	void OnGotLargeTexture(string url, Dictionary<string,object> data , Texture2D largeTex )
	{
		if (!_zoomedIn && largeTex != null)
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
		StartCoroutine(FadeMaterial(time,0,null));
		
	}
	
	public void FadeIn(float time)
	{
		StartCoroutine(FadeMaterial(time,1,null));
		
	}
	
	public void SetAlpha(float alpha)
	{
//		if (alpha < 1 && TransparentShader != null)
//			ImageRenderer.material.shader = TransparentShader;
		Color c = ImageRenderer.material.color;
		c.a = alpha;
		ImageRenderer.material.color = c;
		
	}

	public virtual void ScaleToCube()
	{
		Debug.Log("scaling to cube");
		LeanTween.scaleX(_imageBox.gameObject,CubeXScale,.3f);
	}
	
	public void ScaleToPlane(float time)
	{
		LeanTween.scaleX(_imageBox.gameObject,_startXScale,time);
		
	}
	
	public virtual void ScaleToBox(float animTime)
	{
	
		Debug.Log("scaling to box");

		LeanTween.scale(gameObject,_cubeScale,.1f);
	}
	
	public virtual void ScaleToBook(float animTime)
	{

		Debug.Log("scaling to book");

		LeanTween.scale(gameObject,_bookScale,.1f);
		
	}

	public void SavePlace()
	{
		_savedPos = transform.localPosition;
		_savedRotation = transform.localRotation.eulerAngles;
	}
	
	public void HideText()
	{
		if (Text != null)
			Text.gameObject.SetActive(false);
	}
	
	public void SetText(string key)
	{
		if (Text == null)
			return;
		//		Debug.Log("setting key: " + key);
		
		Text.gameObject.SetActive(true);

		Text.GetComponent<SortText>().SetText(key,_data);
	


//		if (key == "Price")
//		{
//			decimal d = new decimal((float)_data[key]);
//			string text = "" +  decimal.Round(d,2).ToString ();
//			Text.text =  text;
//			Debug.Log("setting price: " + text);
//		}
//		else if (key == "Popularity")
//		{
//			//			float val = (float)_data[key];
//			//			string text = ((int)(1.0f/val)).ToString();
//			Text.text = "";
//		}
//		else if (key == "ExpertRating" || key == "BuyerRating")
//		{
//			float val = (float)_data[key];
//			
//			string stars = "";
//			
//			for (int i=0; i < (int)val; i++)
//				stars = stars + "";
//			
//			float last = val - Mathf.Floor(val);
//			//		Debug.Log("last: " + last);
//			if (last > .33f)
//				stars = stars + "";
//			
//			Text.text = stars;
//		}
//		else if (key == "Availability")
//		{
//			Text.text = "";//((int)(float)_data[key]).ToString();
//		}
	}
	
	public T GetData<T>(string key)
	{

		if (_data.ContainsKey(key) == false)
		{
			Debug.Log("key: " + key + " has null value");
		}
		return (T)_data[key];
	}

	public bool HasData(string key)
	{
		return (_data != null && _data.ContainsKey(key));
	}
	public bool HasData()
	{
		return _data != null && _data.Keys.Count > 1;
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
