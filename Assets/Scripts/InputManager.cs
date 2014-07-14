using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour {

	public float MinTwoFingerSwipeDist = 5;

	Vector2 _twoFingerTouchStartPos;
	Vector2 _simulatedFinger2Pos;
	Vector3 _touchDelta;
	Vector3 _oneFingerTotalWorldDelta;
	Vector3 _oneFingerTouchWorldStartPos;

	float _lastTwoFingerSwipeSpeed;
	float _lastFingerDist;

	int _lastTouchCount;
	Vector2 _lastTouchPos;
	float _lastTouchDist;
	Vector2 _touchStartPos;

	int _stopFrames = 20;
	int _currentStopFrame;
	float _timeSinceMouseDown;
	float _clickTime = .1f;

	GameObject _draggableObj = null;
	Vector3 _dragTouchStartPos;
	Vector3 _dragTouchOffset;
	bool _hasLiftedFingersSinceLastSwipe = true;

	public static InputManager Instance;


	// Use this for initialization
	void Awake () {

//		PlayerPrefs.DeleteAll();
		Instance = this;
	}
	

	public int GetTouchCount()
	{
		if (Input.touchCount > 0)
			return Input.touchCount;
		else if (Application.isEditor)
		{
			int count = 0;
			count += (Input.GetMouseButton(0)) ? 1 : 0;
			count += (Input.GetMouseButton(1)) ? 1 : 0;
			return count;
		}
		
		return 0;
	}

	void Update()
	{

//		if (Application.isEditor)
//		{
		if (Input.GetMouseButtonDown(0))
			_timeSinceMouseDown = 0;

		if (Input.GetMouseButton(0))
			_timeSinceMouseDown += Time.deltaTime;

		if (Input.GetMouseButtonUp(0))
		{
//				Debug.Log("time since mouse down: " + _timeSinceMouseDown);
			if (_timeSinceMouseDown <= _clickTime && _touchDelta.magnitude < 5)
			{
				Utils.SendMessageToAll("OnSingleTap",Input.mousePosition);

				Ray ray = Camera.main.ScreenPointToRay(GetTouchPos(0));
				
				RaycastHit hit;
				
				if (Physics.Raycast(ray,out hit, 1000))
				{
					hit.transform.gameObject.SendMessage("OnTap",SendMessageOptions.DontRequireReceiver);

				}
			}

		}


		// if they just touch the screen and stuff is moving.. stop everything
		if (_lastTouchCount == 0 && GetTouchCount() > 0)
		{
//			Debug.Log("just touched screen so stopping");
			_currentStopFrame = _stopFrames;
		}

		// get one finger swipe velocity
		if (IsTouchingWithOneFinger()) // && Input.touches[0].deltaPosition.magnitude > 0)
		{
			
			Vector3 delta = GetTouchPos(0) -  _lastTouchPos;
			
			
			// no movement with finger
			if (delta.magnitude == 0)
				_currentStopFrame++;
			else
				_currentStopFrame = 0;

		}

		if (Input.GetMouseButtonDown(1))
			_simulatedFinger2Pos = Input.mousePosition;

		UpdateTwoFingerSwipe();

		if (GetTouchCount() == 1)
		{
			Vector3 worldTouchPos = GetTouchWorldPos();
	//		Vector3 lastWorldTouchPos = GetLastTouchWorldPos();


			if (_lastTouchCount == 1)
			{
				_touchDelta = GetTouchPos(0) - _lastTouchPos;
				_oneFingerTotalWorldDelta = _oneFingerTouchWorldStartPos -  worldTouchPos;
//				Debug.Log("delta: " + _oneFingerTotalWorldDelta + " start: " + _oneFingerTouchStartPos + " pos: " + worldTouchPos);
				_oneFingerTotalWorldDelta.z = 0;

				if (IsDraggingObject())
				{
					Vector3 newPos = GetDragTouchPos() + _dragTouchOffset;
					newPos.z = _draggableObj.transform.position.z;
					_draggableObj.transform.position = newPos;
				}
			}
			else // on touch began
			{
				_touchStartPos = GetTouchPos(0);
				_oneFingerTouchWorldStartPos = GetTouchWorldPos();
				_touchDelta = Vector3.zero;
				_oneFingerTotalWorldDelta = Vector3.zero;
				HandleTouchDraggable();
			}

			_lastTouchPos =  GetTouchPos(0) ;
		}
		else // not touching with one finger
		{
			_touchDelta = Vector3.zero;
			_draggableObj = null;
		}
		
		_lastTouchCount = GetTouchCount();
	}

	Vector3 GetDragTouchPos()
	{
		return Camera.main.ScreenToWorldPoint(new Vector3( GetTouchPos(0).x,GetTouchPos(0).y, 
		                                      _draggableObj.transform.position.z - Camera.main.transform.position.z ) );
	}

	void HandleTouchDraggable()
	{
		Ray ray = Camera.main.ScreenPointToRay(GetTouchPos(0));

		RaycastHit hit;

		if (Physics.Raycast(ray,out hit, 1000))
		{
			if (hit.transform.GetComponent<Draggable>() != null && hit.transform.GetComponent<Draggable>().IsDraggable)
			{
				_draggableObj = hit.transform.gameObject;
				_dragTouchStartPos = GetDragTouchPos();
				_dragTouchOffset = _draggableObj.transform.position - _dragTouchStartPos;
			}

		}

	}
	public Vector2 GetTouchStartPos()
	{
		return _touchStartPos;
	}

	public bool IsDraggingObject()
	{
		return _draggableObj != null;
	}

	Vector3 ScreenToWorldPos(Vector3 screenPos)
	{
		Ray ray = Camera.main.ScreenPointToRay(screenPos);
		
		RaycastHit[] hits = Physics.RaycastAll(ray);
		
		foreach (RaycastHit hit in hits)
		{
			Scene currentScene = SceneManager.Instance.GetScene();

			if (( ( currentScene == Scene.Browse || currentScene == Scene.Selected) && hit.transform.name == "HitPlane" ) || 
			    ( Community.CurrentCommunity is SpinningShape && (hit.transform.name == "HelixHitCylinder" || hit.transform.name == "FavoritesSphere")))
				return hit.point;
		}
		
		return Vector3.zero;
	}

	public Vector3 GetTouchWorldPos()
	{
		return ScreenToWorldPos( GetTouchPos(0));
	}

	public Vector3 GetLastTouchWorldPos()
	{
		return ScreenToWorldPos(GetLastTouchPos());
	}

	public bool HasFingerStoppedMoving()
	{
		return (_currentStopFrame >= _stopFrames);
	}

	bool UpdateTwoFingerSwipe()
	{


		if (GetTouchCount() < 2)
		{
			return false;
		}
		
		
		float fingerDist = (GetTouchPos(1) - GetTouchPos(0)).magnitude;
		
		
		// did they just put down two fingers?
		if (_lastTouchCount < 2)
		{
			_hasLiftedFingersSinceLastSwipe = true;
			_lastFingerDist = fingerDist;
			return true;
		}
		else if (!_hasLiftedFingersSinceLastSwipe)
		{
			float spreadDist = fingerDist - _lastFingerDist;
		
			Utils.SendMessageToAll("OnTwoFingerSpread",spreadDist);
		}
		else if (_hasLiftedFingersSinceLastSwipe) // they may be moving their fingers
		{

			if (fingerDist > (_lastFingerDist + MinTwoFingerSwipeDist) )
			{
				Utils.SendMessageToAll("OnTwoFingerSwipe",Vector3.forward);
			}
			else if (fingerDist < (_lastFingerDist - MinTwoFingerSwipeDist))
			{
				Utils.SendMessageToAll("OnTwoFingerSwipe",Vector3.back);
				
			}
			_hasLiftedFingersSinceLastSwipe = false;
		}

		_lastFingerDist = fingerDist;
		return true;
	}

	public Vector3 GetOneFingerTotalWorldDelta()
	{
		return _oneFingerTotalWorldDelta;
	}

	public Vector3 GetLastTouchPos()
	{
		return _lastTouchPos;
	}

	public bool IsTouchingWithOneFinger()
	{
		return (_lastTouchCount == 1 && GetTouchCount() == 1 && Input.GetMouseButton(1) == false);
	}

	public Vector3 GetOneFingerTouchDelta()
	{

		if (IsTouchingWithOneFinger())
		{

			return _touchDelta;
		}
		else
			return Vector3.zero;

	}

	public bool IsFingerMoving()
	{
		if (Application.isEditor)
			return _touchDelta.magnitude > 0;
		else
			return Input.touches[0].phase == TouchPhase.Moved;
	}

	public bool IsFingerStationary()
	{
		if (Application.isEditor)
			return _touchDelta.magnitude == 0;
		else
			return Input.touches[0].phase == TouchPhase.Stationary;

	}

	public Vector2 GetTouchPos(int fingerIndex)
	{

		if (Application.isEditor)
		{
			if (fingerIndex == 0)
			{
				
				return Input.mousePosition;
			}
			else if (fingerIndex == 1)
			{
				return _simulatedFinger2Pos;
			}
		}
		else 
		{
			if (fingerIndex >= Input.touchCount)
				return Vector2.zero;
			else
				return  Input.GetTouch(fingerIndex).position;
		}
		
		return Vector2.zero;
	}

}
