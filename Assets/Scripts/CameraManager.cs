using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {

	public float Speed = 10;
	public float MaxSpeed = 15;
	public float OneFingerSwipeSpeed = .5f;
	public float TwoFingerSwipeSpeed = .1f;
	public float Friction = 2;
	public float MinTwoFingerSwipeDist = 5;

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
	float _lastFingerDist;
	int _stopFrames = 5;

	int _currentStopFrame;
	bool _hasLiftedFingersSinceLastSwipe = true;


	public static CameraManager Instance;

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

	//	Debug.Log("vel: " + _velocity);
		Vector3 movDir = Quaternion.AngleAxis(-transform.rotation.y,Vector3.up) *_velocity;
//		Debug.Log("vel: " + _velocity + " movedir: " + movDir);
		return movDir;
	}

	public Vector3 MoveDirToWorldDir(Vector2 moveDir)
	{
		Vector3 worldDir = Quaternion.AngleAxis(transform.rotation.y,Vector3.up) * new Vector3( moveDir.x, moveDir.y, 0);
		
		return worldDir;
	}

	// Update is called once per frame
	void FixedUpdate () {

		if (SceneManager.Instance.GetScene() != Scene.Browse)
			return;

		if (Input.GetMouseButtonDown(1))
			_simulatedFinger2Pos = Input.mousePosition;

		if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0)
			_velocity =  (_forward * Input.GetAxis("Vertical") + _right * Input.GetAxis("Horizontal")) * Speed;

		// if they just touch the screen and stuff is moving.. stop everything
		if (_lastTouchCount == 0 && GetTouchCount() == 1)
		{
			_currentStopFrame = _stopFrames;
	//		_oneFingerTouchStartPos = GetTouchPos(0);
		}

		// get one finger swipe velocity
		if (_lastTouchCount == 1 && GetTouchCount() == 1 && Input.GetMouseButton(1) == false) // && Input.touches[0].deltaPosition.magnitude > 0)
		{

//			Vector2 delta = (GetTouchPos(0) - _oneFingerTouchStartPos);
//			_velocity = delta * OneFingerSwipeSpeed;

			Vector3 delta = MoveDirToWorldDir (GetWorldPos(_lastTouchPos) - GetWorldPos(GetTouchPos(0)));
	//		float sign = (delta.x > 0) ? 1 : -1;
	//		delta = delta.magnitude * _right * sign;

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
		if (UpdateTwoFingerSwipe())
		{
			_velocity = Vector3.zero;// _forward * speed;
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

		// if they were moving through depth and let go.. stop
		if (  Mathf.Abs( Vector3.Dot(_velocity,_forward) ) > .01f && GetTouchCount() == 0)
			_velocity = Vector3.zero;


		if (GetTouchCount() == 1)
			_lastTouchPos = GetTouchPos(0);
		_lastTouchCount = GetTouchCount();
	}

	public Vector3 GetForward()
	{
		return _forward;
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

	bool UpdateTwoFingerSwipe()
	{

		if (GetTouchCount() < 2)
		{
			return false;
		}

//		if (_lastTouchCount < 2 && GetTouchCount() >= 2)
//			_twoFingerTouchStartPos = GetTouchPos(0);
//
//		speed = (GetTouchPos(0) - _twoFingerTouchStartPos).y * TwoFingerSwipeSpeed;
//		return true;


		float fingerDist = (GetTouchPos(1) - GetTouchPos(0)).magnitude;

		// did they just put down two fingers?
		if (_lastTouchCount < 2)
		{
			_hasLiftedFingersSinceLastSwipe = true;
			_lastFingerDist = fingerDist;
			return true;
		}
		else if (LeanTween.isTweening(gameObject) == false && _hasLiftedFingersSinceLastSwipe) // they may be moving their fingers
		{

			if (fingerDist > (_lastFingerDist + MinTwoFingerSwipeDist) )
			{
				LeanTween.move(gameObject,transform.position + _forward * GridManager.Instance.GetZPadding(),1).setEase(LeanTweenType.easeOutQuint);
			}
			else if (fingerDist < (_lastFingerDist - MinTwoFingerSwipeDist))
			{
				LeanTween.move(gameObject,transform.position - _forward * GridManager.Instance.GetZPadding(),1).setEase(LeanTweenType.easeOutQuint);
			}

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


//			_lastTwoFingerSwipeSpeed = speed;


		}
		_lastFingerDist = fingerDist;
		return true;
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
