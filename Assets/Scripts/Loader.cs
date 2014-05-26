using UnityEngine;
using System.Collections;

public class Loader : MonoBehaviour {

	public float RotationSpeed = 90;

	public static Loader Instance;

	// Use this for initialization
	void Awake () {
		GetComponent<UITexture>().enabled = false;
		Instance = this;
	}


	public void Show()
	{
		GetComponent<UITexture>().enabled = true;
	}

	public void Hide()
	{
		GetComponent<UITexture>().enabled = false;
	}

	// Update is called once per frame
	void Update () {

		transform.Rotate(0,0,RotationSpeed * Time.deltaTime);
	
	}
}
