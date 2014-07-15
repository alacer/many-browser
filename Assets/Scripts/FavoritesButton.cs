using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FavoritesButton : Tappable {

	public void OnTap()
	{
		ImageObj obj = transform.parent.parent.GetComponent<ImageObj>();
		string url =  obj.GetData<string>("Url");
		string largeImageUrl = obj.GetData<string>("LargeUrl");
		bool isAlreadyFavorite = PlayerPrefs.GetInt("IsFavorite",0) == 1;

		// if it's already a favorite just deselect
		if (isAlreadyFavorite)
		{
			PlayerPrefs.SetInt("IsFavorite" + url,1);
			obj.UpdateFavoritesButton();

			List<string> allUrls = new List<string>( PlayerPrefsX.GetStringArray("FavoritesUrls") );

			for (int i=allUrls.Count-1; i >= 0; i--)
			{
				string removeUrl = allUrls[i];

				if (url == removeUrl)
				{
					allUrls.RemoveAt(i);
					break;
				}
			}


			PlayerPrefsX.SetStringArray("FavoritesUrls",allUrls.ToArray());

			return;
		}



		PlayerPrefs.SetInt("IsFavorite" + url,1);

		obj.UpdateFavoritesButton();

		if (largeImageUrl != null && largeImageUrl != string.Empty)
		{
			PlayerPrefs.SetString("LargeImage" + url,largeImageUrl);
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
