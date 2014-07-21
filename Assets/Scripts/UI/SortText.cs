using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SortText : MonoBehaviour {

	Material _textMat;
	TextMesh _textMesh;

	void Awake()
	{
		_textMat = GetComponent<TextMesh>().renderer.material;
		_textMesh = GetComponent<TextMesh>();
//		GetComponent<TextMesh>().text = "";
	}

	public void SetText(string key, Dictionary<string,object> data)
	{
	
			if (key == "Price")
			{


				_textMesh.text = GetPrice((float)data[key]);
	
			}
			else if (key == "Popularity")
			{
				//			float val = (float)_data[key];
				//			string text = ((int)(1.0f/val)).ToString();
				_textMesh.text = "";
			}
			else if (key == "ExpertRating" || key == "BuyerRating")
			{
				float val = (float)data[key];

				
				_textMesh.text = GetStarRating(val);
			}
			else if (key == "Availability")
			{
				_textMesh.text = "";//((int)(float)data[key]).ToString();
			}
	}

	public static string GetPrice(float val)
	{
		decimal d = new decimal(val);
		string text = "" +  decimal.Round(d,2).ToString ();
		
		return text;
	}

	public static string GetStarRating(float val)
	{

		string stars = "";
		
		for (int i=0; i < (int)val; i++)
			stars = stars + "";
		
		float last = val - Mathf.Floor(val);
		//		Debug.Log("last: " + last);
		if (last > .33f)
			stars = stars + "";
		
		return stars;

	}

	// Update is called once per frame
	void Update () {
		float dot = Vector3.Dot(Camera.main.transform.forward,transform.forward);

		float alpha = Mathf.Clamp01( dot - .8f ); 
		alpha *= 1 / (1.0f - .8f);
		_textMat.color = new Color(_textMat.color.r,_textMat.color.g,_textMat.color.b,alpha);


	}
}
