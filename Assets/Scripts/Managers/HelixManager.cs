using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SortOrder
{
	Asending,
	Desending
}

public class HelixManager : SpinningShape {

	public GameObject imageObjPrefab;

	public float HelixRadius = 3f;
	public float SpeedMultiplier = 2;


	Vector3 _topObjPos;


	List<ImageObj> _allObjs = new List<ImageObj>();

	
	public static HelixManager Instance;

	void Awake()
	{
		Instance = this;

		GameObject.Find("HelixHitCylinder").transform.localScale = new Vector3(HelixRadius*2,5000,HelixRadius*2);

		transform.parent = GameObject.Find("UIRoot").transform;


	}

	void OnDestroy()
	{
		Instance = null;
	}

	
	public static void Initialize(List< Dictionary<string,object> > objDataList)
	{
		// destroy current images
		if (Instance != null)
		{
			Instance.Clear();
	//		DestroyImmediate(Instance.gameObject);
		}
		else 
			Instance = ((GameObject)Instantiate(Resources.Load("HelixManager"))).GetComponent<HelixManager>();

		Instance.CreateImageObjs(objDataList);
		Instance.FormHelix("Price",SortOrder.Desending);

	//	SetAllVisible(false);
	}

	void CreateImageObjs(List< Dictionary<string,object> > objDataList)
	{
		Debug.Log("creating images: " + objDataList.Count);
		foreach (Dictionary<string,object> data in objDataList)
		{
			GameObject imageObj = (GameObject) Instantiate (imageObjPrefab);
			imageObj.transform.parent = Instance.transform;
			imageObj.SendMessage("InitData",data);
			_allObjs.Add(imageObj.GetComponent<ImageObj>());
		}

	}

	public void Clear()
	{
		_allObjs.Clear();
		ImageObj[] allObjs = transform.GetComponentsInChildren<ImageObj>();
		for (int i = allObjs.Length-1; i >= 0; i--)
			DestroyImmediate(allObjs[i].gameObject);
	}

	public void FormHelix(string sort, SortOrder order)
	{
		SortObjsBy(sort,order);
		
		Debug.Log("forming helix");
		
		StartCoroutine(FormHelixRoutine(sort));
	}
	
	
	IEnumerator FormHelixRoutine(string sort)
	{

		
		Debug.Log("in form helix");
		while (SceneManager.Instance.GetScene() == Scene.InTransition)
		{
			yield return new WaitForSeconds(.1f);
		}

		bool wasAlreadyInHelix = SceneManager.Instance.GetScene() == Scene.Helix;
		
		if (!wasAlreadyInHelix)
			transform.position = CameraManager.Instance.GetForwardTransitionTargetPos(10) + Vector3.forward * 5;
		
		_targetPos = transform.position;
		_targetRotation = transform.rotation;



		if (SelectionManager.Instance.IsSelected())
		{
			float time = SelectionManager.Instance.LeaveSelectedObj();
			yield return new WaitForSeconds(time);
		}
		
		Debug.Log("all objs: " + _allObjs.Count);
		SceneManager.Instance.OnSceneTransition(Scene.Helix);
		
		//		List<Vector3> positions = new List<Vector3>();
		//		List<Vector3> rotations = new List<Vector3>();
		
		
		
		Vector3 center = transform.position;
		Vector3 dir = Vector3.right;
		
		float angleDelta = 25;
		float angle = 0;
		float heightDelta = .15f;
		
		float animateToHelixTime = 1;
		Vector3 rotation = Vector3.zero;
		
		List<Vector3> positions = new List<Vector3>();
		List<Vector3> rotations = new List<Vector3>();
		
		// Create lists of new positions and rotations
		for (int i=0; i < _allObjs.Count; i++)
		{
			dir = Quaternion.AngleAxis(angle,Vector3.up) * Vector3.back;
			
			center += heightDelta * Vector3.down;
			angle += angleDelta;
			
			Vector3 pos = center + (HelixRadius * dir);
			rotation = Quaternion.LookRotation(-dir.normalized).eulerAngles;
			
			
			if (!wasAlreadyInHelix)
			{
				if (i == _allObjs.Count-1)
				{
					_maxY = _minY + (_minY - pos.y);
					
				}
				
				if (i == 0)
				{
					_topObjPos = pos;
					
					_minY = pos.y;
					
				}
			}
			
			if (wasAlreadyInHelix)
			{
				positions.Add(pos);
				rotations.Add(rotation);
				Community.CurrentCommunity.Name = ImageSearch.Instance.GetSearch();
				Utils.SendMessageToAll("OnCommunityChange");
			}
			else
			{
				_allObjs[i].transform.position = pos;
				_allObjs[i].transform.rotation = Quaternion.Euler(rotation);
			}
			
			_allObjs[i].SetText(sort);
			
			
			//			positions.Add(pos);
			//			rotations.Add(rotation);
		}
		
		if (!wasAlreadyInHelix)
		{
			float moveUpDist = 20;
			transform.position += Vector3.down * moveUpDist;
			
			LeanTween.move(gameObject,transform.position + Vector3.up * moveUpDist,2);
		}
		// now animate all to new positions and rotations
		
		for (int i=0; i < positions.Count; i++)
		{
			
			GameObject obj = _allObjs[i].gameObject;
			LeanTween.cancel(obj);
			
			Vector3 newPos = positions[i];
			Vector3 newRotation = rotations[i];

			LeanTween.move(obj,newPos,animateToHelixTime).setEase(LeanTweenType.easeOutExpo);
			LeanTween.rotate(obj,newRotation,animateToHelixTime).setEase(LeanTweenType.easeOutExpo);
			

		}

		yield return new WaitForSeconds (animateToHelixTime);
		SceneManager.Instance.PushScene(Scene.Helix);

		SetZoomOutPos();

		yield return null;

	}



	public Vector3 GetTopObjPos()
	{
		return _topObjPos;
	}

	

	public void SortObjsBy(string sort, SortOrder order)
	{
		Debug.Log("sorting by: " + sort);

		for (int i=0; i < _allObjs.Count; i++)
		{

			for (int j=0; j <_allObjs.Count-1; j++)
			{
				if (order == SortOrder.Desending)
				{
					if (_allObjs[j].GetData<float>(sort) > _allObjs[j+1].GetData<float>(sort))
					{
						SwapObjs(j,j+1);
					}
				}
				else
				{
					if (_allObjs[j].GetData<float>(sort) < _allObjs[j+1].GetData<float>(sort))
					{
						SwapObjs(j,j+1);
					}
				}
				
			}
			
		}

	}
	
	void SwapObjs(int firstIndex, int secondIndex)
	{
		ImageObj temp = _allObjs[secondIndex];
		
		_allObjs[secondIndex] = _allObjs[firstIndex];
		_allObjs[firstIndex] = temp;
		
	}

	public List<ImageObj> GetAllObjs()
	{
		return _allObjs;
	}
	
	public void SetAllVisible(bool visible)
	{
		for (int i=0; i < transform.childCount; i++)
		{
			transform.GetChild(i).GetComponent<ImageObj>().SetVisible(visible);
		}
	}

}