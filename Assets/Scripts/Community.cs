using UnityEngine;
using System.Collections;

public class Community : MonoBehaviour {

	public bool FadeInOnAwake = true;

	public static Community BackCommunity;
	public static Community ForwardCommunity;

	void Awake()
	{
		if (ForwardCommunity == null)
			ForwardCommunity = this;

		if (FadeInOnAwake)
		{
			for (int i=0; i < transform.childCount; i++)
			{
				ImageObj obj = transform.GetChild(i).GetComponent<ImageObj>();
				
				obj.SetAlpha(0);
				obj.FadeIn(1);
				
			}
		}

	}

	public void FadeOut(float animTime)
	{
		for (int i=0; i < transform.childCount; i++)
		{
			ImageObj obj = transform.GetChild(i).GetComponent<ImageObj>();
			if (obj != null)
				obj.FadeOut(animTime);
		}
	}

	public void FadeIn(float animTime)
	{
		for (int i=0; i < transform.childCount; i++)
		{
			ImageObj obj = transform.GetChild(i).GetComponent<ImageObj>();

			if (obj != null)
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
