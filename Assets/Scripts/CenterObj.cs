﻿using UnityEngine;
using System.Collections;

public class CenterObj : MonoBehaviour {

	public bool IsClickable;
	
	public float ShakeTime = 5;

	Vector3 _startPos;
	Vector3 _startScale;

	float _animTime = 2;
	float _currentTime;



	// Use this for initialization
	void Awake () {
		_startPos = transform.localPosition;
		_startScale = transform.localScale;

		transform.localScale = Vector3.zero;
		transform.localPosition = Vector3.zero;



	}

	public void Deal()
	{

		LeanTween.scale(gameObject,_startScale,_animTime).setEase(LeanTweenType.easeOutBounce);
		LeanTween.moveLocal(gameObject,_startPos,_animTime).setEase(LeanTweenType.easeOutBounce);
	}

	void Update()
	{
		if (!IsClickable)
			return;

		_currentTime += Time.deltaTime;

		if (_currentTime >= ShakeTime)
		{
			LeanTween.moveX( gameObject, transform.position.x+0.01f, .1f).setRepeat(3).setLoopPingPong();
			_currentTime = 0;
		}


	}

	void OnTap()
	{

		SceneTransition.Instance.TransitionToBrowseView();
	}
	

}
