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
	public float DragSpeed = .5f;
	public float ZoomSpeed = .001f;
	public float MaxSpeed = 15;
//	public float OneFingerSwipeSpeed = .5f;
//	public float TwoFingerSwipeSpeed = .1f;
	public float Friction = 2;

	Vector3 _forward;
	Vector3 _moveDir;
	Vector3 _velocity;


	Transform _hitPlane;
	Scene _currentScene;
	bool _backOutToKartua;

	public static CameraManager Instance;
	
	// Use this for initialization
	void Awake () {
		Instance = this;

		PlayerPrefs.DeleteAll();

		AddFavorites();
	

		Application.targetFrameRate = 60;
		LeanTween.init(3000);
		_hitPlane = GameObject.Find("HitPlane").transform;

		_forward = Quaternion.AngleAxis(transform.rotation.y,Vector3.up) * Vector3.forward;
		
		if (SkipIntro && Application.isEditor)
			animation["CameraAnim"].speed = 10;


	}

	void AddFavorites()
	{
		FavoritesButton.SaveFavorite("Textures/Now/alien_meal");
		FavoritesButton.SaveFavorite("Textures/Now/amazon");
		FavoritesButton.SaveFavorite("Textures/Now/camera_3");
		FavoritesButton.SaveFavorite("Textures/Now/contacts");
		FavoritesButton.SaveFavorite("Textures/Now/coworkers");
		FavoritesButton.SaveFavorite("Textures/Now/design project");
		FavoritesButton.SaveFavorite("Textures/Now/devices");
		FavoritesButton.SaveFavorite("Textures/Now/family");
		FavoritesButton.SaveFavorite("Textures/Now/ferris_wheel");
		FavoritesButton.SaveFavorite("Textures/Now/gopro");
//		FavoritesButton.SaveFavorite("Textures/Now/hamburger");
		FavoritesButton.SaveFavorite("Textures/Now/healthy meals");
		FavoritesButton.SaveFavorite("Textures/Now/home");
		FavoritesButton.SaveFavorite("Textures/Now/home entertainment");
		FavoritesButton.SaveFavorite("Textures/Now/home monitor");
		FavoritesButton.SaveFavorite("Textures/Now/imgres-1");
		FavoritesButton.SaveFavorite("Textures/Now/kid");
		FavoritesButton.SaveFavorite("Textures/Now/kid gift ideas");
		FavoritesButton.SaveFavorite("Textures/Now/kids field trip");
		FavoritesButton.SaveFavorite("Textures/Now/lake");
		FavoritesButton.SaveFavorite("Textures/Now/meal ideas");
		FavoritesButton.SaveFavorite("Textures/Now/MIDDLE_HAIR");
		FavoritesButton.SaveFavorite("Textures/Now/model-004");
		FavoritesButton.SaveFavorite("Textures/Now/model-007");	
		FavoritesButton.SaveFavorite("Textures/Now/model-008");
		FavoritesButton.SaveFavorite("Textures/Amazon/Categories/AmaCat01");
		FavoritesButton.SaveFavorite("Textures/Amazon/Categories/AmaCat02");
		FavoritesButton.SaveFavorite("Textures/Amazon/Categories/AmaCat03");
		FavoritesButton.SaveFavorite("Textures/Amazon/Categories/AmaCat04");
		FavoritesButton.SaveFavorite("Textures/Amazon/Categories/AmaCat05");
		FavoritesButton.SaveFavorite("Textures/Amazon/Categories/AmaCat06");



	}

	void OnAnimationComplete()
	{
		GameObject.Find("SidePanels").SendMessage("ShowPanel",CommunityType.Kartua);

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

		if (SceneManager.Instance == null)
			Debug.Log("instance is null");

		_currentScene = SceneManager.Instance.GetScene();

		if (SceneManager.IsInHelixOrBrowse() == false)
			return;

		// get one finger swipe velocity
		if (InputManager.Instance.IsTouchingWithOneFinger() && _currentScene == Scene.Browse && InputManager.Instance.IsDraggingObject() == false)
		{

//			if (InputManager.Instance.HasFingerStoppedMoving())
//				_velocity = Vector3.zero;
//			else

			_velocity = InputManager.Instance.GetOneFingerTotalWorldDelta() * DragSpeed;

		}


		// apply velocity and friction
		float magnitude = LimitVelocity();


		if (magnitude > 0)
		{

			// do helix movement if we are in the helix
			if (Community.CurrentCommunity is SpinningShape)
			{
				_velocity.x = 0;
				_velocity.y = 0;
			}


			// stop right before we go through the hit plane

			// (transform.position + _velocity).z > _hitPlane.position.z)
			 
			HandleCommunityTransitions();

			transform.position += _velocity;


			if (Community.CurrentCommunity is SpinningShape == false)
			LimitPositionToBounds();

				
	//		ApplyBounds();
			ApplyFriction(magnitude);
		}

	}

	void LimitPositionToBounds()
	{
		if (LeanTween.isTweening(gameObject))
			return;

		Vector3 pos = transform.position;


		if (_velocity.z > 0 && transform.position.z > Community.CurrentCommunity.MaxZ)
			pos.z = Community.CurrentCommunity.MaxZ;
		else if (_velocity.z < 0 && transform.position.z < Community.CurrentCommunity.MinZ)
			pos.z = Community.CurrentCommunity.MinZ;
		
		if (_velocity.x > 0 && transform.position.x > Community.CurrentCommunity.MaxX)
			pos.x = Community.CurrentCommunity.MaxX;
		else if (_velocity.x < 0 && transform.position.x < Community.CurrentCommunity.MinX)
			pos.x = Community.CurrentCommunity.MinX;

		if (_velocity.y > 0 && transform.position.y > Community.CurrentCommunity.MaxY)
			pos.y = Community.CurrentCommunity.MaxY;
		else if (_velocity.y < 0 && transform.position.y < Community.CurrentCommunity.MinY)
			pos.y = Community.CurrentCommunity.MinY;



		transform.position = pos;
	}

