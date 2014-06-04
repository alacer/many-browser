﻿using UnityEngine;
using System.Collections;

public class SelectionManager : MonoBehaviour {

	#region Selection

	Transform _selectedObj;
	Transform _previousParent;
	Vector3 _previousPos;
	Vector3 _previousRot;

	public static SelectionManager Instance;

	void Awake()
	{
		Instance = this;
	}

	public bool IsSelected()
	{
		return _selectedObj != null;
	}

	void OnSingleTap(Vector3 screenPos)
	{

		if (_selectedObj != null)
		{
	
			LeaveSelectedObj();
			return;
		}


		Ray ray = Camera.main.ScreenPointToRay(screenPos);
		Debug.DrawRay(ray.origin,ray.direction*1000);
		RaycastHit hit;
		
		
		if (Physics.Raycast(ray, out hit,1000, 1 << LayerMask.NameToLayer("ImageObj")))
		{
			if (SceneManager.IsInHelixOrBrowse())
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
		
		obj.parent = Camera.main.transform;

		obj.SendMessage("OnSelected",SendMessageOptions.DontRequireReceiver);

		Debug.Log("selected obj stop");
	//	_velocity = Vector3.zero;
		LeanTween.cancel(gameObject);
	}
	
	public float LeaveSelectedObj()
	{
		if (_selectedObj == null)
			return 0;

		Debug.Log("leaving selected");
		SceneManager.Instance.OnSceneTransition();

		_selectedObj.SendMessage("OnUnselected",SendMessageOptions.DontRequireReceiver);

		_selectedObj.parent = _previousParent;
		float animTime = .3f;
		LeanTween.move(_selectedObj.gameObject,_previousPos,animTime);
		LeanTween.rotate(_selectedObj.gameObject,_previousRot, animTime).setOnComplete ( () =>
		                                                                                {
			SceneManager.Instance.PopScene();
		});

		_selectedObj = null;
		return animTime;
	}

	void OnSceneChange(Scene scene)
	{
		Debug.Log("in new scene: " + scene.ToString());
		if (scene != Scene.Selected && _selectedObj != null)
		{
			LeaveSelectedObj();
		}

	}
	
	#endregion
}