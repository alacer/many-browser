using UnityEngine;
using System.Collections;

public class HelixButton : MonoBehaviour {

	// Use this for initialization
	void OnTap()
	{
		GridManager.Instance.FormHelix();

	}

	void SortByPrice()
	{
		GridManager.Instance.SortByPrice();
	}
}
