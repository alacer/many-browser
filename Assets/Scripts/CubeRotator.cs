using UnityEngine;
using System.Collections;

public class CubeRotator : MonoBehaviour {

	public Transform CubeTransform;
	public float _rotationSpeed = 1;
	public float MinAngleToRotate = 5;
	Vector3 forward = Vector3.back;

	Vector3 _totalAngleChange;
	Vector3 _startRotation = new Vector3(0,270,0);
	float _lastSpeed;
	GameObject _backPanel;
	GameObject _leftPanel;


	bool _fingerLiftedAfterSwipe = true;


	void Start()
	{

		UpdateActionsScale();
		_backPanel = GameObject.Find("BackPanel");
		_leftPanel = GameObject.Find("ActionButtonPanel");


		ImageObj obj = transform.parent.GetComponent<ImageObj>();
		if (obj.HasData())
			GameObject.Find("ObjectDescriptionLabel").GetComponent<UILabel>().text = obj.GetData<string>("Description");
	}

	public void DoFastRotateToFront()
	{
		TweenAlpha.Begin(_backPanel,0,0);
		LeanTween.rotateY(CubeTransform.gameObject,-90,.1f);
		
		_totalAngleChange = Vector3.zero;
	}

	void UpdateColliders()
	{
		Debug.Log("forward: " + forward);

		if (CubeTransform.collider != null)
			CubeTransform.collider.enabled = (forward == Vector3.back);


		if (CubeTransform.FindChild("box_actions") != null && CubeTransform.FindChild("box_actions").collider != null)
		{
			Transform actions = CubeTransform.FindChild("box_actions");

			bool enabled = (forward == Vector3.left);

			actions.collider.enabled = enabled;

			if (enabled)
				UpdateActionsScale();

		}

		if (CubeTransform.FindChild("FavoritesButton(Clone)") != null)
		{
			CubeTransform.FindChild("FavoritesButton(Clone)").collider.enabled = (forward == Vector3.left);
			
		}
	}

	void UpdateActionsScale()
	{
		Transform actions = CubeTransform.FindChild("box_actions");

		if (actions == null)
			return;
		
		Vector3 scale = actions.lossyScale;
		actions.parent = null;
		actions.localScale = scale;
		actions.parent = CubeTransform;
	}

	// Update is called once per frame
	void Update () {

		Vector3 touchDelta = InputManager.Instance.GetOneFingerTouchDelta();


		if (InputManager.Instance.GetTouchCount() == 0)
		{
			_totalAngleChange = Vector3.zero;
			_fingerLiftedAfterSwipe = true;
		}

		if (touchDelta.x != 0 && LeanTween.isTweening(CubeTransform.gameObject) == false)
		{
			_totalAngleChange += touchDelta;
		}

		// did the just swipe to rotate the box?
		if ( Mathf.Abs(_totalAngleChange.x) >  Mathf.Abs(_totalAngleChange.y) &&  Mathf.Abs(_totalAngleChange.x) > MinAngleToRotate && _fingerLiftedAfterSwipe)
		{
			TweenAlpha.Begin(_backPanel,0,0);
			FadeButtonPanel(0,0);

			float dir = (_totalAngleChange.x > 0) ? -1 : 1;

			Debug.Log("rotating");
			float animTime = .3f;


			forward = Quaternion.AngleAxis(-dir * 90,Vector3.up) * forward;

			// do the rotation
			LeanTween.rotateY(CubeTransform.gameObject,CubeTransform.rotation.eulerAngles.y + (dir * 90),animTime).setOnComplete( () => 
			{

				if (forward == Vector3.forward && transform.parent.GetComponent<ImageObj>().IsCommunityItem == false)
					TweenAlpha.Begin(_backPanel,.3f,1);

				UpdateColliders();
	



				_fingerLiftedAfterSwipe = false;
				_totalAngleChange = Vector3.zero;
			});


			_fingerLiftedAfterSwipe = false;
			_totalAngleChange = Vector3.zero;
		}
	
	}

	void FadeButtonPanel(float alpha, float time)
	{
		for (int i=0; i < _leftPanel.transform.childCount; i++)
		{
			TweenAlpha.Begin(_leftPanel.transform.GetChild(i).gameObject,time,alpha);

		}

	}
	

	void OnSelect()
	{	
		forward = Vector3.back;
		UpdateColliders();

		GameObject.Find("DraggableDescriptionPanel").SendMessage("ResetPosition");

	}

	
	void OnUnselect()
	{

		FadeButtonPanel(0,0);
		TweenAlpha.Begin(_backPanel,0,0);
		TweenAlpha.Begin(_leftPanel,0,0);

		LeanTween.cancel(CubeTransform.gameObject);

		LeanTween.rotateLocal(CubeTransform.gameObject,_startRotation,.3f);
		forward = Vector3.back;

	}
}
