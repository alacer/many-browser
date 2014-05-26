using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

public class ImageSearch : MonoBehaviour {


	public static ImageSearch Instance;
	MatchCollection _matchCollection;
	List<string> _urls = new List<string>();
	

	void Awake()
	{
		Instance = this;
	}

	
	void OnPress(bool pressed)
	{
		GetComponent<UIInput>().label.text = "";
	}

	public void Search (string search)
	{
		StartCoroutine(SearchRoutine(search));
	}
	

	// Use this for initialization
	IEnumerator SearchRoutine (string search) {

		GetComponent<UIInput>().label.text = search;

		if (SceneManager.Instance.GetScene() == Scene.Default)
			SceneManager.Instance.TransitionToBrowseView();

		Debug.Log("searching for: " + search);

		string query = "http://75.103.15.46:8080/context";

		WWWForm form = new WWWForm();

		form.AddField("keywords",search);
		form.AddField("limit",10);

		WWW www = new WWW (query,form);
		 
		string returnStr = null;

		float time = 0;
		float timeOut = 3;
		while (timeOut < timeOut && www.isDone == false)
		{
			yield return new WaitForSeconds(.1f);
			time += .1f;
		}

//		yield return www;

		Debug.Log("search complete");

		if (www.isDone == true && (www.error == null || www.error == string.Empty))
		{
			Debug.Log("got server string: " + www.text);
			returnStr = www.text;

			var data = JsonConvert.DeserializeObject<List< Dictionary<string, object>>>(returnStr);
			
			//	Debug.Log("picture: " + data[0]["picture"].ToString());
			
			foreach(Dictionary<string,object> obj in data)
			{
				_urls.Add(obj["picture"].ToString());
			}
		}
		else // use Bing if there is an issue with the server
		{
			Debug.Log("issue with server.. trying bing search");
			string backupQuery =  "http://www.bing.com/images/search?&q=" + search + "&qft=+filterui:imagesize-small&FORM=R5IR1#a";
			WWW backupWWW = new WWW (backupQuery);

			yield return backupWWW;
			returnStr = backupWWW.text;

			_matchCollection = Regex.Matches(returnStr, @"imgurl(.*?).jpg");// @"http(.*?).jpg",
			
			// get the urls from our search
			foreach (Match m in _matchCollection)
			{
				string url  = m.Value.Split(new string[] {"quot;"} , System.StringSplitOptions.None)[1];
				
				_urls.Add(url);
			}
		}


		GridManager.Instance.Initialize(_urls);


	}


}
