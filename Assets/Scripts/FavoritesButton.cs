using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FavoritesButton : Tappable {

	public void OnTap()
	{

		string url =  transform.parent.parent.GetComponent<ImageObj>().GetData<string>("Url");
		Debug.Log("got tapped " + url);

		string[] urls =  PlayerPrefsX.GetStringArray("FavoritesUrls");
		if (urls == null || urls.Length == 0)
			urls = new string[] { url };
		else
		{
			List<string> urlList = new List<string>(urls);

			if (urlList.Contains(url) == false)
			{
				urlList.Add(url);
				PlayerPrefsX.SetStringArray("FavoritesUrls",urlList.ToArray());
			}

		}


	}
}
