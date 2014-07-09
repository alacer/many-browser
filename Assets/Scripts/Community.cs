﻿using UnityEngine;
using System.Collections;

public enum CommunityType
{
	Generic = 0,
	Helix,
	Kartua,
	
}

public class Community : MonoBehaviour {

	public string Name;
	public CommunityType Type;
	public bool FadeInOnAwake = true;
 
	public Vector3 HitPlaneOffset;
	public Vector3 RelativeZoomedOutCameraPos = new Vector3(0,0,-5);
	Vector3 _zoomedInPos;

	static Community _currentCommunity;

	public static Community CurrentCommunity
	{
		get { return _currentCommunity; }
		set 
		{ 

			_currentCommunity = value; 
			GameObject hitPlane = GameObject.Find("HitPlane");
			Vector3 pos = hitPlane.transform.position;
			pos.z = _currentCommunity.transform.position.z;
			hitPlane.transform.position = pos + _currentCommunity.HitPlaneOffset;

		}

	}

	Community _backCommunity;
	ImageObj _backCommunityItem;

	public Community BackCommunity {
		get { return _backCommunity; }
		set { 
			_backCommunity = value; 
		}
	}

	public ImageObj BackCommunityItem
	{
		get { return _backCommunityItem; }
		set { _backCommunityItem = value; }

	}

	void Awake()
	{
		if (CurrentCommunity == null)
			CurrentCommunity = this;

		if (FadeInOnAwake)
		{
			for (int i=0; i < transform.childCount; i++)
			{
				ImageObj obj = transform.GetChild(i).GetComponent<ImageObj>();

				if (obj.gameObject.activeSelf == true)
				{
					obj.SetAlpha(0);
					obj.FadeIn(1);
				}
				
			}
		}
	}

	public void SetZoomedInCameraPos(Vector3 zoomedInPos)
	{
		_zoomedInPos = zoomedInPos;
	}

	public Vector3 GetZoomedInPos()
	{
		return _zoomedInPos;
	}

	public void SetZoomedOutYPos(float zoomedOutYPos)
	{
		RelativeZoomedOutCameraPos.y = zoomedOutYPos - transform.position.y;
	}

	public Vector3 GetZoomedOutCameraPos()
	{
		return transform.position + RelativeZoomedOutCameraPos;

	}

	public void FadeOut(float animTime)
	{
		for (int i=0; i < transform.childCount; i++)
		{

			ImageObj obj = transform.GetChild(i).GetComponent<ImageObj>();

			if (obj != null && obj.gameObject.activeSelf == true)
			{
				obj.FadeOut(animTime);
			
			}
		}
	}

	public void FadeIn(float animTime)
	{
		for (int i=0; i < transform.childCount; i++)
		{
			ImageObj obj = transform.GetChild(i).GetComponent<ImageObj>();

			if (obj != null && obj.gameObject.activeSelf == true)
				obj.FadeIn(animTime);
		}
	}

	public IEnumerator FadeOutAndRemove()
	{
		float animTime = 1;
		FadeOut(1);

		yield return new WaitForSeconds(animTime);

		Destroy(gameObject);
	}

	

}
