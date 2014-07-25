using UnityEngine;
using System.Collections;

public class GoIntoButton : Tappable {

	void OnTap()
	{

		ImageObj obj = transform.parent.GetComponent<ImageObj>();

		if (obj == null)
			obj = GetComponent<ImageObj>();

		if (SelectionManager.Instance.GetSelected() != obj || LeanTween.isTweening(obj.gameObject))
		{
			Debug.Log("not selected or obj is tweening");
			return;
		}

		Debug.Log("going in tapped");

		obj.GoInto();

	}
}
