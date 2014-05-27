using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {

	public float Speed = 10;
	public float MaxSpeed = 15;
	public float OneFingerSwipeSpeed = .5f;
	public float TwoFingerSwipeSpeed = .1f;
	public float Friction = 2;
	public float MinTwoFingerSwipeDist = 5;
	public float MaxAngularSpeed = 6;

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
	int _stopFrames = 20;

	Transform _helixObj;
	int _currentStopFrame;
	bool _hasLiftedFingersSinceLastSwipe = true;


	public static CameraManager Instance;

	// Use this for initialization
	void Awake () {
		Instance = this;

		_helixObj = transform.Find("HelixBottom");
//		transform.rotation = Quaternion.LookRotation( Quaternion.AngleAxis(-30,Vector3.up) * Vector3.forward);

		_forward = Quaternion.AngleAxis(transform.rotation.y,Vector3.up) * Vector3.forward;
		_right = Quaternion.AngleAxis(transform.rotation.y,Vector3.up) * Vector3.right;

		float screenFactor = Screen.width / 1024.0f;
//		OneFingerSwipeSpeed *= screenFactor;
//		TwoFingerSwipeSpeed *= screenFactor;
//		MaxSpeed *= screenFactor;
//		MaxAngularSpeed *= screenFactor;
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

		if (SceneManager.Instance.GetScene() != Scene.Browse && SceneManager.Instance.GetScene() != Scene.Helix)
			return;

		if (Input.GetMouseButtonDown(1))
			_simulatedFinger2Pos = Input.mousePosition;

		// if they just touch the screen and stuff is moving.. stop everything
		if (_lastTouchCount == 0 && GetTouchCount() == 1)
		{
			_currentStopFrame = _stopFrames;
		}

		// get one finger swipe velocity
		if (_lastTouchCount == 1 && GetTouchCount() == 1 && Input.GetMouseButton(1) == false) // && Input.touches[0].deltaPosition.magnitude > 0)
		{

			Vector3 delta = ScreenToWorldPos(_lastTouchPos) - ScreenToWorldPos( GetTouchPos(0) );


			// no movement with finger
			if (delta.magnitude == 0)
			{
				_currentStopFrame++;
				if (_currentStopFrame >= _stopFrames)
				{
					Debug.Log("stopping");
					_velocity = Vector3.zero;
				}

			}
			else // some movement
			{
				_currentStopFrame = 0;

				UpdateVelocity(delta);
			}

		}


		// two finger swipe?
		if (UpdateTwoFingerSwipe())
		{
			_velocity = Vector3.zero;
		}


		// apply velocity and friction
		float magnitude = LimitVelocity();


		if (magnitude > 0)
		{

			// do helix movement if we are in the helix
			if (SceneManager.Instance.GetScene() == Scene.Helix)
			{
				HelixUpdate();
			}
			else
				transform.position += _velocity;
				
			// apply friction
			magnitude -= Friction ;
			magnitude = Mathf.Max(0,magnitude);
			_velocity = _velocity.normalized * magnitude;
		}

		// if they were moving through depth and let go.. stop
		if (  Mathf.Abs( Vector3.Dot(_velocity,_forward) ) > .01f && GetTouchCount() == 0 && SceneManager.Instance.GetScene() == Scene.Browse)
		{
			_velocity = Vector3.zero;
		}


		if (GetTouchCount() == 1)
			_lastTouchPos =  GetTouchPos(0) ;

		_lastTouchCount = GetTouchCount();
	}

	float LimitVelocity()
	{		
		float magnitude =_velocity.magnitude;
		float maxSpeed = (SceneManager.Instance.GetScene() == Scene.Helix) ? MaxAngularSpeed : MaxSpeed;


		if (magnitude > maxSpeed)
		{
			_velocity = _velocity.normalized * maxSpeed;
			magnitude = maxSpeed;
		}

		return magnitude;

	}

	void UpdateVelocity(Vector3 delta)
	{
		if (SceneManager.Instance.GetScene() == Scene.Browse)
			_velocity = delta;
		else if (SceneManager.Instance.GetScene() == Scene.Helix)
		{
			if (ScreenToWorldPos( GetTouchPos(0)) != Vector3.zero)
			{
				Vector3 lastPos = ScreenToWorldPos(_lastTouchPos) - _helixObj.position;
				Vector3 currentPos = ScreenToWorldPos( GetTouchPos(0)) - _helixObj.position;
				lastPos.y = 0;
				currentPos.y = 0;
				float dir = (Vector3.Cross(lastPos,currentPos).y > 0) ? 1 : -1;
				float angleDelta = Vector3.Angle(lastPos,currentPos);
				
				if (angleDelta > 0)
				{
					angleDelta = Mathf.Min(angleDelta,MaxAngularSpeed);
					_velocity = new Vector3( angleDelta * dir , delta.y, 0);
				}
			}

		}
	}

	void HelixUpdate()
	{

		GridManager.Instance.transform.RotateAround(_helixObj.position,Vector3.up,_velocity.x);
		transform.position += new Vector3(0,_velocity.y,0);

		// clamp bounds
		if (transform.position.y > GridManager.Instance.GetHelixMaxY())
			transform.position = new Vector3(transform.position.x, GridManager.Instance.GetHelixMaxY(), transform.position.z);
		else if (transform.position.y < GridManager.Instance.GetHelixMinY())
			transform.position = new Vector3(transform.position.x, GridManager.Instance.GetHelixMinY(), transform.position.z);
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
			return  Input.GetTouch(fingerIndex).position;
		}

		return Vector2.zero;
	}

	Vector3 ScreenToWorldPos(Vector3 screenPos)
	{
		Ray ray = Camera.main.ScreenPointToRay(screenPos);

		RaycastHit[] hits = Physics.RaycastAll(ray);

		foreach (RaycastHit hit in hits)
		{
			Scene scene = SceneManager.Instance.GetScene();
			if (( scene == Scene.Browse && hit.transform.name == "HitPlane" ) || 
			    ( scene == Scene.Helix && hit.transform.name == "HelixHitCylinder" ))
				return hit.point;
		}

		return Vector3.zero;
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
		else if (LeanTween.isTweening(gameObject) == false && _hasLiftedFingersSinceLastSwipe) // they may be moving their fingers
		{
	//		Debug.Log("finger dist: " + (_lastFingerDist - fingerDist));

			if (fingerDist > (_lastFingerDist + MinTwoFingerSwipeDist) )
			{
				LeanTween.move(gameObject,transform.position + _forward * GridManager.Instance.GetZPadding(),1).setEase(LeanTweenType.easeOutQuint);
			}
			else if (fingerDist < (_lastFingerDist - MinTwoFingerSwipeDist))
			{
				LeanTween.move(gameObject,transform.position - _forward * GridManager.Instance.GetZPadding(),1).setEase(LeanTweenType.easeOutQuint);
			}



		}
		_lastFingerDist = fingerDist;
		return true;
	}


	Vector3 GetWorldPos(Vector2 touchPos)
	{
		return Camera.main.ScreenToWorldPoint(new Vector3(touchPos.x,touchPos.y,10));

	}
}
