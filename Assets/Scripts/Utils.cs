using UnityEngine;
using System.Collections;

public class Utils {

	public static void SendMessageToAll(string message, object param)
	{
		GameObject[] allObjs = GameObject.FindObjectsOfType<GameObject>();
		
		foreach (GameObject obj in allObjs)
		{
			obj.SendMessage(message,param, SendMessageOptions.DontRequireReceiver);
			
		}
		
	}
	
	public static void SendMessageToAll(string message)
	{
		GameObject[] allObjs = GameObject.FindObjectsOfType<GameObject>();
		
		foreach (GameObject obj in allObjs)
		{
			obj.SendMessage(message, SendMessageOptions.DontRequireReceiver);
			
		}
		
	}

	public static void ActiveChildren(Transform parent, bool active)
	{
		for (int i=0; i < parent.childCount; i++)
		{
			GameObject child = parent.GetChild(i).gameObject;

			child.SetActive(active);

		}
	}
}
