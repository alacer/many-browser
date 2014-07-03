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
	public float MaxAngularSpeed = 2;
	public float HelixRadius = 3f;
	public float SpeedMultiplier = 2;
	public float MaxZ;
	public float MinZ;
	public float Friction = 0.003f;

	float _maxHelixY;
	float _minHelixY;

	Vector3 _velocity;
	Vector3 _topObjPos;

	List<ImageObj> _allObjs = new List<ImageObj>();

	
	public static HelixManager Instance;

	void Awake()
	{
		Instance = this;

		GameObject.Find("HelixHitCylinder").transform.localScale = new Vector3(HelixRadius*2,5000,HelixRadius*2);

		transform.parent = GameObject.Find("UIRoot").transform;

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
		Instance.FormHelix("Price",SortOrder.Desending);

	//	SetAllVisible(false);
	}

	void CreateImageObjs(List< Dictionary<string,object> > objDataList)
	{
		Debug.Log("creating images: " + objDataList.Count);
		foreach (Dictionary<string,object> data in objDataList)
		{
			GameObject imageObj = (GameObject) Instantiate (imageObjPrefab);
			imageObj.transform.parent = Instance.transform;
			imageObj.SendMessage("InitData",data);
			_allObjs.Add(imageObj.GetComponent<ImageObj>());
		}

	}

	void FixedUpdate()
	{
		 if (SceneManager.Instance.GetScene() != Scene.Helix)
			return;

		if (InputManager.Instance.IsTouchingWithOneFinger())
		{
			if (InputManager.Instance.HasFingerStoppedMoving())
				_velocity = Vector3.zero;
			else
				_velocity = GetHelixVelocity();
		}

		// apply velocity and friction
		float magnitude = LimitVelocity();
		
		
		if (magnitude > 0)
		{

			transform.RotateAround(transform.position,Vector3.up,_velocity.x);
			transform.position += new Vector3(0,_velocity.y,0);

			// clamp bounds
			if (transform.position.y > GetHelixMaxY())
				transform.position = new Vector3(transform.position.x, GetHelixMaxY(), transform.position.z);
			else if (transform.position.y < GetHelixMinY())
				transform.position = new Vector3(transform.position.x, GetHelixMinY(), transform.position.z);
		
		}

		ApplyFriction(magnitude);
	}

	Vector3 GetHelixVelocity()
	{
		Vector3 vel = Vector3.zero;
		if (InputManager.Instance.GetTouchWorldPos() != Vector3.zero)
		{
			Vector3 lastPos = InputManager.Instance.GetLastTouchWorldPos() - transform.position;
			Vector3 currentPos = InputManager.Instance.GetTouchWorldPos() - transform.position;
			lastPos.y = 0;
			currentPos.y = 0;
			float dir = (Vector3.Cross(lastPos,currentPos).y > 0) ? 1 : -1;
			float angleDelta = Vector3.Angle(lastPos,currentPos);

			angleDelta = Mathf.Min(angleDelta,MaxAngularSpeed);
			vel = new Vector3( angleDelta * dir , -InputManager.Instance.GetOneFingerDelta().y, 0) * SpeedMultiplier;

		}

		return vel;
	}

	void ApplyFriction(float magnitude)
	{
		// apply friction
		magnitude -= Friction ;
		magnitude = Mathf.Max(0,magnitude);
		_velocity = _velocity.normalized * magnitude;
	}


	float LimitVelocity()
	{		
		float magnitude =_velocity.magnitude;
		float maxSpeed = MaxAngularSpeed;
		
		
		if (magnitude > maxSpeed)
		{
			_velocity = _velocity.normalized * maxSpeed;
			magnitude = maxSpeed;
		}
		
		return magnitude;
		
	}


	public void FormHelix(string sort, SortOrder order)
	{
		
		SortObjsBy(sort,order);
		
		Debug.Log("forming helix");
		
		StartCoroutine(FormHelixRoutine(sort));
	}
	
	
	IEnumerator FormHelixRoutine(string sort)
	{
		if (SceneManager.Instance.GetScene() != Scene.Helix)
			transform.position = CameraManager.Instance.GetForwardTransitionTargetPos() + Vector3.forward * 5;

		Debug.Log("in form helix");
		while (SceneManager.Instance.GetScene() == Scene.InTransition)
		{
			yield return new WaitForSeconds(.1f);
		}
		
		if (SelectionManager.Instance.IsSelected())
		{
			float time = SelectionManager.Instance.LeaveSelectedObj();
			yield return new WaitForSeconds(time);
			
		}

		Debug.Log("all objs: " + _allObjs.Count);
		SceneManager.Instance.OnSceneTransition(Scene.Helix);

//		List<Vector3> positions = new List<Vector3>();
//		List<Vector3> rotations = new List<Vector3>();



		Vector3 center = transform.position;
		Vector3 dir = Vector3.right;
		
		float angleDelta = 25;
		float angle = 0;
		float heightDelta = .15f;

		float animateToHelixTime = 1;
		Vector3 rotation = Vector3.zero;

		List<Vector3> positions = new List<Vector3>();
		List<Vector3> rotations = new List<Vector3>();

//		_minHelixY = transform.position.z;

		// Create lists of new positions and rotations
		for (int i=0; i < _allObjs.Count; i++)
		{
			dir = Quaternion.AngleAxis(angle,Vector3.up) * Vector3.back;

			center += heightDelta * Vector3.down;
			angle += angleDelta;

			Vector3 pos = center + (HelixRadius * dir);
			rotation = Quaternion.LookRotation(-dir.normalized).eulerAngles;
			
			if (i == _allObjs.Count-1)
			{
				_maxHelixY = _minHelixY + (_minHelixY - pos.y);

			}
			
			if (i == 0)
			{
				_topObjPos = pos;

				_minHelixY = pos.y;

			}

			positions.Add(pos);
			rotations.Add(pos);

			_allObjs[i].transform.position = pos;
			_allObjs[i].transform.rotation = Quaternion.Euler(rotation);

			_allObjs[i].SetText(sort);


//			positions.Add(pos);
//			rotations.Add(rotation);
		}

		float moveUpDist = 20;
		transform.position += Vector3.down * moveUpDist;

		LeanTween.move(gameObject,transform.position + Vector3.up * moveUpDist,2);
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

	public Vector3 GetTopObjPos()
	{
		return _topObjPos;
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

}
