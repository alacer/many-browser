using UnityEngine;
using System.Collections;

public class Community : MonoBehaviour {

	public bool FadeInOnAwake = true;

	public static Community CurrentCommunity;

	void Awake()
	{
		CurrentCommunity = this;

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


	public IEnumerator FadeOutAndRemove()
	{
		float animTime = 1;
		for (int i=0; i < transform.childCount; i++)
		{
			ImageObj obj = transform.GetChild(i).GetComponent<ImageObj>();

			obj.FadeOut(animTime);

		}

		yield return new WaitForSeconds(animTime);

		Destroy(gameObject);
	}

	

}
