using UnityEngine;
using System.Collections;

public class SortText : MonoBehaviour {

	Material _textMat;

	void Start()
	{
		_textMat = GetComponent<TextMesh>().renderer.material;
	}
	
	// Update is called once per frame
	void Update () {
		float dot = Vector3.Dot(Camera.main.transform.forward,transform.forward);

		float alpha = Mathf.Clamp01( dot - .8f ); 
		alpha *= 1 / (1.0f - .8f);
		_textMat.color = new Color(_textMat.color.r,_textMat.color.g,_textMat.color.b,alpha);


	}
}
