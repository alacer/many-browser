﻿using UnityEngine;
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
	GameObject _rightPanel;
	bool _fingerLiftedAfterSwipe = true;
	bool _selected;

	void Start()
	{

		_backPanel = GameObject.Find("BackPanel");
		_leftPanel = GameObject.Find("ActionButtonPanel");
		_rightPanel = GameObject.Find("RightPanel");

		GameObject.Find("ObjectDescriptionLabel").GetComponent<UILabel>().text = transform.parent.GetComponent<ImageObj>().GetData<string>("Description");
	}
	
	// Update is called once per frame
	void Update () {


		Vector3 touchDelta = InputManager.Instance.GetOneFingerTouchDelta();



		if (InputManager.Instance.GetTouchCount() == 0)
		{
	//		Debug.Log("touch count 0");
			_totalAngleChange = Vector3.zero;
			_fingerLiftedAfterSwipe = true;
		}

		if (touchDelta.x != 0 && LeanTween.isTweening(CubeTransform.gameObject) == false)
		{
	
			_totalAngleChange += touchDelta;

//			Debug.Log("total angle: " + _totalAngleChange + " Min angle: " + MinAngleToRotate + "fingerlifted " + _fingerLiftedAfterSwipe);
		}

		if ( Mathf.Abs(_totalAngleChange.x) >  Mathf.Abs(_totalAngleChange.y) &&  Mathf.Abs(_totalAngleChange.x) > MinAngleToRotate && _fingerLiftedAfterSwipe)
		{
			TweenAlpha.Begin(_backPanel,0,0);
			FadeButtonPanel(0,0);

			float dir = (_totalAngleChange.x > 0) ? -1 : 1;

			Debug.Log("rotating");
			float animTime = .3f;

			forward = Quaternion.AngleAxis(-dir * 90,Vector3.up) * forward;


			LeanTween.rotateY(CubeTransform.gameObject,CubeTransform.rotation.eulerAngles.y + (dir * 90),animTime).setOnComplete( () => 
			{

				Debug.Log("forward: " + forward);

//				if (forward == Vector3.back)
//					transform.parent.SendMessage("ScaleToAspect",animTime);
//				else // if (forward == Vector3.forward)
//					transform.parent.SendMessage("ScaleToBox",animTime);
//

				if (forward == Vector3.forward)
					TweenAlpha.Begin(_backPanel,.3f,1);
//				else if (forward == Vector3.left)
//					FadeButtonPanel(1,.3f);

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
		//	Debug.Log("child name: " + 
			TweenAlpha.Begin(_leftPanel.transform.GetChild(i).gameObject,time,alpha);

		}

	}
	

	void OnSelect()
	{	
		GameObject.Find("DraggableDescriptionPanel").SendMessage("ResetPosition");
		_selected = true;
	//	LeanTween.rotate(CubeTransform.gameObject,_startRotation,.3f);
	}

	
	void OnUnselect()
	{

		Debug.Log("cube rototor unselect");
		FadeButtonPanel(0,0);
		TweenAlpha.Begin(_backPanel,0,0);
		TweenAlpha.Begin(_leftPanel,0,0);

		LeanTween.cancel(CubeTransform.gameObject);

		LeanTween.rotateLocal(CubeTransform.gameObject,_startRotation,.3f);
		forward = Vector3.back;

		_selected = false;
	}
}
