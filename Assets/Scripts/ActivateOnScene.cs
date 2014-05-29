using UnityEngine;
using System.Collections;

public class ActivateOnScene : MonoBehaviour {

	
	public Transform TransformToActivate;
	
	public Scene ActiveScene;
	
	void OnSceneChange(Scene scene)
	{
		TransformToActivate.gameObject.SetActive((scene == ActiveScene));

	}
}
