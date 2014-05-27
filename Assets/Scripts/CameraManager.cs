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

	Transform _selectedObj;
	Transform _previousParent;
	Vector3 _previousPos;
	Vector3 _previousRot;


	public static CameraManager Instance;


	// Use this for initialization
	void Awake () {
		Instance = this;
		
		_helixObj = transform.Find("HelixBottom");
		
		_forward = Quaternion.AngleAxis(transform.rotation.y,Vector3.up) * Vector3.forward;
		_right = Quaternion.AngleAxis(transform.rotation.y,Vector3.up) * Vector3.right;
		
		
	}
	
#region Selection

	void OnSingleTap(Vector3 screenPos)
	{
		
		Debug.Log("current scene: " + _currentScene);
		Ray ray = Camera.main.ScreenPointToRay(screenPos);
		Debug.DrawRay(ray.origin,ray.direction*1000);
		RaycastHit hit;
		
		
		if (Physics.Raycast(ray, out hit,1000, 1 << LayerMask.NameToLayer("ImageObj")))
		{
			if (IsInHelixOrBrowse())
				OnSelectObj(hit.transform);
			Debug.Log("hit obj: " + hit.transform.name);
			//	hit.transform.localScale *= .5f;
		}
		
	}

	void OnSelectObj(Transform obj)
	{
		SceneManager.Instance.OnSceneTransition();
		_selectedObj = obj;

		_previousPos = obj.position;
		_previousRot = obj.rotation.eulerAngles;
		_previousParent = obj.parent;

		obj.parent = transform;

		_velocity = Vector3.zero;
		LeanTween.cancel(gameObject);
		float animTime = .3f;
		Vector3 left = -Vector3.Cross(Vector3.up,GetForward());
		LeanTween.move(obj.gameObject,transform.position + GetForward()*.9f + left*.5f ,animTime).setEase(LeanTweenType.easeOutQuad);
		LeanTween.rotateLocal(obj.gameObject,Vector3.zero, animTime).setEase(LeanTweenType.easeOutQuad).setOnComplete ( () =>
		{
			SceneManager.Instance.PushScene(Scene.Selected);
		});
	}

	void LeaveSelectedObj()
	{
		SceneManager.Instance.OnSceneTransition();

		_selectedObj.parent = _previousParent;
		float animTime = .3f;
		LeanTween.move(_selectedObj.gameObject,_previousPos,animTime);
		LeanTween.rotate(_selectedObj.gameObject,_previousRot, animTime).setOnComplete ( () =>
		                                                            {
			SceneManager.Instance.PopScene();
		});
	}

#endregion
	

#region Finger Swiping
	

	void OnTwoFingerSwipe(Vector3 dir)
	{
		if (LeanTween.isTweening(gameObject))
			return;

		if (_currentScene == Scene.Browse)
			HandleBrowseTwoFingerSwipe(dir);
		else if (_currentScene == Scene.Selected && dir == Vector3.back)
			LeaveSelectedObj();

	}

	
	void HandleBrowseTwoFingerSwipe(Vector3 dir)
	{
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


		if (IsInHelixOrBrowse() == false)
			return;

	//	Debug.Log("current scene " + _currentScene + " is in: " +IsInHelixOrBrowse());
		Vector3 worldTouchPos = ScreenToWorldPos( InputManager.Instance.GetTouchPos(0));
		Vector3 lastWorldTouchPos = ScreenToWorldPos(InputManager.Instance.GetLastTouchPos());


		// get one finger swipe velocity
		if (InputManager.Instance.IsTouchingWithOneFinger()) // && Input.touches[0].deltaPosition.magnitude > 0)
		{

			Vector3 delta = lastWorldTouchPos -  worldTouchPos;

			Debug.Log("delta: " +delta);
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
	
	bool IsInHelixOrBrowse()
	{
		return (_currentScene == Scene.Helix || _currentScene == Scene.Browse);
	}

	Vector3 GetWorldPos(Vector2 touchPos)
	{
		return Camera.main.ScreenToWorldPoint(new Vector3(touchPos.x,touchPos.y,10));

	}

#endregion
}
