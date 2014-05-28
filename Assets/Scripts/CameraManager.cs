using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {

	public float Speed = 10;
	public float MaxSpeed = 15;
	public float OneFingerSwipeSpeed = .5f;
	public float TwoFingerSwipeSpeed = .1f;
	public float Friction = 2;

	public float MaxAngularSpeed = 6;
	
	Vector3 _tweenDir;

	Vector3 _forward;
	Vector3 _right;
	Vector3 _moveDir;
	Vector3 _velocity;
	Transform _helixObj;
	Scene _currentScene;


	public static CameraManager Instance;


	// Use this for initialization
	void Awake () {
		Instance = this;
		
		_helixObj = transform.Find("HelixBottom");
		
		_forward = Quaternion.AngleAxis(transform.rotation.y,Vector3.up) * Vector3.forward;
		_right = Quaternion.AngleAxis(transform.rotation.y,Vector3.up) * Vector3.right;
		
		
	}
	
	

#region Finger Swiping
	

	void OnTwoFingerSwipe(Vector3 dir)
	{
		if (LeanTween.isTweening(gameObject))
			return;

		if (_currentScene == Scene.Browse)
			HandleBrowseTwoFingerSwipe(dir);

	}

	
	void HandleBrowseTwoFingerSwipe(Vector3 dir)
	{
		Debug.Log("two finger swipe stop");
		_velocity = Vector3.zero;
		
		_tweenDir = dir;
		
		Vector3 moveDir = (dir.z == 1) ? _forward : -_forward;
		
		LeanTween.move(gameObject,transform.position + moveDir * GridManager.Instance.GetZPadding(),1).setEase(LeanTweenType.easeOutQuint).setOnComplete( () =>
		                                                                                                                                                 {
			_tweenDir = Vector3.zero;
			
		});
	}


#endregion

#region movement

	// Update is called once per frame
	void FixedUpdate () {
		_currentScene = SceneManager.Instance.GetScene();


		if (SceneManager.IsInHelixOrBrowse() == false)
			return;

		Vector3 worldTouchPos = ScreenToWorldPos( InputManager.Instance.GetTouchPos(0));
		Vector3 lastWorldTouchPos = ScreenToWorldPos(InputManager.Instance.GetLastTouchPos());


		// get one finger swipe velocity
		if (InputManager.Instance.IsTouchingWithOneFinger()) // && Input.touches[0].deltaPosition.magnitude > 0)
		{

			Vector3 delta = lastWorldTouchPos -  worldTouchPos;

			if (InputManager.Instance.HasFingerStoppedMoving())
				_velocity = Vector3.zero;
			else
				UpdateVelocity(delta,worldTouchPos,lastWorldTouchPos);


		}


		// apply velocity and friction
		float magnitude = LimitVelocity();


		if (magnitude > 0)
		{

			// do helix movement if we are in the helix
			if (_currentScene == Scene.Helix)
			{
				HelixUpdate();
			}
			else
				transform.position += _velocity;
				
			ApplyFriction(magnitude);
		}

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
		float maxSpeed = (_currentScene == Scene.Helix) ? MaxAngularSpeed : MaxSpeed;


		if (magnitude > maxSpeed)
		{
			_velocity = _velocity.normalized * maxSpeed;
			magnitude = maxSpeed;
		}

		return magnitude;

	}

	void UpdateVelocity(Vector3 delta, Vector3 worldTouchPos, Vector3 lastWorldTouchPos)
	{
		if (_currentScene == Scene.Browse)
			_velocity = delta;
		else if (_currentScene == Scene.Helix)
		{
			if (worldTouchPos != Vector3.zero)
			{
				Vector3 lastPos = lastWorldTouchPos - _helixObj.position;
				Vector3 currentPos = worldTouchPos - _helixObj.position;
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

#endregion
	

	
#region Helper Functions

	public Vector3 GetForward()
	{
		return _forward;
	}

	Vector3 ScreenToWorldPos(Vector3 screenPos)
	{
		Ray ray = Camera.main.ScreenPointToRay(screenPos);

		RaycastHit[] hits = Physics.RaycastAll(ray);

		foreach (RaycastHit hit in hits)
		{
			if (( _currentScene == Scene.Browse && hit.transform.name == "HitPlane" ) || 
			    ( _currentScene == Scene.Helix && hit.transform.name == "HelixHitCylinder" ))
				return hit.point;
		}

		return Vector3.zero;
	}
	
	public Vector3 GetMoveDir()
	{
		
		
		Vector3 movDir = Quaternion.AngleAxis(-transform.rotation.y,Vector3.up) *_velocity;
		
		
		movDir += _tweenDir;
		return movDir;
	}
	
	public Vector3 MoveDirToWorldDir(Vector2 moveDir)
	{
		Vector3 worldDir = Quaternion.AngleAxis(transform.rotation.y,Vector3.up) * new Vector3( moveDir.x, moveDir.y, 0);
		
		return worldDir;
	}
	


	Vector3 GetWorldPos(Vector2 touchPos)
	{
		return Camera.main.ScreenToWorldPoint(new Vector3(touchPos.x,touchPos.y,10));

	}

#endregion
}
