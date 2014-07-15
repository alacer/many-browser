using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FavoritesButton : Tappable {

	public void OnTap()
	{

		string url =  transform.parent.parent.GetComponent<ImageObj>().GetData<string>("Url");
		string largeImageUrl = transform.parent.parent.GetComponent<ImageObj>().GetData<string>("LargeUrl");
		Debug.Log("got tapped " + url);

		if (largeImageUrl != null && largeImageUrl != string.Empty)
		{
			PlayerPrefs.SetString(url,largeImageUrl);
		}

		string[] urls =  PlayerPrefsX.GetStringArray("FavoritesUrls");
		if (urls == null || urls.Length == 0)
		{
			urls = new string[] { url };
			PlayerPrefsX.SetStringArray("FavoritesUrls",urls);
		}
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
