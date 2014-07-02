using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SortOrder
{
	Asending,
	Desending
}

public class HelixManager : Community {

	public GameObject imageObjPrefab;

	public float HelixRadius = 3f;

	public float MaxZ;
	public float MinZ;

	float _maxHelixY;
	float _minHelixY;


	List<ImageObj> _allObjs = new List<ImageObj>();
	
	public static HelixManager Instance;

	void Awake()
	{
		Instance = this;

		GameObject.Find("HelixHitCylinder").transform.localScale = new Vector3(HelixRadius*2,5000,HelixRadius*2);

		transform.parent = GameObject.Find("UIRoot").transform;
		

		
		FormHelix("Price",SortOrder.Desending);
	}

	
	public static void Initialize(List< Dictionary<string,object> > objDataList)
	{
		// destroy current images
//		if (Instance != null)
//		{
//			ImageObj[] allObjs = FindObjectsOfType<ImageObj>();
//			for (int i = allObjs.Length-1; i >= 0; i--)
//				DestroyImmediate(allObjs[i].gameObject);
//			DestroyImmediate(Instance.gameObject);
//		}



		Instance =  ((GameObject)Instantiate(Resources.Load("HelixManager"))).GetComponent<HelixManager>();
		Instance.CreateImageObjs(objDataList);

	//	SetAllVisible(false);
	}

