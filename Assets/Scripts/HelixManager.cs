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

	Vector3 _targetPos;
	Quaternion _targetRotation;

	List<ImageObj> _allObjs = new List<ImageObj>();

	
	public static HelixManager Instance;

	void Awake()
	{
		Instance = this;

		GameObject.Find("HelixHitCylinder").transform.localScale = new Vector3(HelixRadius*2,5000,HelixRadius*2);

		transform.parent = GameObject.Find("UIRoot").transform;


	}

	void OnDestroy()
	{
		Instance = null;
	}

	
	public static void Initialize(List< Dictionary<string,object> > objDataList)
	{
		// destroy current images
		if (Instance != null)
		{
			Instance.Clear();
	//		DestroyImmediate(Instance.gameObject);
		}
		else 
			Instance = ((GameObject)Instantiate(Resources.Load("HelixManager"))).GetComponent<HelixManager>();

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

	public void Clear()
	{
		_allObjs.Clear();
		ImageObj[] allObjs = transform.GetComponentsInChildren<ImageObj>();
		for (int i = allObjs.Length-1; i >= 0; i--)
			DestroyImmediate(allObjs[i].gameObject);
	}

	public void FormHelix(string sort, SortOrder order)
	{
		SortObjsBy(sort,order);
		
		Debug.Log("forming helix");
		
		StartCoroutine(FormHelixRoutine(sort));
	}
	
	
	IEnumerator FormHelixRoutine(string sort)
	{

		
		Debug.Log("in form helix");
		while (SceneManager.Instance.GetScene() == Scene.InTransition)
		{
			yield return new WaitForSeconds(.1f);
		}

		bool wasAlreadyInHelix = SceneManager.Instance.GetScene() == Scene.Helix;
		
		if (!wasAlreadyInHelix)
			transform.position = CameraManager.Instance.GetForwardTransitionTargetPos(10) + Vector3.forward * 5;
		
		_targetPos = transform.position;
		_targetRotation = transform.rotation;



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
		
		// Create lists of new positions and rotations
		for (int i=0; i < _allObjs.Count; i++)
		{
			dir = Quaternion.AngleAxis(angle,Vector3.up) * Vector3.back;
			
			center += heightDelta * Vector3.down;
			angle += angleDelta;
			
			Vector3 pos = center + (HelixRadius * dir);
			rotation = Quaternion.LookRotation(-dir.normalized).eulerAngles;
			
			
			if (!wasAlreadyInHelix)
			{
				if (i == _allObjs.Count-1)
				{
					_maxHelixY = _minHelixY + (_minHelixY - pos.y);
					
				}
				
				if (i == 0)
				{
					_topObjPos = pos;
					
					_minHelixY = pos.y;
					
				}
			}
			
			if (wasAlreadyInHelix)
			{
				positions.Add(pos);
				rotations.Add(rotation);
				Community.CurrentCommunity.Name = ImageSearch.Instance.GetSearch();
				Utils.SendMessageToAll("OnCommunityChange");
			}
			else
			{
				_allObjs[i].transform.position = pos;
				_allObjs[i].transform.rotation = Quaternion.Euler(rotation);
			}
			
			_allObjs[i].SetText(sort);
			
			
			//			positions.Add(pos);
			//			rotations.Add(rotation);
		}
		
		if (!wasAlreadyInHelix)
		{
			float moveUpDist = 20;
			transform.position += Vector3.down * moveUpDist;
			
			LeanTween.move(gameObject,transform.position + Vector3.up * moveUpDist,2);
		}
		// now animate all to new positions and rotations
		
		for (int i=0; i < positions.Count; i++)
		{
			
			GameObject obj = _allObjs[i].gameObject;
			LeanTween.cancel(obj);
			
			Vector3 newPos = positions[i];
			Vector3 newRotation = rotations[i];

			LeanTween.move(obj,newPos,animateToHelixTime).setEase(LeanTweenType.easeOutExpo);
			LeanTween.rotate(obj,newRotation,animateToHelixTime).setEase(LeanTweenType.easeOutExpo);
			

		}

		yield return new WaitForSeconds (animateToHelixTime);
		SceneManager.Instance.PushScene(Scene.Helix);


		yield return null;

	}

	void SetZoomOutPos()
	{
		Debug.Log (" pos: " + transform.position.y + " max: " + _maxHelixY);

		float midYPos = transform.position.y - (_maxHelixY / 2.0f);
		Community.CurrentCommunity.ZoomedOutCameraPos.y = midYPos;
	}

	void FixedUpdate()
	{
		 if (SceneManager.Instance.GetScene() != Scene.Helix)
			return;

		if (InputManager.Instance.IsTouchingWithOneFinger())
		{
			_velocity = GetHelixVelocity();
		}

		// apply velocity and friction
		float magnitude = LimitVelocity();
		
		
		if (magnitude > 0)
		{
			Debug.Log("vel: " + _velocity);


//			_targetPos += new Vector3(0,_velocity.y,0);
//			_targetRotation = _targetRotation * Quaternion.AngleAxis(_velocity.x,Vector3.up);

			Debug.Log("target pos: " + _targetPos);
			transform.position += new Vector3(0,_velocity.y,0);
			if (_velocity.y != 0)
				SetZoomOutPos();

			transform.RotateAround(transform.position,Vector3.up,_velocity.x);


			// clamp bounds
			if (transform.position.y > GetHelixMaxY())
				transform.position = new Vector3(transform.position.x, GetHelixMaxY(), transform.position.z);
			else if (transform.position.y < GetHelixMinY())
				transform.position = new Vector3(transform.position.x, GetHelixMinY(), transform.position.z);
		
		}

	//	transform.rotation = Quaternion.Lerp(transform.rotation,_targetRotation,Time.deltaTime*SpeedMultiplier);
	//	transform.position = Vector3.Lerp(transform.position,_targetPos,Time.deltaTime * SpeedMultiplier);

		ApplyFriction(magnitude);
	}

	Vector3 GetHelixVelocity()
	{

		Vector3 vel = Vector3.zero;
		if (InputManager.Instance.IsFingerMoving() && InputManager.Instance.GetTouchWorldPos() != Vector3.zero && InputManager.Instance.GetLastTouchWorldPos() != Vector3.zero)
		{
			Vector3 worldPos = InputManager.Instance.GetTouchWorldPos();
			Vector3 lastWorldPos = InputManager.Instance.GetLastTouchWorldPos();
			float yVel = lastWorldPos.y - worldPos.y;

			Vector3 lastPos = lastWorldPos - transform.position;
			Vector3 currentPos = worldPos - transform.position;
			lastPos.y = 0;
			currentPos.y = 0;
			float dir = (Vector3.Cross(lastPos,currentPos).y > 0) ? 1 : -1;
			float angleDelta = Vector3.Angle(lastPos,currentPos);

			angleDelta = Mathf.Min(angleDelta,MaxAngularSpeed);
			vel = new Vector3( angleDelta * dir , -yVel, 0);

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
	
	public void SetAllVisible(bool visible)
	{
		for (int i=0; i < transform.childCount; i++)
		{
			transform.GetChild(i).GetComponent<ImageObj>().SetVisible(visible);
		}
	}

}
