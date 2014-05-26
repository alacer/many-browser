using UnityEngine;
using System.Collections;

public class OnVisibleDispatcher : MonoBehaviour {

	public GameObject Observer;

	void OnBecameVisible()
	{
		Observer.SendMessage("OnBecameVisible",SendMessageOptions.DontRequireReceiver);
	}
	
	void OnBecameInvisible()
	{
		Observer.SendMessage("OnBecameInvisible",SendMessageOptions.DontRequireReceiver);
	}
}
