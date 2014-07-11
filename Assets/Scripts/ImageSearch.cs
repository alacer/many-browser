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
	string _search;
	string _currentKeywordTrail;
	string _lastLabel = string.Empty;

	void Awake()
	{

		Instance = this;
	}

	void Start()
	{
		SceneManager.Instance.PushScene(Scene.Browse);
					
		UpdateKeywordTrail();
	}


	public void OnSubmit()
	{
		string text = NGUIText.StripSymbols(GetComponent<UIInput>().value);



		// remove trail first if needed 
		if (_currentKeywordTrail != null && _currentKeywordTrail != string.Empty && text.Contains(_currentKeywordTrail))
		{
			text = text.Remove(0,_currentKeywordTrail.Length+1);
		}

		Debug.Log("trail: " + _currentKeywordTrail + " search: " + text);

		ImageObj obj = Community.CurrentCommunity.FindObjWithName(text);
		if (obj != null)
		{
			StartCoroutine( SelectionManager.Instance.OnObjectSearch(obj.transform) );
		}
		else
			Search(text);
	}


	void OnCommunityChange()
	{
		UpdateKeywordTrail();

	}

	public void UpdateKeywordTrail()
	{
		Community community = Community.CurrentCommunity;

		List<string> keywords = new List<string>();

		while (community != null)
		{
			keywords.Add(community.Name);
			community = community.BackCommunity;

		}


		string labelText = string.Empty;

		for (int i = keywords.Count-2; i >= 0; i--)
		{
			labelText += "#" + keywords[i] + " ";
		}

		if (labelText == string.Empty)
			labelText = "#NOW";

		_currentKeywordTrail = labelText;
		GetComponent<UIInput>().label.text = labelText;
		GetComponent<UIInput>().value = labelText;
	}

	public void Search (string search)
	{
		Search(search,false);
	}

	public void Search (string search, bool inPastSearch)
	{
		if (search == string.Empty)
			return;

		if (!_searching)
		{
			_searching = true;
			StartCoroutine(SearchRoutine(search,inPastSearch));
		}
	}
	

	// Use this for initialization
	IEnumerator SearchRoutine (string search, bool inPastSearch) {

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
		HelixButton.UnselectAll();
		_search = search;

		LeanTween.alpha(GameObject.Find("Overlay"),0,.3f);
		// clear out previous searches & show loader
		ImageManager.Instance.Clear();
		_dataList.Clear();
		Loader.Instance.Show();
		Debug.Log("game obj: " + gameObject.name);
//		GetComponent<UIInput>().label.text = search;

		string returnStr = null;

		Debug.Log("searching for: " + search);
	
		//	string query = "http://75.103.15.46:8080/context";
		string query = "http://54.83.28.73:8080/context";
		WWWForm form = new WWWForm();
		WWW www = null;

		form.AddField("keywords",search);
	//	form.AddField("limit",50);


		bool gotError = false;

		Debug.Log("searching for: " + search);
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

		if (gotError || (returnStr == null && www.text == string.Empty))
		{

			returnStr = CachedResponse.text;

			Debug.Log("got server error. Using cached search: " + returnStr);
		}
		else if (returnStr == null)
		{
		
			returnStr = www.text;

		}
		Debug.Log("adding to cache: " + search);
		PlayerPrefs.SetString(search,returnStr);

		Debug.Log("1");
		JArray jArray = (JArray)JsonConvert.DeserializeObject(returnStr);
		Debug.Log("2");

		for (int i=0; i < jArray.Count; i++)
		{
			Debug.Log("3");
			Dictionary<string,object> data = new Dictionary<string, object>();

			// image url
			try {
				
				data["Url"] = (string)jArray[i]["body"]["image"]["url"];
				
				
			} catch (System.Exception ex) {
				Debug.LogWarning("could not parse image error: " + ex.Message);
				continue;
			}

			Debug.Log("4");
			// price
			try {
				string str = (string)jArray[i]["body"]["data"]["AWSECommerceService"]["ItemAttributes"][0]["ListPrice"][0]["Amount"][0];
			
				float price = float.Parse(str);
				data["Price"] = price;

			} catch (System.Exception ex) {
				Debug.LogWarning("could not parse price error: " + ex.Message);
				data["Price"] = Random.Range(5.0f,20.0f);
			}

			// Title
			try {
				string str = (string)jArray[i]["body"]["data"]["AWSECommerceService"]["ItemAttributes"][0]["Title"][0];
				

				data["Title"] = str;
				
			} catch (System.Exception ex) {
				Debug.LogWarning("could not parse Title error: " + ex.Message);
				data["Title"] = "";
			}

			// Format
			try {
				string str = (string)jArray[i]["body"]["data"]["AWSECommerceService"]["ItemAttributes"][0]["Format"][0];
				
				
				data["Format"] = str;
				
			} catch (System.Exception ex) {
				Debug.LogWarning("could not parse Format error: " + ex.Message);
				data["Format"] = "";
			}

			// Author
			try {
				string str = (string)jArray[i]["body"]["data"]["AWSECommerceService"]["ItemAttributes"][0]["Author"][0];
				
				
				data["Author"] = str;
				
			} catch (System.Exception ex) {
				Debug.LogWarning("could not parse Author error: " + ex.Message);
				data["Author"] = "";
			}

			Debug.Log("5");
			// Release Data
			try {
				string str = (string)jArray[i]["body"]["data"]["AWSECommerceService"]["ItemAttributes"][0]["ReleaseDate"][0];
				
				
				data["ReleaseDate"] = str;
				
			} catch (System.Exception ex) {
				Debug.LogWarning("could not parse ReleaseDate error: " + ex.Message);
				data["ReleaseDate"] = "";
			}

			try {
				string str = (string)jArray[i]["body"]["data"]["AWSECommerceService"]["ItemAttributes"][0]["PublicationDate"][0];
				
				
				data["PublicationDate"] = str;
				
			} catch (System.Exception ex) {
				Debug.LogWarning("could not parse PublicationDate error: " + ex.Message);
				data["PublicationDate"] = "";
			}

			// availablitity
			try {
				string str = (string)jArray[i]["body"]["data"]["AWSECommerceService"]["Offers"][0]["Offer"][0]["OfferListing"][0]["AvailabilityAttributes"][0]["MinimumHours"][0];
				
				float availability = float.Parse(str) ;
				Debug.Log("setting availability: " + availability);
				data["Availability"] = availability;
				
			} catch (System.Exception ex) {
				Debug.LogWarning("could not parse availability error: " + ex.Message);
				data["Availability"] = (Random.Range(0,2) == 1) ? 24.0f : 48.0f;
			}

			// Popularity
			try {
				
				float popularity = 1 / float.Parse( (string)jArray[i]["body"]["data"]["AWSECommerceService"]["SalesRank"][0] );
				data["Popularity"] = popularity;
				
			} catch (System.Exception ex) {
				Debug.LogWarning("could not parse popularity  error: " + ex.Message);
				data["Popularity"] = Random.Range(.001f,1.0f);
			}

			// Expert Rating
			try {
				
				float expertReview = (float)jArray[i]["body"]["data"]["vpi"]["expertReview"];
				data["ExpertRating"] = expertReview;
				
			} catch (System.Exception ex) {
				Debug.LogWarning("could not parse expert review error: " + ex.Message);
				data["ExpertRating"] = Random.Range(1.0f,5.0f);
			}

			// Buyer Rating
			try {
				
				float customerReview = (float)jArray[i]["body"]["data"]["vpi"]["customerReview"];
				data["BuyerRating"] = customerReview;
				
			} catch (System.Exception ex) {
				Debug.LogWarning("could not parse customer review error: " + ex.Message);
				data["BuyerRating"] = Random.Range(1.0f,5.0f);
			}

			// Description
			try {
				data["Description"] = (string)jArray[i]["body"]["description"];
				
			} catch (System.Exception ex) {
				Debug.LogWarning("could not parse description error: " + ex.Message);
			}

			// Large image url
			try {
				data["LargeUrl"] = jArray[i]["body"]["images"]["primary"]["largeImage"]["url"].ToString();
				
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
		Debug.Log("6");

		Loader.Instance.Hide();

		if (_dataList.Count > 0)
		{
			if (!inPastSearch)
				PastSearches.OnDidSearch(search);
			StartCoroutine( DoSearchTransition(inPastSearch) );
		}
	}

	IEnumerator DoSearchTransition(bool inPastSearch)
	{
		if (!inPastSearch)
		{
			float animTime = SelectionManager.Instance.LeaveSelectedObj();

			yield return new WaitForSeconds (animTime + .5f);
		}

		bool isInHelix = SceneManager.Instance.GetScene() == Scene.Helix;

		HelixManager.Initialize(_dataList);
		TweenAlpha.Begin(GameObject.Find("ErrorLabel"),0,0);
		
		ImageManager.Instance.Initialize(_dataList);

		Debug.Log("in scene: " + SceneManager.Instance.GetScene());

		if (!isInHelix && !inPastSearch)
			CameraManager.Instance.DoToHelixTransition(null);

		_searching = false;
		yield return null;
	}

	public string GetSearch()
	{
		return _search;
	}
}
