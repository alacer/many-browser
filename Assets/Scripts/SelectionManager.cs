using UnityEngine;
using System.Collections;

public class SelectionManager : MonoBehaviour {

	#region Selection

	Transform _selectedObj;
	Transform _previousParent;
	Vector3 _previousPos;
	Vector3 _previousRot;

	GameObject _overlay;
	public static SelectionManager Instance;

	void Awake()
	{
		_overlay = GameObject.Find("Overlay");

		Instance = this;
	}

	public bool IsSelected()
	{
		return _selectedObj != null;
	}

	void OnTwoFingerSpread(float spread)
	{
		if (SceneManager.Instance.GetScene() != Scene.Selected)
			return;

		if (spread < 0)
			LeaveSelectedObj();
		else if (spread > 0)
		{

			StartCoroutine ( DoForwardCommunityTransition() );
		}

	}

	IEnumerator DoForwardCommunityTransition()
	{
		ImageObj obj = _selectedObj.GetComponent<ImageObj>();

		CameraManager.Instance.DoForwardTransitionOnObj(obj);

		_selectedObj.parent = null;


		yield return new WaitForSeconds(2);

		LeaveSelectedObj();

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
		LeanTween.alpha(_overlay,1,.3f);
		SceneManager.Instance.OnSceneTransition(Scene.Selected);
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

	public void FadeOutOverlay()
	{
		LeanTween.alpha(_overlay,0,.3f);
	}

	public float LeaveSelectedObj()
	{
		return LeaveSelectedObj(false);
	}

	public float LeaveSelectedObj(bool inForwardCommunityTransition)
	{
		if (_selectedObj == null)
			return 0;

		float animTime = .3f;

		StartCoroutine(LeaveSelectedRoutine(animTime, inForwardCommunityTransition));

		return animTime;
	}

	IEnumerator LeaveSelectedRoutine(float animTime, bool inForwardCommunityTransition)
	{

		FadeOutOverlay();
		Debug.Log("leaving selected");
		SceneManager.Instance.OnSceneTransition(SceneManager.Instance.GetLastScene());
		
		_selectedObj.SendMessage("OnUnselected",SendMessageOptions.DontRequireReceiver);
		
		_selectedObj.parent = _previousParent;
		
		if (inForwardCommunityTransition)
				yield return new WaitForSeconds(2);

		LeanTween.move(_selectedObj.gameObject,_previousPos,animTime);
		LeanTween.rotate(_selectedObj.gameObject,_previousRot, animTime).setOnComplete ( () =>
		                                                                                {
			SceneManager.Instance.PopScene();
		});

		_selectedObj = null;
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
