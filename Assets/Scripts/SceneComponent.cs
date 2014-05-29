using UnityEngine;
using System.Collections;

public class SceneComponent : MonoBehaviour {

	public string ComponentName;

	public Scene ActiveScene;

	void OnSceneChange(Scene scene)
	{
		if (scene == ActiveScene)
		{
			gameObject.AddComponent("ComponentName");
		}
		else
		{
			if (gameObject.GetComponent("ComponentName") != null)
				Destroy(gameObject.GetComponent("ComponentName"));
		}
	}


}
