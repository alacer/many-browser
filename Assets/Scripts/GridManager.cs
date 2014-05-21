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

	float _timeUntilRandomMove;
	float _xRadius;
	float _yRadius;
	float _zRadius;
	List<List<Transform>> _xLayers = new List<List<Transform>>();
	List<List<Transform>> _yLayers = new List<List<Transform>>();
	List<List<Transform>> _zLayers = new List<List<Transform>>();

	List<Transform> _allObjs = new List<Transform>();

	List<string> _urls = new List<string>();

	public static GridManager Instance;

	void Awake()
	{
		Application.targetFrameRate = 60;
		_timeUntilRandomMove = Random.Range(RandomMoveMinTime,RandomMoveMaxTime);

		Instance = this;
		_xRadius = (xSize*xPadding)/2.0f;
		_yRadius = (ySize*yPadding)/2.0f;
		_zRadius = (zSize*zPadding)/2.0f;
	}

	public void Initialize(List<string> urls)
	{
		_urls = urls;
		CreateGrid();
		ImageManager.Instance.Initialize(urls);
	}
	

	void Update()
	{
		if (InputManager.Instance == null)
			return;


		CheckForNeededShifts();

		_timeUntilRandomMove -= Time.deltaTime;

		if (_timeUntilRandomMove <= 0)
		{
			DoRandomMove();
			_timeUntilRandomMove = Random.Range(RandomMoveMinTime,RandomMoveMaxTime);
		}
	}

	Vector3 GetCenterPos()
	{
		return Camera.main.transform.GetChild(0).position;
	}

	void DoRandomMove()
	{

		ImageObj.DoRandomMove();

	}


#region Shift Functions

	void CheckForNeededShifts()
	{
		Vector3 moveDir = InputManager.Instance.GetMoveDir();
		
		if (moveDir.magnitude == 0)
			return;
		
		Vector3 cameraPos = Camera.main.transform.position;
		Vector3 center = GetCenterPos();// cameraPos + Vector3.forward * _zRadius - Vector3.right * _xRadius/2.0f + Vector3.up * _yRadius/2.0f;
		
		// is the camera moving right and do we need to shift the cube right? use .01 to account for float point error
		if (moveDir.x > .01f &&  (center.x - _xLayers[0][0].position.x) > _xRadius) 
			ShiftRight();
		else if (moveDir.x < -.01f &&  (_xLayers[_xLayers.Count-1][0].position.x - center.x) > _xRadius) // left?
			ShiftLeft();

		// is the camera moving backward and do we need to shift the cube back?
		if (moveDir.z < -.01f &&  (_zLayers[_zLayers.Count-1][0].position.z - center.z) > _zRadius) 
			ShiftBackward();
		else if (moveDir.z > .01f &&  (center.z - _zLayers[0][0].position.z) > _zRadius) // forward?
			ShiftForward();
		
		// is the camera moving backward and do we need to shift the cube back?
		if (moveDir.y < -.01f &&  (_yLayers[_yLayers.Count-1][0].position.y - center.y) > _yRadius) 
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

						GameObject imageObj = (GameObject) Instantiate (imageObjPrefab,new Vector3(x + center.x,y + center.y,zPos + center.z),Quaternion.Euler(90,0,0));
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
}
