using UnityEngine;
using System.Collections;

public class Community : MonoBehaviour {

	public bool FadeInOnAwake = true;

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
			hitPlane.transform.position = pos;
		}

	}

	Community _backCommunity;
	ImageObj _backCommunityItem;

	public Community BackCommunity {
		get { return _backCommunity; }
		set { _backCommunity = value; }
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
