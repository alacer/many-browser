using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public class ImageSearch : MonoBehaviour {


	public static ImageSearch Instance;
	MatchCollection _matchCollection;
	List<string> _urls = new List<string>();
	bool _searching;

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
		if (!_searching)
		{
			_searching = true;
			StartCoroutine(SearchRoutine(search));
		}
	}
	

	// Use this for initialization
	IEnumerator SearchRoutine (string search) {

		if (search.Contains(" "))
		{
			string[] words = search.Split(new char[] {' '});
			search = string.Empty;

			for (int i=0; i < words.Length; i++)
			{
				string word = words[i];

				search += (i==0) ? word : word + ",";

			}
		}


		// clear out previous searches & show loader
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

		Dictionary<string,string> largeImageDict = new Dictionary<string, string>();

		bool gotError = false;

		// if it's in the cache just use the cache
		if (PlayerPrefs.HasKey(search))
		{
			returnStr = PlayerPrefs.GetString(search);	
			Debug.Log("got search from cache " + returnStr);
		}
		else // otherwise perform the request
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

		if (!gotError) // no error so parse the request
		{
	
			if (returnStr == null)
			{
				returnStr = www.text;
				PlayerPrefs.SetString(search,returnStr);
			}

			//var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Arrays };
			var data = JsonConvert.DeserializeObject<List< Dictionary<string, object>>>(returnStr);

	//		dynamic dyn = JsonConvert.DeserializeObject(returnStr);

//			Debug.Log("label: " + dyn[0].label);

			foreach(Dictionary<string,object> obj in data)
			{
			
				Dictionary<string,object> objDict = (Dictionary<string,object>)obj;

//				foreach (string key in objDict.Keys)
//				{
//					Debug.Log("key: " + key);
//					if (objDict[key] != null)
//						Debug.Log(" value: " + objDict[key].GetType().ToString());
//				}

//				Dictionary<string,object> dataObj = (Dictionary<string,object>)obj["data"];

//				Dictionary<string,object> AWSObj = (Dictionary<string,object>)obj["AWSECommerceService"];


		//		Debug.Log("price: " + objDict["data"]["ListPrice"]["Amount"]);

				if (objDict["image"] == null)
					continue;

				Newtonsoft.Json.Linq.JObject imageObj = (Newtonsoft.Json.Linq.JObject)obj["image"];



				Newtonsoft.Json.Linq.JToken token;


				if (imageObj.TryGetValue("url", out token))
				{
					string url = token.ToObject<string>();
					_urls.Add( url );

					Newtonsoft.Json.Linq.JArray largeImageArray = (Newtonsoft.Json.Linq.JArray)objDict["images"];
					if (largeImageArray != null && largeImageArray.Count > 0)
					{
						string largeUrl = largeImageArray[0]["url"].ToString();

						if (largeUrl != string.Empty && largeUrl != null)
							largeImageDict[url] = largeUrl;

					}
				}
			}
		}
		else // use Bing if there is an issue with the server
		{
			Debug.Log("issue with server.. trying bing search");
			TweenAlpha.Begin(GameObject.Find("ErrorLabel"),.3f,1);

//			string backupQuery =  "http://www.bing.com/images/search?&q=" + search + "&qft=+filterui:imagesize-small&FORM=R5IR1#a";
//			WWW backupWWW = new WWW (backupQuery);
//
//			yield return backupWWW;
//			returnStr = backupWWW.text;
//
//			_matchCollection = Regex.Matches(returnStr, @"imgurl(.*?).jpg");// @"http(.*?).jpg",
//			
//			// get the urls from our search
//			foreach (Match m in _matchCollection)
//			{
//				string url  = m.Value.Split(new string[] {"quot;"} , System.StringSplitOptions.None)[1];
//				
//				_urls.Add(url);
//			}
		}

		Debug.Log("got url count: " + _urls.Count);

		Loader.Instance.Hide();

		if (_urls.Count > 0)
		{
			GridManager.Initialize(_urls);
			TweenAlpha.Begin(GameObject.Find("ErrorLabel"),0,0);
			ImageManager.Instance.Initialize(_urls,largeImageDict);

			if (SceneManager.Instance.GetScene() == Scene.Default)
				SceneManager.Instance.TransitionToBrowseView();
			else
				SceneManager.Instance.PushScene(Scene.Browse);
			_searching = false;
		}
	}


}
