using UnityEngine;
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
	public GameObject BackgroundLabel;
 
	public Vector3 HitPlaneOffset;
	public Vector3 RelativeZoomedOutCameraPos = new Vector3(0,0,-5);
	Vector3 _zoomedInPos;

	static Community _currentCommunity;

	public static Community CurrentCommunity
	{
		get { return _currentCommunity; }
		set 
		{ 
			if (_currentCommunity != null &&  _currentCommunity.BackgroundLabel != null)
				_currentCommunity.FadeBackgroundLabel(0,1);


			_currentCommunity = value; 
			GameObject hitPlane = GameObject.Find("HitPlane");
			Vector3 pos = hitPlane.transform.position;
			pos.z = _currentCommunity.transform.position.z;
			hitPlane.transform.position = pos + _currentCommunity.HitPlaneOffset;

			if (_currentCommunity.BackgroundLabel != null)
				_currentCommunity.FadeBackgroundLabel(1,1);

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

	public void FadeBackgroundLabel(float alpha, float fadeTime)
	{
		StartCoroutine(FadeBackgroundLabelRoutine( alpha,fadeTime));
	}

	IEnumerator FadeBackgroundLabelRoutine(float alpha, float fadeTime)
	{

		TextMesh mesh = BackgroundLabel.GetComponent<TextMesh>();

		float cycleTime = .05f;
		float timeLeft = fadeTime;
		float alphaChange = alpha - mesh.color.a;
		float numCycles = fadeTime / cycleTime;
		
		while (timeLeft > 0)
		{
			Color c = mesh.color;
			c.a += alphaChange / numCycles;

			mesh.color = c;

			yield return new WaitForSeconds(cycleTime);
			timeLeft -= cycleTime;
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
