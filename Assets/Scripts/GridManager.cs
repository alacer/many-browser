using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SortOrder
{
	Asending,
	Desending
}

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
	public float HelixRadius = 3f;

	public float MaxZ;
	public float MinZ;
	
	float _xRadius;
	float _yRadius;
	float _zRadius;
	float _maxHelixY;
	float _minHelixY;

	Quaternion _savedRotation;
	Vector3 _savedPosition;

	List<List<Transform>> _xLayers = new List<List<Transform>>();
	List<List<Transform>> _yLayers = new List<List<Transform>>();
	List<List<Transform>> _zLayers = new List<List<Transform>>();

	List<ImageObj> _allObjs = new List<ImageObj>();
	List<ImageObj> _uniqueObjs = new List<ImageObj>();


	public static GridManager Instance;

	void Awake()
	{

		_savedRotation = transform.rotation;
	
		Application.targetFrameRate = 60;

		Instance = this;
		_xRadius = (xSize*xPadding)/2.0f;
		_yRadius = (ySize*yPadding)/2.0f;
		_zRadius = (zSize*zPadding)/2.0f;

		GameObject.Find("HelixHitCylinder").transform.localScale = new Vector3(HelixRadius*2,5000,HelixRadius*2);
	}

	public static void Initialize()
	{
		if (Instance != null)
		{
			ImageObj[] allObjs = FindObjectsOfType<ImageObj>();
			for (int i = allObjs.Length-1; i >= 0; i--)
				DestroyImmediate(allObjs[i].gameObject);
			DestroyImmediate(Instance.gameObject);
		}

		Instance =  ((GameObject)Instantiate(Resources.Load("GridManager"))).GetComponent<GridManager>();

		Instance.transform.parent = GameObject.Find("UIRoot").transform;
		Instance.CreateGrid();

	//	SetAllVisible(false);
	}

	public float GetHelixMaxY()
	{
		return _maxHelixY;
	}
	
	public float GetHelixMinY()
	{
		return _minHelixY;
	}


	public void SortObjsBy(string sort, SortOrder order)
	{
		Debug.Log("sorting by: " + sort);
		_uniqueObjs = ImageManager.Instance.GetUniqueImageObjList();

		for (int i=0; i < _uniqueObjs.Count; i++)
		{

			for (int j=0; j <_uniqueObjs.Count-1; j++)
			{
				if (order == SortOrder.Desending)
				{
					if (_uniqueObjs[j].GetData<float>(sort) > _uniqueObjs[j+1].GetData<float>(sort))
					{
						SwapObjs(j,j+1);
					}
				}
				else
				{
					if (_uniqueObjs[j].GetData<float>(sort) < _uniqueObjs[j+1].GetData<float>(sort))
					{
						SwapObjs(j,j+1);
					}
				}
				
			}
			
		}

	}
	
	void SwapObjs(int firstIndex, int secondIndex)
	{
		ImageObj temp = _uniqueObjs[secondIndex];
		
		_uniqueObjs[secondIndex] = _uniqueObjs[firstIndex];
		_uniqueObjs[firstIndex] = temp;
		
	}


	public void FormHelix(string sort, SortOrder order)
	{

		SortObjsBy(sort,order);

		Debug.Log("forming helix");
	
		StartCoroutine(FormHelixRoutine(sort));
	}

	
	IEnumerator FormHelixRoutine(string sort)
	{
		while (SceneManager.Instance.GetScene() == Scene.InTransition)
		{

			yield return new WaitForSeconds(.1f);
		}

		if (SelectionManager.Instance.IsSelected())
		{
			float time = SelectionManager.Instance.LeaveSelectedObj();
			yield return new WaitForSeconds(time);
			
		}

		bool savePositions = (SceneManager.Instance.GetScene() == Scene.Browse);
		SceneManager.Instance.OnSceneTransition();
		CameraManager.Instance.SavePlace();

		if (savePositions)
		{
			_savedRotation = transform.rotation;
			_savedPosition = transform.position;
		}

		List<Vector3> positions = new List<Vector3>();
		List<Vector3> rotations = new List<Vector3>();


		Transform helixObj = Camera.main.transform.FindChild("HelixBottom");
		Vector3 center = helixObj.position;
		Vector3 dir = Vector3.right;

		float angleDelta = 25;
		float angle = 0;
		float heightDelta = .15f;
	

		float animateToHelixTime = 1;
		Vector3 rotation = Vector3.zero;


		// Create lists of new positions and rotations
		for (int i=0; i < _uniqueObjs.Count; i++)
		{

			center += heightDelta * Vector3.up;
			angle += angleDelta;

			dir = Quaternion.AngleAxis(angle,Vector3.up) * Vector3.right;


			Vector3 pos = center + (HelixRadius * dir);
			rotation = Quaternion.LookRotation(-dir.normalized).eulerAngles;

			if (i == 0)
				_minHelixY = pos.y;

			if (i == _uniqueObjs.Count-1)
				_maxHelixY = pos.y;

			positions.Add(pos);
			rotations.Add(rotation);
		}

		if (savePositions)
			foreach(ImageObj obj in _allObjs)
			{
				obj.SavePlace();
			}

		// now animate all to new positions and rotations
		for (int i=0; i < _uniqueObjs.Count; i++)
		{

			_uniqueObjs[i].SetText(sort);
			GameObject obj = _uniqueObjs[i].gameObject;
			LeanTween.cancel(obj);

			Vector3 newPos = positions[i];
			Vector3 newRotation = rotations[i];

			Vector3 viewportPos = Camera.main.WorldToViewportPoint(newPos);
			bool newPosIsVisible = (viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1);
			if (obj.GetComponent<ImageObj>().IsVisible() || newPosIsVisible)
			{
				LeanTween.move(obj,newPos,animateToHelixTime).setEase(LeanTweenType.easeOutExpo);
				LeanTween.rotate(obj,newRotation,animateToHelixTime).setEase(LeanTweenType.easeOutExpo);
			}
			else 
			{
				obj.transform.position = positions[i];
				obj.transform.rotation = Quaternion.Euler(rotations[i]);
			}
		}

		// now hide the non unique objects
		foreach(ImageObj obj in _allObjs)
		{


			if (_uniqueObjs.Contains(obj) == false)
				obj.gameObject.SetActive(false);

		}

		yield return new WaitForSeconds (animateToHelixTime);
		SceneManager.Instance.PushScene(Scene.Helix);
		yield return null;
	}

	IEnumerator UnformHelix()
	{
		HelixButton.UnselectAll();
		SceneManager.Instance.OnSceneTransition();
		float animTime = 1;


		transform.rotation = _savedRotation;
		transform.position = _savedPosition;

		CameraManager.Instance.MoveToSavedPlace(animTime);

		foreach (ImageObj obj in _allObjs)
		{
			obj.HideText();

			if (_uniqueObjs.Contains(obj))
			{
				obj.MoveToSavedPlace(animTime);
			}
			else
			{
				Vector3 pos = obj.transform.position;

				Vector3 viewportPos = Camera.main.WorldToViewportPoint(pos);
				bool posIsVisible = (viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1);

				if (posIsVisible)
				{
					obj.transform.position += Random.onUnitSphere * 10;
					LeanTween.move(obj.gameObject,pos,animTime);
				}

				obj.gameObject.SetActive(true);


			}

			
		}


		yield return new WaitForSeconds(animTime);

		SceneManager.Instance.PushScene(Scene.Browse);
	}

	void OnTwoFingerSwipe(Vector3 dir)
	{
				
		if (SceneManager.Instance.GetScene() == Scene.Helix && dir == Vector3.back)
			StartCoroutine(UnformHelix());


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
	

	public void SetAllVisible(bool visible)
	{
		for (int i=0; i < transform.childCount; i++)
		{
			transform.GetChild(i).GetComponent<ImageObj>().SetVisible(visible);
		}
	}


#region Shift Functions

	void CheckForNeededShifts()
	{
		Vector3 moveDir = CameraManager.Instance.GetMoveDir();
		
		if (moveDir.magnitude == 0 && LeanTween.isTweening(Camera.main.gameObject) == false)
			return;
		
	//	Vector3 cameraPos = Camera.main.transform.position;
		Vector3 center = GetCenterPos();// cameraPos + Vector3.forward * _zRadius - Vector3.right * _xRadius/2.0f + Vector3.up * _yRadius/2.0f;
		
		// is the camera moving right and do we need to shift the cube right? use .01 to account for float point error
		if (moveDir.x > .01f &&  (center.x - _xLayers[0][0].position.x) > _xRadius) 
			ShiftRight();
		else if (moveDir.x < -.01f &&  (_xLayers[_xLayers.Count-1][0].position.x - center.x) > _xRadius) // left?
			ShiftLeft();

		// is the camera moving backward and do we need to shift the cube back?
		if (moveDir.z < -.01f &&  (_zLayers[_zLayers.Count-1][0].position.z - center.z) > _zRadius+1) 
			ShiftBackward();
		else if (moveDir.z > .01f &&  (center.z - _zLayers[0][0].position.z) > _zRadius) // forward?
			ShiftForward();
		
		// is the camera moving backward and do we need to shift the cube back?
		if (moveDir.y < -.01f  &&  (_yLayers[_yLayers.Count-1][0].position.y - center.y) > _yRadius) 
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

		int zIndex=0;
		for (float z = -_zRadius; z <= _zRadius; z += zPadding)
		{

			int xIndex=0;
			for (float x = -_xRadius; x <= _xRadius; x += xPadding)
			{


				int yIndex=0;
				for (float y = -_yRadius; y <= _yRadius; y += yPadding)
				{
					float zOffset = Random.Range(MinZ,MaxZ);
					float zPos = z + zOffset;

					GameObject imageObj = (GameObject) Instantiate (imageObjPrefab,new Vector3(x + center.x,y + center.y,zPos + center.z),Quaternion.Euler(0,0,0));
					imageObj.transform.parent = transform;


					if (xIndex > _xLayers.Count-1)
						_xLayers.Add (new List<Transform>());
					if (yIndex > _yLayers.Count-1)
						_yLayers.Add (new List<Transform>());
					if (zIndex > _zLayers.Count-1)
						_zLayers.Add (new List<Transform>());


					_xLayers[xIndex].Add(imageObj.transform);
					_yLayers[yIndex].Add(imageObj.transform);
					_zLayers[zIndex].Add(imageObj.transform);
					_allObjs.Add(imageObj.GetComponent<ImageObj>());

					yIndex++;
				}


				xIndex++;
			}

			zIndex++;
		}


	}

	public float GetZPadding()
	{
		return zPadding;
	}
}