#region CommunityTransitions

	void HandleCommunityTransitions()
	{
		if (_velocity.z == 0 || LeanTween.isTweening(gameObject))
			return;

		if (_velocity.z > 0) // going forward
		{

			// move to zoomed out view if we are at or behind zoomed in pos
			if (transform.position.z < Community.CurrentCommunity.GetZoomedInPos().z) 
			{
				ZoomToDefaultPos();
			}
			else
				HandleForwardCommmunityTransitions();
		}
		else if (_velocity.z < 0 && LeanTween.isTweening(gameObject) == false) // going back
		{
			_velocity.z = 0;
			// if the camera is further in than the zoomed in pos just zoom out to it
			if (transform.position.z > Community.CurrentCommunity.GetZoomedInPos().z)
			{
				ZoomToDefaultPos ();

				return;
			}

			Vector3 zoomOutPos = Community.CurrentCommunity.GetZoomedOutCameraPos();

			if (transform.position.z <= zoomOutPos.z) // are we already at zoomed out pos
				DoBackTransition();
			else 
				ZoomOut();
		}


	}

	void ZoomToDefaultPos()
	{
		_velocity.z = 0;
		LeanTween.move(gameObject,Community.CurrentCommunity.GetZoomedInPos(),.3f).setOnComplete ( () => {
			_velocity = Vector3.zero;
		});
	}

	void ZoomOut()
	{
//		if (SceneManager.Instance.GetScene() == Scene.Helix)
//			GameObject.Find("Billboard Ad").GetComponent<Animator>().SetTrigger("TriggerAnimation");

		Vector3 zoomOutPos = Community.CurrentCommunity.GetZoomedOutCameraPos();
		LeanTween.move(gameObject,zoomOutPos,.3f).setOnComplete ( () => {
			_velocity = Vector3.zero;
		});
	}
	
	public void DoBackTransition()
	{
		if (Community.CurrentCommunity.BackCommunity == null || LeanTween.isTweening(gameObject))
			return;

		ImageObj backItem = Community.CurrentCommunity.BackCommunityItem;
		Vector3 backMoveToPos = (backItem != null) ? 
			backItem.transform.position + Vector3.back : Community.CurrentCommunity.BackCommunity.transform.position + Vector3.back * 2;

		
		_velocity = Vector3.zero;
		
		StartCoroutine( Community.CurrentCommunity.FadeOutAndRemove() );
		
		Community.CurrentCommunity = Community.CurrentCommunity.BackCommunity;
		
		if (backItem != null) // we just came from another community
			backItem.DoCommunityBackTransition();
		
		Community.CurrentCommunity.FadeIn(1);
		
		LeanTween.move(gameObject,backMoveToPos,1).setOnComplete ( () => {
			if (SceneManager.Instance.GetScene() == Scene.Helix)
				SceneManager.Instance.PushScene(Scene.Browse);


			_velocity = Vector3.zero;
			if (_backOutToKartua && Community.CurrentCommunity.BackCommunity != null)
				DoBackTransition();
			else
			{
				_backOutToKartua = false;
				ZoomToDefaultPos();
			}
		});
		
		Utils.SendMessageToAll("OnCommunityChange");
	}
	

	void HandleForwardCommmunityTransitions()
	{
		if (LeanTween.isTweening(gameObject))
			return;

		Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width/2.0f,Screen.height/2.0f));
		//	Debug.DrawRay(ray.origin,ray.direction*1000);
		RaycastHit hit;
		
		
		if (Physics.Raycast(ray, out hit, .5f, 1 << LayerMask.NameToLayer("ImageObj")))
		{
			ImageObj obj = hit.transform.GetComponent<ImageObj>();
			
			// if we are hitting a community transition into it
			if (obj.CanGoThrough())
			{
				DoForwardTransitionOnObj(obj);
				Debug.Log("starting transition: " + hit.transform.name);
			}

			//	hit.transform.localScale *= .5f;
		}
	}

	public void DoForwardTransitionOnObj(ImageObj obj)
	{
		if (obj.CanGoThrough() == false || LeanTween.isTweening(gameObject))
			return;

		if (obj is PastSearchObj)
		{
			StartCoroutine(StartToHelixThroughObjTransition(obj));
			return;
		}

		_velocity = Vector3.zero;

		Vector3 targetPos = GetForwardTransitionTargetPos(12);

		Community backCommunity = Community.CurrentCommunity;

		Community.CurrentCommunity = obj.DoCommunityForwardTransition(targetPos).GetComponent<Community>();

		Community.CurrentCommunity.BackCommunity = backCommunity;
		Community.CurrentCommunity.BackCommunityItem = obj;

		LeanTween.move(gameObject,targetPos, 1).setOnComplete( () => {
			_velocity = Vector3.zero;
		});

		Community.CurrentCommunity.SetZoomedInCameraPos(targetPos);

		Utils.SendMessageToAll("OnCommunityChange");
	}

	IEnumerator StartToHelixThroughObjTransition(ImageObj obj)
	{
		((PastSearchObj)obj).DoSearch();
		Community.CurrentCommunity.FadeOut(.3f);
		obj.FadeOut(.3f);
		LeanTween.move(gameObject,GetForwardTransitionTargetPos(10), 2).setOnComplete( () => {
			_velocity = Vector3.zero;
		});

		yield return new WaitForSeconds(.3f);

		while (SceneManager.Instance.GetScene() != Scene.Helix)
		{

			yield return new WaitForSeconds(.1f);
			Debug.Log("moving forward");
		}

		DoToHelixTransition(obj);
		yield return null;
	}

	public void DoToHelixTransition(ImageObj obj)
	{
		_velocity = Vector3.zero;

		if (obj == null)
			Debug.Log("obj is null");


		Community.CurrentCommunity.FadeOut(1);

		Community backCommunity = Community.CurrentCommunity;

		Community.CurrentCommunity = HelixManager.Instance;
		Community.CurrentCommunity.Name = ImageSearch.Instance.GetSearch();

		Community.CurrentCommunity.BackCommunity = backCommunity;

		Community.CurrentCommunity.BackCommunityItem = obj;

		Vector3 targetPos = HelixManager.Instance.GetTopObjPos() + Vector3.back*2;

		LeanTween.cancel(gameObject);
		LeanTween.move(gameObject,targetPos, 2).setDelay(1).setOnComplete( () => {
			_velocity = Vector3.zero;
		});

		Community.CurrentCommunity.SetZoomedInCameraPos(targetPos);

		Utils.SendMessageToAll("OnCommunityChange");
	}

	public void OnCommunityButtonTouch()
	{
		Debug.Log("backing out");
		_backOutToKartua = true;
		if (Community.CurrentCommunity.BackCommunity == null)
			ZoomToDefaultPos();
		else
			DoBackTransition();
	}

	public Vector3 GetForwardTransitionTargetPos(float distPastCommunity)
	{

		return new Vector3(transform.position.x, transform.position.y, Community.CurrentCommunity.transform.position.z + distPastCommunity);
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



#endregion
}
