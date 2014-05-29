using UnityEngine;
using System.Collections;

public class SortButtons : MonoBehaviour {


	void OnSceneChange(Scene scene)
	{
		bool active = (scene != Scene.Default);
		Utils.ActiveChildren(transform,active);
	}
}
