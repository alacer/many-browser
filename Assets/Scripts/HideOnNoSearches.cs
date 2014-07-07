using UnityEngine;
using System.Collections;

public class HideOnNoSearches : MonoBehaviour {

	Vector3 _startPos;
	Vector3 _offScreenPos = Vector3.up * 10000;

	// Use this for initialization
	void Start () {

		_startPos = transform.localPosition;

		if (PastSearches.GetPastSearchesCount() == 0)
			transform.localPosition = _offScreenPos;
		else
		{

			transform.localPosition = _startPos;
			this.enabled = false;
		}
	}
	
	// Update is called once per frame
	void Update () {

		if (PlayerPrefs.HasKey("PastSearch0"))
		{
			transform.localPosition = _startPos;
			this.enabled = false;
		}
	}
}
