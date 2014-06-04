using UnityEngine;
using System.Collections;

public class SortButtons : MonoBehaviour {

	void Start()
	{
		Utils.ActiveChildren(transform,false);
	}

	void Show()
	{
		Utils.ActiveChildren(transform,true);
	}

	void OnSceneChange(Scene scene)
	{
		bool active = (scene != Scene.Default);
		Utils.ActiveChildren(transform,active);

		if (scene == Scene.Default)
		{
			HelixButton.UnselectAll();
		}
	}
}
