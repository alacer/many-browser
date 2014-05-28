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
		ImageManager.Instance.Clear();
		_urls.Clear();
		Loader.Instance.Show();
		Debug.Log("game obj: " + gameObject.name);
		GetComponent<UIInput>().label.text = search;

		string returnStr = null;

		Debug.Log("searching for: " + search);

		string query = "http://75.103.15.46:8080/context";

		WWWForm form = new WWWForm();
		WWW www = null;

		form.AddField("keywords",search);
		form.AddField("limit",50);

		bool gotError = false;

		if (PlayerPrefs.HasKey(search))
		{
			returnStr = PlayerPrefs.GetString(search);	
			Debug.Log("got search from cache " + returnStr);
		}
		else
		{
			www = new WWW (query,form);
			 
			float time = 0;
			float timeOut = 6;
			while (timeOut < timeOut && www.isDone == false)
			{
				yield return new WaitForSeconds(.1f);
				time += .1f;
			}

			yield return www;

			gotError = !(www.isDone == true && (www.error == null || www.error == string.Empty));

			Debug.Log("got server string: " + www.text);
		}

		Debug.Log("search complete");

		if (!gotError)
		{
	
			if (returnStr == null)
			{
				returnStr = www.text;
				PlayerPrefs.SetString(search,returnStr);
			}

			var data = JsonConvert.DeserializeObject<List< Dictionary<string, object>>>(returnStr);

			Debug.Log ("data type: " + data.GetType().ToString());

			//	Debug.Log("picture: " + data[0]["picture"].ToString());
			
			foreach(Dictionary<string,object> obj in data)
			{
		 		Debug.Log("type: " + obj.GetType().ToString());

				Dictionary<string,object> objDict = (Dictionary<string,object>)obj;

				foreach (string key in objDict.Keys)
				{
					Debug.Log("key: " + key);
					if (objDict[key] != null)
						Debug.Log(" value: " + objDict[key].GetType().ToString());
				}

				object o = objDict["image"];

				if (objDict["image"] == null)
					continue;

				Debug.Log("obj is null: " + (objDict["image"] == null));

				Debug.Log("obj type: " + o.GetType().ToString());

				Newtonsoft.Json.Linq.JObject imageObj = (Newtonsoft.Json.Linq.JObject)obj["image"];



				Newtonsoft.Json.Linq.JToken token;

				imageObj.TryGetValue("url", out token);

				Debug.Log("has values: " + token.HasValues);




				_urls.Add(  token.ToObject<string>());
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

		Debug.Log("got url count: " + _urls.Count);

		Loader.Instance.Hide();

		GridManager.Initialize(_urls);

		if (SceneManager.Instance.GetScene() == Scene.Default)
			SceneManager.Instance.TransitionToBrowseView();
	}


}
