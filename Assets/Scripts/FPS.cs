using UnityEngine;
using System.Collections;

public class FPS : MonoBehaviour {

	int frames;
	float time = 1;
	

	// Update is called once per frame
	void Update () {
	
		time -= Time.deltaTime;

		if (time <= 0)
		{
		
		//	int numObjs = GameObject.FindObjectsOfType<GameObject>().Length;

			GetComponent<GUIText>().text = "fps: " + frames.ToString();// + " objs: " + numObjs;


			time = 1;
			frames = 0;

		}
		frames++;
	}
}
