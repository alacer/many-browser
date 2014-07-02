using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {

	public bool SkipIntro;
	public float MaxX;
	public float MinX;
	public float MaxY;
	public float MinY;
	public float MaxZ;
	public float MinZ;
	public float Speed = 10;
	public float ZoomSpeed = .001f;
	public float MaxSpeed = 15;
	public float OneFingerSwipeSpeed = .5f;
	public float TwoFingerSwipeSpeed = .1f;
	public float Friction = 2;

	public float MaxAngularSpeed = 6;

	Vector3 _forward;
	Vector3 _moveDir;
	Vector3 _velocity;
	Vector3 _savedPos;
	Vector3 _savedRotation;

	ImageObj _backCommunityItem;

	Transform _helixObj;
	Scene _currentScene;


	public static CameraManager Instance;


	// Use this for initialization
	void Awake () {
		Instance = this;
		LeanTween.init(3000);
		_helixObj = transform.Find("HelixBottom");
		
		_forward = Quaternion.AngleAxis(transform.rotation.y,Vector3.up) * Vector3.forward;
		
		if (SkipIntro)
			animation["CameraAnim"].speed = 10;
	}


#region Finger Swiping

	void OnTwoFingerSpread(float spread)
	{


		_velocity = GetForward()*spread * ZoomSpeed;

	}
	

#endregion

#region movement

	// Update is called once per frame
	void FixedUpdate () {
		_currentScene = SceneManager.Instance.GetScene();

//		if (SceneManager.IsInHelixOrBrowse() == false)
//			return;

		Vector3 worldTouchPos = ScreenToWorldPos( InputManager.Instance.GetTouchPos(0));
		Vector3 lastWorldTouchPos = ScreenToWorldPos(InputManager.Instance.GetLastTouchPos());


		// get one finger swipe velocity
		if (InputManager.Instance.IsTouchingWithOneFinger() && _currentScene == Scene.Browse) // && Input.touches[0].deltaPosition.magnitude > 0)
		{

			Vector3 delta = lastWorldTouchPos -  worldTouchPos;
			delta.z = 0;

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
			{
				transform.position += _velocity;
				HandleCommunityTransitions();
			}
				
	//		ApplyBounds();
			ApplyFriction(magnitude);
		}

	}

#region CommunityTransitions

	void HandleCommunityTransitions()
	{
		if (_velocity.z == 0 || LeanTween.isTweening(gameObject))
			return;

		if (_velocity.z > 0)
		{
			HandleForwardCommmunityTransitions();
		}
		else if (_velocity.z < 0)
		{
			HandleBackwardCommunityTransitions();
		}


	}

	void HandleBackwardCommunityTransitions()
	{
		if (Community.BackCommunity != null && transform.position.z < _backCommunityItem.transform.position.z + 1)
		{
			Debug.Log("doing back transition");
			_velocity = Vector3.zero;

			StartCoroutine( Community.ForwardCommunity.FadeOutAndRemove() );

			Community.ForwardCommunity = Community.BackCommunity;
			Community.BackCommunity = null;
			_backCommunityItem.DoCommunityBackTransition();
			LeanTween.move(gameObject,_backCommunityItem.transform.position + Vector3.back, 1);
		}

	}

	void HandleForwardCommmunityTransitions()
	{

		Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width/2.0f,Screen.height/2.0f));
		//	Debug.DrawRay(ray.origin,ray.direction*1000);
		RaycastHit hit;
		
		
		if (Physics.Raycast(ray, out hit, .5f, 1 << LayerMask.NameToLayer("ImageObj")))
		{
			ImageObj obj = hit.transform.GetComponent<ImageObj>();
			
			// if we are hitting a community transition into it
			if (obj.IsCommunity())
			{
				DoForwardTransitionOnObj(obj);
				Debug.Log("starting transition: " + hit.transform.name);
			}

			//	hit.transform.localScale *= .5f;
		}
	}

	public void DoForwardTransitionOnObj(ImageObj obj)
	{
		_velocity = Vector3.zero;
		_backCommunityItem = obj;
		Community.BackCommunity = Community.ForwardCommunity;


		Vector3 targetPos = new Vector3(transform.position.x, transform.position.y, Community.BackCommunity.transform.position.z + 2);


		Community.ForwardCommunity = obj.DoCommunityForwardTransition(targetPos).GetComponent<Community>();

		LeanTween.move(gameObject,targetPos, 1).setOnComplete( () => {
			_velocity = Vector3.zero;
		});
		
	}
	
	
	#endregion

	void ApplyBounds()
	{
		Vector3 pos = transform.position;


		if (pos.x > MaxX)
			transform.position = new Vector3(MaxX,transform.position.y,transform.position.z);
		else if (pos.x < MinX)
			transform.position = new Vector3(MinX,transform.position.y,transform.position.z);

		if (pos.y > MaxY)
			transform.position = new Vector3(transform.position.x,MaxY,transform.position.z);
		else if (pos.y < MinY)
			transform.position = new Vector3(transform.position.x,MinY,transform.position.z);

		if (pos.z > MaxZ)
			transform.position = new Vector3(transform.position.x,transform.position.y,MaxZ);
		else if (pos.z < MinZ)
			transform.position = new Vector3(transform.position.x,transform.position.y,MinZ);
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

	void OnSceneChange(Scene newScene)
	{
		_velocity = Vector3.zero;
	}

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
			if (( ( _currentScene == Scene.Browse || _currentScene == Scene.Selected) && hit.transform.name == "HitPlane" ) || 
			    ( _currentScene == Scene.Helix && hit.transform.name == "HelixHitCylinder" ))
				return hit.point;
		}

		return Vector3.zero;
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

	public void MoveToSavedPlace(float animTime)
	{
		transform.position = _savedPos;
		transform.rotation = Quaternion.Euler(_savedRotation);

//		LeanTween.move(gameObject,_savedPos,animTime).setEase(LeanTweenType.easeOutExpo);
//		LeanTween.rotate(gameObject,_savedRotation,animTime).setEase(LeanTweenType.easeOutExpo);


	}
	
	public void SavePlace()
	{
		_savedPos = transform.position;
		_savedRotation = transform.rotation.eulerAngles;
	}

#endregion
}