	void CreateImageObjs(List< Dictionary<string,object> > objDataList)
	{
		foreach (Dictionary<string,object> data in objDataList)
		{
			GameObject imageObj = (GameObject) Instantiate (imageObjPrefab);
			imageObj.transform.parent = Instance.transform;
			imageObj.SendMessage("InitData",data);
			_allObjs.Add(imageObj.GetComponent<ImageObj>());
		}

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

		SceneManager.Instance.OnSceneTransition(Scene.Helix);
		CameraManager.Instance.SavePlace();

		
		List<Vector3> positions = new List<Vector3>();
		List<Vector3> rotations = new List<Vector3>();
		
		
		Transform helixObj = Camera.main.transform.FindChild("HelixBottom");
		helixObj.position = CameraManager.Instance.GetForwardTransitionTargetPos() + Vector3.forward * 5;

		Vector3 center = helixObj.position;
		Vector3 dir = Vector3.right;
		
		float angleDelta = 25;
		float angle = 0;
		float heightDelta = .15f;
		
		
		float animateToHelixTime = 1;
		Vector3 rotation = Vector3.zero;
		
		
		// Create lists of new positions and rotations
		for (int i=0; i < _allObjs.Count; i++)
		{
			
			center += heightDelta * Vector3.down;
			angle += angleDelta;
			
			dir = Quaternion.AngleAxis(angle,Vector3.up) * Vector3.right;
			
			
			Vector3 pos = center + (HelixRadius * dir);
			rotation = Quaternion.LookRotation(-dir.normalized).eulerAngles;
			
			if (i == 0)
				_maxHelixY = pos.y + 1;
			
			if (i == _allObjs.Count-1)
				_minHelixY = pos.y;
			
			positions.Add(pos);
			rotations.Add(rotation);
		}

		
		// now animate all to new positions and rotations
		for (int i=0; i < _allObjs.Count; i++)
		{
			
			_allObjs[i].SetText(sort);
			GameObject obj = _allObjs[i].gameObject;
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

		
		yield return new WaitForSeconds (animateToHelixTime);
		SceneManager.Instance.PushScene(Scene.Helix);
		yield return null;
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

		for (int i=0; i < _allObjs.Count; i++)
		{

			for (int j=0; j <_allObjs.Count-1; j++)
			{
				if (order == SortOrder.Desending)
				{
					if (_allObjs[j].GetData<float>(sort) > _allObjs[j+1].GetData<float>(sort))
					{
						SwapObjs(j,j+1);
					}
				}
				else
				{
					if (_allObjs[j].GetData<float>(sort) < _allObjs[j+1].GetData<float>(sort))
					{
						SwapObjs(j,j+1);
					}
				}
				
			}
			
		}

	}
	
	void SwapObjs(int firstIndex, int secondIndex)
	{
		ImageObj temp = _allObjs[secondIndex];
		
		_allObjs[secondIndex] = _allObjs[firstIndex];
		_allObjs[firstIndex] = temp;
		
	}

	public List<ImageObj> GetAllObjs()
	{
		return _allObjs;
	}




//	IEnumerator UnformHelix()
//	{
//		HelixButton.UnselectAll();
//		SceneManager.Instance.OnSceneTransition(Scene.Browse);
//		float animTime = 1;
//
//
//		transform.rotation = _savedRotation;
//		transform.position = _savedPosition;
//
//		CameraManager.Instance.MoveToSavedPlace(animTime);
//
//		foreach (ImageObj obj in _allObjs)
//		{
//			obj.HideText();
//
//			if (_uniqueObjs.Contains(obj))
//			{
//				obj.MoveToSavedPlace(animTime);
//			}
//			else
//			{
//				Vector3 pos = obj.transform.position;
//
//				Vector3 viewportPos = Camera.main.WorldToViewportPoint(pos);
//				bool posIsVisible = (viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1);
//
//				if (posIsVisible)
//				{
//					obj.transform.position += Random.onUnitSphere * 10;
//					LeanTween.move(obj.gameObject,pos,animTime);
//				}
//
//				obj.gameObject.SetActive(true);
//
//
//			}
//
//			
//		}
//
//
//		yield return new WaitForSeconds(animTime);
//
//		SceneManager.Instance.PushScene(Scene.Browse);
//	}

	void OnTwoFingerSwipe(Vector3 dir)
	{
				
//		if (SceneManager.Instance.GetScene() == Scene.Helix && dir == Vector3.back)
//			StartCoroutine(UnformHelix());


	}
	

	void Update()
	{
		if (CameraManager.Instance == null)
			return;


	}
	

	public void SetAllVisible(bool visible)
	{
		for (int i=0; i < transform.childCount; i++)
		{
			transform.GetChild(i).GetComponent<ImageObj>().SetVisible(visible);
		}
	}
	
//	void CreateGrid()
//	{
//
//		Vector3 center = GetCenterPos();
//
//		int zIndex=0;
//		for (float z = -_zRadius; z <= _zRadius; z += zPadding)
//		{
//
//			int xIndex=0;
//			for (float x = -_xRadius; x <= _xRadius; x += xPadding)
//			{
//
//
//				int yIndex=0;
//				for (float y = -_yRadius; y <= _yRadius; y += yPadding)
//				{
//					float zOffset = Random.Range(MinZ,MaxZ);
//					float zPos = z + zOffset;
//
//					x += Random.Range(-RandomOffsetRadX,RandomOffsetRadX);
//
//					GameObject imageObj = (GameObject) Instantiate (imageObjPrefab,new Vector3(x + center.x,y + center.y,zPos + center.z),Quaternion.Euler(0,0,0));
//					imageObj.transform.parent = transform;
//
//
//					if (xIndex > _xLayers.Count-1)
//						_xLayers.Add (new List<Transform>());
//					if (yIndex > _yLayers.Count-1)
//						_yLayers.Add (new List<Transform>());
//					if (zIndex > _zLayers.Count-1)
//						_zLayers.Add (new List<Transform>());
//
//
//					_xLayers[xIndex].Add(imageObj.transform);
//					_yLayers[yIndex].Add(imageObj.transform);
//					_zLayers[zIndex].Add(imageObj.transform);
//					_allObjs.Add(imageObj.GetComponent<ImageObj>());
//
//					yIndex++;
//				}
//
//
//				xIndex++;
//			}
//
//			zIndex++;
//		}
//
//
//	}
//
//	public float GetZPadding()
//	{
//		return zPadding;
//	}
}
