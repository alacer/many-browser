using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour {

	public float MinTwoFingerSwipeDist = 5;

	Vector2 _twoFingerTouchStartPos;
	Vector2 _simulatedFinger2Pos;
	float _lastTwoFingerSwipeSpeed;
	float _lastFingerDist;

	int _lastTouchCount;
	Vector2 _lastTouchPos;
	float _lastTouchDist;

	int _stopFrames = 20;
	int _currentStopFrame;
	float _timeSinceMouseDown;
	float _clickTime = .1f;

	bool _hasLiftedFingersSinceLastSwipe = true;

	public static InputManager Instance;


	// Use this for initialization
	void Awake () {
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
				Debug.Log("time since mouse down: " + _timeSinceMouseDown);
				if (_timeSinceMouseDown <= _clickTime)
					Utils.SendMessageToAll("OnSingleTap",Input.mousePosition);

			}
//		}


//		foreach(Touch touch in Input.touches)
//		{
//
//			if (touch.phase == TouchPhase.Ended && touch.tapCount == 1)
//			{
//				Debug.Log("got tap");
//				SendMessageToAll("OnSingleTap",new Vector3(touch.position.x,touch.position.y,0));
//				break;
//			}
//		}

		// if they just touch the screen and stuff is moving.. stop everything
		if (_lastTouchCount == 0 && GetTouchCount() > 0)
		{
			Debug.Log("just touched screen so stopping");
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
			_lastTouchPos =  GetTouchPos(0) ;
		
		_lastTouchCount = GetTouchCount();
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
			
			
			
		}
		_lastFingerDist = fingerDist;
		return true;
	}
	

	public Vector3 GetLastTouchPos()
	{
		return _lastTouchPos;
	}

	public bool IsTouchingWithOneFinger()
	{
		return (_lastTouchCount == 1 && GetTouchCount() == 1 && Input.GetMouseButton(1) == false);
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
