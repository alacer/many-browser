using UnityEngine;
using System.Collections;

public class RestaurantPeople : MonoBehaviour {

	public Transform TopLeftObj;
	public Transform BottomLeftObj;

	public GameObject[] Children;

	public bool IsEnabled = false;

	// Use this for initialization
	public void Enable () {


		for (int i=0; i < Children.Length; i++)
		{
			GameObject child = Children[i];
			child.SetActive(true);
			
		}

		StartCoroutine(MoveToPosition());
		IsEnabled = true;
	}

	public void Disable()
	{
		for (int i=0; i < Children.Length; i++)
		{
			GameObject child = Children[i];
			child.GetComponent<UITexture>().alpha = 0;
			child.SetActive(false);

		}
		IsEnabled = false;
	}


	IEnumerator MoveToPosition()
	{
		for (int i=0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);


			Vector3 startPos = child.localPosition;
			child.localScale = Vector3.zero;
			child.localPosition = TopLeftObj.localPosition;

			child.GetComponent<UITexture>().alpha = 1;
		//	Utils.Instance.FadeMaterial(child.renderer.material,.1f,1);


			
			TweenPosition.Begin(child.gameObject,.5f,startPos);
			TweenScale.Begin(child.gameObject,.5f,Vector3.one);


			yield return new WaitForSeconds(.1f);

			
		}

	}

	public void OnPersonTap()
	{
		GameObject.Find("ImageObj_Restaurant").transform.FindChild("CubeRotator").SendMessage("OnPersonTap");
		Shrink(.5f, BottomLeftObj.gameObject);

		StartCoroutine(OnPersonTapRoutine(.5f));



	}

	IEnumerator OnPersonTapRoutine(float animTime)
	{

		Vector3 startPos = BottomLeftObj.transform.localPosition;

		TweenPosition.Begin(BottomLeftObj.gameObject,animTime,BottomLeftObj.transform.localPosition + Vector3.up * 100);


		yield return new WaitForSeconds(animTime + (animTime/2.0f));

		TweenRotation.Begin(BottomLeftObj.gameObject,animTime/2.0f,Quaternion.Euler(0,0,0));
		TweenScale.Begin(BottomLeftObj.gameObject,animTime,Vector3.zero);
		TweenPosition.Begin(BottomLeftObj.gameObject,animTime,new Vector3(100,100));

		yield return new WaitForSeconds(animTime);

		BottomLeftObj.transform.localPosition = startPos;
		BottomLeftObj.transform.localRotation = Quaternion.Euler(0,0,0);

		Disable();
	}

	

	public void Shrink(float animTime, GameObject exception)
	{
		for (int i=0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			
			if (child.gameObject != exception)
				TweenScale.Begin(child.gameObject,animTime,Vector3.zero);
			
		}
		
	}
	

}
