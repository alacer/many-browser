using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class ImageObj : MonoBehaviour {

	public float MoveSpeed = 5;
	public float RotateDistance = 5;

	static float MaxZOffset = 3;
	bool _isFacingCamera;

	string _url;
	bool _loaded;
	
	Vector3 _totalZMovement;
	Vector3 _moveDelta;

	static Dictionary<string,Texture2D>  _imageCache = new Dictionary<string, Texture2D>();
	static List<ImageObj> _visibleObjs = new List<ImageObj>();
	

	void Start()
	{
		_totalZMovement = transform.forward * Random.Range(-MaxZOffset,MaxZOffset);
		transform.position += _totalZMovement;
	}

	void OnBecameVisible()
	{
		if (!_loaded)
			ImageManager.Instance.AddObjToLoad(this);



		_visibleObjs.Add(this);
	}

	void OnBecameInvisible()
	{
		if (!_loaded)
			ImageManager.Instance.AddObjToQueue(this);

		if (_visibleObjs.Contains(this))
			_visibleObjs.Remove(this);
	}

	void UpdateRoation()
	{
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

	void UpdateRandomMovement()
	{
		float magnitude = _moveDelta.magnitude;
		if (magnitude > 0)
		{
			Vector3 delta =  _moveDelta.normalized * Time.deltaTime * MoveSpeed;
			_totalZMovement += delta;
			
			// don't move any more if the image has reached it's max z limit
			if (_totalZMovement.magnitude >= MaxZOffset)
			{
				_moveDelta = Vector3.zero;
				return;
				
			}
			
			transform.position += delta;
			
			magnitude -= delta.magnitude;
			
			if (magnitude < 0)
				magnitude = 0;
			
			_moveDelta = _moveDelta.normalized * magnitude;
			
		}
	}



	void Update()
	{

		UpdateRandomMovement();

		UpdateRoation();
	}

	public void SetMoveDelta(Vector3 moveDelta)
	{
		_moveDelta = moveDelta;
	}

	public static void DoRandomMove()
	{
		if (_visibleObjs.Count == 0)
			return;

		ImageObj obj = _visibleObjs[Random.Range(0,_visibleObjs.Count)];

		obj.SetMoveDelta(Random.Range(-MaxZOffset,MaxZOffset) * obj.transform.forward);



	}

	public IEnumerator LoadTexture(string url)
	{

		while (ImageManager.Instance.GetFPS() < 40)
		{
			// don't load if the framerate is too low
			yield return new WaitForSeconds(Random.Range(.1f,.3f));
		}

		// do we already have this image downloaded? if so Just set it. 
		if (_imageCache.ContainsKey(url))
		{
			SetTexture(_imageCache[url]);

		}
		else // otherwise, download it and set it
		{
			WWW imageWWW = new WWW (url);

		

			yield return imageWWW;
			
			if (string.IsNullOrEmpty(imageWWW.error))
			{
				_imageCache[url] = imageWWW.texture;
				SetTexture(imageWWW.texture);
				
			}
		}

		_loaded = true;

	}

	void SetTexture(Texture2D tex)
	{
		renderer.material.mainTexture = tex;
		
		float aspectRatio = tex.width / tex.height;
		Vector3 scale = transform.localScale;
		scale.x *= aspectRatio;
		transform.localScale = scale;
		
		LeanTween.rotateX(gameObject,0,1).setEase(LeanTweenType.easeOutElastic);

	}

	
}
