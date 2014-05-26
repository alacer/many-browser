using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour {

	public GameObject imageObjPrefab;
	public float xPadding = 2;
	public float yPadding = 2;
	public float zPadding = 5;
	public float xSize = 10;
	public float ySize = 10;
	public float zSize = 10;
	public float RandomMoveMaxTime = 1;
	public float RandomMoveMinTime = .5f;
	public float MaxRandomZMovement = 3;


	public float MaxZ;
	public float MinZ;
	
	float _xRadius;
	float _yRadius;
	float _zRadius;
	float _maxHelixY;
	float _minHelixY;
	List<List<Transform>> _xLayers = new List<List<Transform>>();
	List<List<Transform>> _yLayers = new List<List<Transform>>();
	List<List<Transform>> _zLayers = new List<List<Transform>>();

	List<Transform> _allObjs = new List<Transform>();


	List<string> _urls = new List<string>();

	public static GridManager Instance;

	void Awake()
	{
		LeanTween.init(2000);
		Application.targetFrameRate = 60;

		Instance = this;
		_xRadius = (xSize*xPadding)/2.0f;
		_yRadius = (ySize*yPadding)/2.0f;
		_zRadius = (zSize*zPadding)/2.0f;
	}

	public static void Initialize(List<string> urls)
	{
		if (Instance != null)
			DestroyImmediate(Instance.gameObject);


		Instance =  ((GameObject)Instantiate(Resources.Load("GridManager"))).GetComponent<GridManager>();
		Instance.SetUrls(urls);

	//	SetAllVisible(false);
	}

	public void SetUrls(List<string> urls)
	{
		_urls = urls;
		CreateGrid();
		ImageManager.Instance.Initialize(urls);
	}

	public float GetHelixMaxY()
	{
		return _maxHelixY;
	}
	
	public float GetHelixMinY()
	{
		return _minHelixY;
	}


	public void FormHelix()
	{
		Debug.Log("forming helix");
		StartCoroutine(FormHelixRoutine());
	}


	IEnumerator FormHelixRoutine()
	{
		SceneManager.Instance.SetScene(Scene.Helix);


		List<Vector3> positions = new List<Vector3>();
		List<Vector3> rotations = new List<Vector3>();

//		List<ImageObj> _visibleObjs = ImageObj.GetVisibleObjs();

//		for (int i=0; i < _allObjs.Count; i++

//		Debug.Log("visible obj count: " + _visibleObjs.Count);

		Transform helixObj = Camera.main.transform.FindChild("HelixBottom");
		Vector3 center = helixObj.position;
		Vector3 dir = Vector3.right;
		float radius = 2.5f;
		float angleDelta = 35;
		float angle = 0;
		float heightDelta = .2f;
		float animTime = 2;
		float startHeight = 10;
		float animateUpTime = .5f;
		Vector3 rotation = Vector3.zero;

	//	Vector3 pos = center + (radius * dir);

//		for (int i=0; i < _allObjs.Count; i++)
//		{
//
//			LeanTween.move(_allObjs[i].gameObject,center + startHeight*Vector3.up,animateUpTime);
//
//		}
//
//		yield return new WaitForSeconds(animateUpTime);
			
		// Create lists of new positions and rotations
		for (int i=0; i < _allObjs.Count; i++)
		{

//			Debug.Log("rotation: " + rotation);

			center += heightDelta * Vector3.up;
			angle += angleDelta;

			dir = Quaternion.AngleAxis(angle,Vector3.up) * Vector3.right;


			Vector3 pos = center + (radius * dir);
			rotation = Quaternion.LookRotation(dir.normalized).eulerAngles;

			if (i == 0)
				_minHelixY = pos.y;

			if (i == _allObjs.Count-1)
				_maxHelixY = pos.y;

			positions.Add(pos);
			rotations.Add(rotation);
		}

		// now animate all to new positions and rotations
		for (int i=0; i < _allObjs.Count; i++)
		{

	//		yield return new WaitForSeconds(.01f);
			GameObject obj = _allObjs[i].gameObject;
			LeanTween.cancel(obj);
			if (obj.renderer.isVisible)
			{

				LeanTween.move(obj,positions[i],2).setEase(LeanTweenType.easeOutExpo);
				LeanTween.rotate(obj,rotations[i],2).setEase(LeanTweenType.easeOutExpo).setOnComplete(() => 
				{
//					for (int j=0; j < _allObjs.Count; j++)
//					{
//						_allObjs[j].transform.parent = helixObj;
//						
//					}
				});
			}
			else // 
			{
				obj.transform.position = positions[i];
				obj.transform.rotation = Quaternion.Euler(rotations[i]);
			}
		}

//		yield return new WaitForSeconds(2);
//


//		for (int i=0; i < _visibleObjs.Count; i++)
//		{
//			GameObject obj = _visibleObjs[i].gameObject;
//			LeanTween.cancel(obj);
//			LeanTween.move(obj,positions[i],animTime);
//			LeanTween.rotate(obj,rotations[i],animTime);
//			
//			//		yield return new WaitForSeconds(.1f);
//		}


		yield return null;
	}
	

	void Update()
	{
		if (CameraManager.Instance == null)
			return;


		if (SceneManager.Instance.GetScene() == Scene.Browse)
		{
			CheckForNeededShifts();
		}
	}

	Vector3 GetCenterPos()
	{
		return Camera.main.transform.FindChild("GridCenter").position;
	}

	void DoRandomMove()
	{

		ImageObj.DoRandomMove();
	}

	public void SetAllVisible(bool visible)
	{
		for (int i=0; i < transform.childCount; i++)
		{
			transform.GetChild(i).GetComponent<Renderer>().enabled = visible;
		}
	}


#region Shift Functions

	void CheckForNeededShifts()
	{
		Vector3 moveDir = CameraManager.Instance.GetMoveDir();
		
		if (moveDir.magnitude == 0 && LeanTween.isTweening(Camera.main.gameObject) == false)
			return;
		
		Vector3 cameraPos = Camera.main.transform.position;
		Vector3 center = GetCenterPos();// cameraPos + Vector3.forward * _zRadius - Vector3.right * _xRadius/2.0f + Vector3.up * _yRadius/2.0f;
		
		// is the camera moving right and do we need to shift the cube right? use .01 to account for float point error
		if (moveDir.x > .01f &&  (center.x - _xLayers[0][0].position.x) > _xRadius) 
			ShiftRight();
		else if (moveDir.x < -.01f &&  (_xLayers[_xLayers.Count-1][0].position.x - center.x) > _xRadius) // left?
			ShiftLeft();

		// is the camera moving backward and do we need to shift the cube back?
		if ((moveDir.z < -.01f || LeanTween.isTweening(Camera.main.gameObject)) &&  (_zLayers[_zLayers.Count-1][0].position.z - center.z) > _zRadius+1) 
			ShiftBackward();
		else if ((moveDir.z > .01f || LeanTween.isTweening(Camera.main.gameObject)) &&  (center.z - _zLayers[0][0].position.z) > _zRadius) // forward?
			ShiftForward();
		
		// is the camera moving backward and do we need to shift the cube back?
		if ((moveDir.y < -.01f || LeanTween.isTweening(Camera.main.gameObject)) &&  (_yLayers[_yLayers.Count-1][0].position.y - center.y) > _yRadius) 
			ShiftDown();
		else if (moveDir.y > .01f &&  (center.y - _yLayers[0][0].position.y) > _yRadius) // forward?
			ShiftUp();
	}

	void ShiftUp()
	{
	
		List<Transform> newYLayer = new List<Transform>();
		float lastLayerYPos = _yLayers[_yLayers.Count-1][0].position.y;
		float newLayerYPos = lastLayerYPos + yPadding;
		
		for (int i=_yLayers[0].Count-1; i >= 0 ; i--)
		{
			Transform imageObj = _yLayers[0][i];
			imageObj.position = new Vector3(imageObj.position.x,newLayerYPos,imageObj.position.z);
			_yLayers[0].Remove(imageObj);
			newYLayer.Add(imageObj);
		}
		
		_yLayers.RemoveAt(0);
		_yLayers.Add(newYLayer);
	}
	
	
	void ShiftDown()
	{

		List<Transform> newYLayer = new List<Transform>();
		float firstLayerYPos = _yLayers[0][0].position.y;
		float newLayerYPos = firstLayerYPos - yPadding;

		for (int i=_yLayers[_yLayers.Count-1].Count-1; i >= 0 ; i--)
		{
			Transform imageObj = _yLayers[_yLayers.Count-1][i];
			imageObj.position = new Vector3(imageObj.position.x,newLayerYPos,imageObj.position.z);
			_yLayers[_yLayers.Count-1].Remove(imageObj);
			newYLayer.Add(imageObj);
		}
		
		_yLayers.RemoveAt(_yLayers.Count-1);
		_yLayers.Insert(0,newYLayer);
	}

	void ShiftForward()
	{
		List<Transform> newZLayer = new List<Transform>();
		float lastLayerZPos = _zLayers[_zLayers.Count-1][0].position.z;
		float newLayerZPos = lastLayerZPos + zPadding;

		for (int i=_zLayers[0].Count-1; i >= 0 ; i--)
		{
			Transform imageObj = _zLayers[0][i];
			imageObj.position = new Vector3(imageObj.position.x,imageObj.position.y,newLayerZPos);

			_zLayers[0].Remove(imageObj);
			newZLayer.Add(imageObj);
		}
		
		_zLayers.RemoveAt(0);
		_zLayers.Add(newZLayer);
	}


	void ShiftBackward()
	{
		List<Transform> newZLayer = new List<Transform>();
		float firstLayerZPos = _zLayers[0][0].position.z;
		float newLayerZPos = firstLayerZPos - zPadding;
		
		for (int i=_zLayers[_zLayers.Count-1].Count-1; i >= 0 ; i--)
		{
			Transform imageObj = _zLayers[_zLayers.Count-1][i];
			imageObj.position = new Vector3(imageObj.position.x,imageObj.position.y,newLayerZPos);
			_zLayers[_zLayers.Count-1].Remove(imageObj);
			newZLayer.Add(imageObj);
		}
		
		_zLayers.RemoveAt(_zLayers.Count-1);
		_zLayers.Insert(0,newZLayer);
	}

	
	void ShiftLeft()
	{
		List<Transform> newXLayer = new List<Transform>();
		float firstLayerXPos = _xLayers[0][0].position.x;
		float newLayerXPos = firstLayerXPos - xPadding;
		
		for (int i=_xLayers[_xLayers.Count-1].Count-1; i >= 0 ; i--)
		{
			Transform imageObj = _xLayers[_xLayers.Count-1][i];
			imageObj.position = new Vector3(newLayerXPos,imageObj.position.y,imageObj.position.z);
			_xLayers[_xLayers.Count-1].Remove(imageObj);
			newXLayer.Add(imageObj);
		}
		
		_xLayers.RemoveAt(_xLayers.Count-1);
		_xLayers.Insert(0,newXLayer);
	}

	void ShiftRight()
	{
		List<Transform> newXLayer = new List<Transform>();
		float lastLayerXPos = _xLayers[_xLayers.Count-1][0].position.x;
		float newLayerXPos = lastLayerXPos + xPadding;

		for (int i=_xLayers[0].Count-1; i >= 0 ; i--)
		{
			Transform imageObj = _xLayers[0][i];
			imageObj.position = new Vector3(newLayerXPos,imageObj.position.y,imageObj.position.z);
			_xLayers[0].Remove(imageObj);
			newXLayer.Add(imageObj);
		}

		_xLayers.RemoveAt(0);
		_xLayers.Add(newXLayer);
	}



#endregion

	void CreateGrid()
	{
		Vector3 center = GetCenterPos();

		// create the image grid with random images from the urls
		if (_urls.Count > 0)
		{
			int zIndex=0;
			for (float z = -_zRadius; z <= _zRadius; z += zPadding)
			{
				List<Transform> zLayer = new List<Transform>();

				int xIndex=0;
				for (float x = -_xRadius; x <= _xRadius; x += xPadding)
				{
					List<Transform> xLayer = new List<Transform>();

					int yIndex=0;
					for (float y = -_yRadius; y <= _yRadius; y += yPadding)
					{
						float zOffset = Random.Range(MinZ,MaxZ);
						float zPos = z + zOffset;

						GameObject imageObj = (GameObject) Instantiate (imageObjPrefab,new Vector3(x + center.x,y + center.y,zPos + center.z),Quaternion.Euler(0,0,0));
						imageObj.transform.parent = transform;
						
				//		imageObj.SendMessage("Initialize",_urls[Random.Range(0,_urls.Count)]);

						if (xIndex > _xLayers.Count-1)
							_xLayers.Add (new List<Transform>());
						if (yIndex > _yLayers.Count-1)
							_yLayers.Add (new List<Transform>());
						if (zIndex > _zLayers.Count-1)
							_zLayers.Add (new List<Transform>());


						_xLayers[xIndex].Add(imageObj.transform);
						_yLayers[yIndex].Add(imageObj.transform);
						_zLayers[zIndex].Add(imageObj.transform);
						_allObjs.Add(imageObj.transform);

						yIndex++;
					}


					xIndex++;
				}

				zIndex++;
			}

		}
	}

	public float GetZPadding()
	{
		return zPadding;
	}
}
