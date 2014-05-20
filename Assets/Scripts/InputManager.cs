using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour {

	public float Speed = 10;
	public float MaxSpeed = 15;
	public float OneFingerSwipeSpeed = .5f;
	public float TwoFingerSwipeSpeed = .1f;
	public float Friction = 2;

	int _lastTouchCount;
	Vector2 _lastTouchPos;
	float _lastTouchDist;

	Vector3 _forward;
	Vector3 _right;
	Vector3 _moveDir;
	Vector3 _velocity;
	Vector2 _twoFingerTouchStartPos;
	Vector2 _simulatedFinger2Pos;
	float _lastTwoFingerSwipeSpeed;
	int _stopFrames = 5;
	int _currentStopFrame;


	public static InputManager Instance;

	// Use this for initialization
	void Awake () {
		Instance = this;
//		transform.rotation = Quaternion.LookRotation( Quaternion.AngleAxis(-30,Vector3.up) * Vector3.forward);

		_forward = Quaternion.AngleAxis(transform.rotation.y,Vector3.up) * Vector3.forward;
		_right = Quaternion.AngleAxis(transform.rotation.y,Vector3.up) * Vector3.right;

		float screenFactor = Screen.width / 1024.0f;
		OneFingerSwipeSpeed *= screenFactor;
		TwoFingerSwipeSpeed *= screenFactor;
		MaxSpeed *= screenFactor;
	}

	public Vector3 GetMoveDir()
	{
		Vector3 movDir = Quaternion.AngleAxis(-transform.rotation.y,Vector3.up) *_velocity;

		return movDir;
	}

	// Update is called once per frame
	void FixedUpdate () {

		if (Input.GetMouseButtonDown(1))
			_simulatedFinger2Pos = Input.mousePosition;

		if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0)
			_velocity =  (_forward * Input.GetAxis("Vertical") + _right * Input.GetAxis("Horizontal")) * Speed;

		// if they just touch the screen and stuff is moving.. stop everything
		if (_lastTouchCount == 0 && GetTouchCount() == 1)
			_currentStopFrame = _stopFrames;

		// get one finger swipe velocity
		if (_lastTouchCount == 1 && GetTouchCount() == 1 && Input.GetMouseButton(1) == false) // && Input.touches[0].deltaPosition.magnitude > 0)
		{

			Vector3 delta = (GetWorldPos(_lastTouchPos) - GetWorldPos(GetTouchPos(0)));
			float sign = (delta.x > 0) ? 1 : -1;
			delta = delta.magnitude * _right * sign;

			// no movement with finger
			if (delta.magnitude == 0)
			{
				_currentStopFrame++;
				if (_currentStopFrame >= _stopFrames)
					_velocity = Vector3.zero;

			}
			else // some movement
			{
				_currentStopFrame = 0;

				_velocity = delta * OneFingerSwipeSpeed;
			}

		}


		// two finger swipe?
		float speed;
		if (GetTwoFingerSwipeSpeed(out speed))
		{
			_velocity = _forward * speed;
		}

		// apply velocity and friction
		float magnitude = _velocity.magnitude;
//		Debug.Log("speed: " + magnitude);

		if (magnitude > MaxSpeed)
		{
			_velocity = _velocity.normalized * MaxSpeed;
			magnitude = MaxSpeed;
		}

		if (magnitude > 0)
		{
			transform.position += _velocity;
				
			magnitude -= Friction;

			magnitude = Mathf.Max(0,magnitude);

			_velocity = _velocity.normalized * magnitude;
		}


		if (GetTouchCount() == 1)
			_lastTouchPos = GetTouchPos(0);
		_lastTouchCount = GetTouchCount();
	}



	int GetTouchCount()
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

	Vector2 GetTouchPos(int fingerIndex)
	{
		if (Application.isEditor)
		{
			if (fingerIndex == 0)
				return Input.mousePosition;
			else if (fingerIndex == 1)
			{
				return _simulatedFinger2Pos;
			}
		}
		else 
		{
			return Input.GetTouch(fingerIndex).position;
		}

		return Vector2.zero;
	}

	bool GetTwoFingerSwipeSpeed(out float speed)
	{

		if (GetTouchCount() < 2)
		{
			speed = 0;
			return false;
		}

		if (_lastTouchCount < 2 && GetTouchCount() >= 2)
			_twoFingerTouchStartPos = GetTouchPos(0);

		speed = (GetTouchPos(0) - _twoFingerTouchStartPos).y * TwoFingerSwipeSpeed;
		return true;


//		float fingerDist = (GetTouchPos(1) - GetTouchPos(0)).magnitude;
//
//		// did they just put down two fingers?
//		if (_lastFingerDist == 0)
//		{
//			_lastFingerDist = fingerDist;
//			speed = 0;
//			return false;
//		}
//		else // they may be moving their fingers
//		{
//			float newSpeed = (fingerDist - _lastFingerDist) * TwoFingerSwipeSpeed;
//
//			// not moving fingers, if the stop for a few frames then stop
//			if (newSpeed == 0)
//			{
//				_currentStopFrame++;
//
//				if (_currentStopFrame >= _stopFrames)
//				{
//					speed = 0;
//					return true;
//				}
//			}
//			else // they are moving their fingers
//				_currentStopFrame = 0;
//
//			
//			speed = newSpeed;
//
//			_lastFingerDist = fingerDist;
//			_lastTwoFingerSwipeSpeed = speed;
//
//		return true;
//		}

	}

//	Vector2 GetTouchPos()
//	{
//		if (Application.isEditor)
//			return new Vector2(Input.mousePosition.x,Input.mousePosition.y);
//		else
//			return Input.touches[0].position;
//	}

	Vector3 GetWorldPos(Vector2 touchPos)
	{
		return Camera.main.ScreenToWorldPoint(new Vector3(touchPos.x,touchPos.y,10));

	}
}
