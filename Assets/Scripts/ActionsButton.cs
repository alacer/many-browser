using UnityEngine;
using System.Collections;

public class ActionsButton : MonoBehaviour {

	bool _isClosed = true;

	void OnTap()
	{
		Debug.Log("tapped is close: " + _isClosed);

		if (_isClosed)
		{
			animation.Play("open");
			_isClosed = false;
		}
		else
		{
			animation.Play("close");
			_isClosed = true;
		}
	}

	
}
