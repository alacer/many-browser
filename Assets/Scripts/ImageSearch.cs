using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;



public class ImageSearch : MonoBehaviour {


	public static ImageSearch Instance;
	public TextAsset CachedResponse;
	List<Dictionary<string,object>> _dataList = new List<Dictionary<string, object>>();
	bool _searching;

	void Awake()
	{
		Instance = this;
	}

	void Start()
	{

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

//		if (search.Contains(" "))
//		{
//			string[] words = search.Split(new char[] {' '});
//			search = string.Empty;
//
//			for (int i=0; i < words.Length; i++)
//			{
//				string word = words[i];
//
//				search += (i==0) ? word : word + ",";
//
//			}
//		}


		// clear out previous searches & show loader
		ImageManager.Instance.Clear();
		_dataList.Clear();
		Loader.Instance.Show();
		Debug.Log("game obj: " + gameObject.name);
		GetComponent<UIInput>().label.text = search;

		string returnStr = null;

		Debug.Log("searching for: " + search);

		string query = "http://75.103.15.46:8080/context";

		WWWForm form = new WWWForm();
		WWW www = null;

		form.AddField("keywords",search);
	//	form.AddField("limit",50);


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
			while (time < timeOut && www.isDone == false)
			{
				yield return new WaitForSeconds(.1f);
				time += .1f;
			}

			yield return www;

			gotError = !(www.isDone == true && (www.error == null || www.error == string.Empty));

			Debug.Log("got server string: " + www.text);
		}

		Debug.Log("search complete");

		if (gotError)
		{
			returnStr = CachedResponse.text;
		}

	
		if (returnStr == null)
		{
			returnStr = www.text;
			PlayerPrefs.SetString(search,returnStr);
		}

		JArray jArray = (JArray)JsonConvert.DeserializeObject(returnStr);


		for (int i=0; i < jArray.Count; i++)
		{

			Dictionary<string,object> data = new Dictionary<string, object>();

			try {
				
				data["Url"] = (string)jArray[i]["body"]["image"]["url"];
				
				
			} catch (System.Exception ex) {
				Debug.LogWarning("could not parse image error: " + ex.Message);
				continue;
			}

		
			try {
				string str = (string)jArray[i]["body"]["data"]["AWSECommerceService"]["ItemAttributes"][0]["ListPrice"][0]["Amount"][0];
			
				float price = float.Parse(str) / 100.0f;
				data["Price"] = price;

			} catch (System.Exception ex) {
				Debug.LogWarning("could not parse price error: " + ex.Message);
				data["Price"] = Random.Range(5.0f,20.0f);
			}


			try {

				
				data["LargeUrl"] = (string)jArray[i]["images"][0]["url"];
				
			} catch (System.Exception ex) {
				Debug.LogWarning("could not parse large image error: " + ex.Message);
			}

			_dataList.Add(data);
		}

//		else //  issue with the server
//		{
//			Debug.Log("issue with server.. trying bing search");
//			TweenAlpha.Begin(GameObject.Find("ErrorLabel"),.3f,1);
//
//		}


		Loader.Instance.Hide();

		if (_dataList.Count > 0)
		{
			GridManager.Initialize();
			TweenAlpha.Begin(GameObject.Find("ErrorLabel"),0,0);
		
			ImageManager.Instance.Initialize(_dataList);

			if (SceneManager.Instance.GetScene() == Scene.Default)
				SceneManager.Instance.TransitionToBrowseView();
			else
				SceneManager.Instance.PushScene(Scene.Browse);
			_searching = false;
		}
	}


}
