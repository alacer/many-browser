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

	Vector3 _forward;
	Vector3 _moveDir;
	Vector3 _velocity;
	Vector3 _savedPos;
	Vector3 _savedRotation;

	ImageObj _backCommunityItem;
	
	Scene _currentScene;


	public static CameraManager Instance;


	// Use this for initialization
	void Awake () {
		Instance = this;
		Application.targetFrameRate = 60;
		LeanTween.init(3000);
		
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

		if (SceneManager.IsInHelixOrBrowse() == false)
			return;


		// get one finger swipe velocity
		if (InputManager.Instance.IsTouchingWithOneFinger() && _currentScene == Scene.Browse)
		{

			if (InputManager.Instance.HasFingerStoppedMoving())
				_velocity = Vector3.zero;
			else
				_velocity = InputManager.Instance.GetOneFingerDelta();

		}


		// apply velocity and friction
		float magnitude = LimitVelocity();


		if (magnitude > 0)
		{

			// do helix movement if we are in the helix
			if (_currentScene == Scene.Helix)
			{
				_velocity.x = 0;
				_velocity.y = 0;
			}

			transform.position += _velocity;
			HandleCommunityTransitions();

				
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

		Vector3 targetPos = GetForwardTransitionTargetPos();
		Community.BackCommunity = Community.ForwardCommunity;

		Community.ForwardCommunity = obj.DoCommunityForwardTransition(targetPos).GetComponent<Community>();

		LeanTween.move(gameObject,targetPos, 1).setOnComplete( () => {
			_velocity = Vector3.zero;
		});
		
	}

	public void DoToHelixTransition()
	{
		_velocity = Vector3.zero;

		Community.ForwardCommunity.FadeOut(1);

		Community.BackCommunity = Community.ForwardCommunity;
		Community.ForwardCommunity = HelixManager.Instance;

		Vector3 targetPos = HelixManager.Instance.GetTopObjPos() + Vector3.back*2;

		LeanTween.move(gameObject,targetPos, 2).setDelay(1).setOnComplete( () => {
			_velocity = Vector3.zero;
		});

	}

	public Vector3 GetForwardTransitionTargetPos()
	{


		return new Vector3(transform.position.x, transform.position.y, Community.ForwardCommunity.transform.position.z + 2);
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
		float maxSpeed = MaxSpeed;


		if (magnitude > maxSpeed)
		{
			_velocity = _velocity.normalized * maxSpeed;
			magnitude = maxSpeed;
		}

		return magnitude;

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
