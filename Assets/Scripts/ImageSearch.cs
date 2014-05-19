using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ImageSearch : MonoBehaviour {




	public static ImageSearch Instance;
	MatchCollection _matchCollection;
	List<string> _urls = new List<string>();

	// Use this for initialization
	IEnumerator Start () {

		Instance = this;

		string search = "shoe";
		string query =  "http://www.bing.com/images/search?&q=" + search + "&qft=+filterui:imagesize-small&FORM=R5IR1#a";
	//		string query = "http://www.bing.com/images/search?q=" + search + "&go=Submit&qs=n&form=QBIR&pq=" + search + "s&sc=8-1&sp=-1&sk=";

		WWW www = new WWW (query);
		 
		string returnStr;

		yield return www;

		returnStr = www.text;

		Debug.Log ("return string: " + returnStr);
	
		_matchCollection = Regex.Matches(returnStr, @"imgurl(.*?).jpg");// @"http(.*?).jpg",

		// get the urls from our search
		foreach (Match m in _matchCollection)
		{
			string url  = m.Value.Split(new string[] {"quot;"} , System.StringSplitOptions.None)[1];

			_urls.Add(url);
		}


		GridManager.Instance.Initialize(_urls);



	}


}
