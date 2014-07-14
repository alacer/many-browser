using UnityEngine;
using System.Collections;

public class ImageOverlay : MonoBehaviour {

	void OnTap()
	{

		Debug.Log("tapped overlay");

		SelectionManager.Instance.LeaveSelectedObj();

	}
}
