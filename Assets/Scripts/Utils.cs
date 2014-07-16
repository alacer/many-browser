using UnityEngine;
using System.Collections;

public class Utils : MonoBehaviour{

	public static Utils Instance;

	void Awake()
	{
		Instance = this;

	}

	public void FadeMaterial(Material material, float fadeTime, float alpha)
	{
		StartCoroutine(FadeMaterialRoutine(material,fadeTime,alpha));
	}

	IEnumerator FadeMaterialRoutine(Material material, float fadeTime, float alpha)
	{

		float cycleTime = .05f;
		float timeLeft = fadeTime;
		float alphaChange = alpha - material.color.a;
		float numCycles = fadeTime / cycleTime;
		Color c = material.color;
		
		
		while (timeLeft > 0)
		{
			c.a += alphaChange / numCycles;
			material.color = c;

			yield return new WaitForSeconds(cycleTime);
			timeLeft -= cycleTime;
		}
		
		c.a = alpha;
		material.color = c;

		
	}

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
