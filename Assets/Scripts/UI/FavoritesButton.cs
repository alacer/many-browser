using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FavoritesButton : Tappable {


	public static void SaveFavorite(string url)
	{

		PlayerPrefs.SetInt("IsFavorite" + url,1);

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


	public void OnTap()
	{
		ImageObj obj = transform.parent.parent.GetComponent<ImageObj>();
		string url=null;
		string largeImageUrl=null;

		if (obj.IsCommunityItem)
		{
			// if we are in the favorites sphere already then the url was already set
			if (obj.HasData("Url"))
			{
				url = obj.GetData<string>("Url");
			}
			else // otherwise compose it
			{
				url = obj.GetResourceUrl(); 
			}
		}
		else
		{
			url =  obj.GetData<string>("Url");
			largeImageUrl = obj.GetData<string>("LargeUrl");
		}


		bool isAlreadyFavorite = PlayerPrefs.GetInt("IsFavorite" + url,0) == 1;

		Debug.Log("is already favorite: " + isAlreadyFavorite);

		// if it's already a favorite just deselect
		if (isAlreadyFavorite)
		{
			PlayerPrefs.SetInt("IsFavorite" + url,0);
			obj.UpdateFavoritesButton(url);

			List<string> allUrls = new List<string>( PlayerPrefsX.GetStringArray("FavoritesUrls") );
			allUrls.Remove(url);

			Debug.Log("removing url: " + url);


			PlayerPrefsX.SetStringArray("FavoritesUrls",allUrls.ToArray());

			return;
		}

		Debug.Log("saving favorite: " + url);

		SaveFavorite(url);

		PlayerPrefs.Save();

		obj.UpdateFavoritesButton(url);
		
		if (largeImageUrl != null && largeImageUrl != string.Empty)
		{
			PlayerPrefs.SetString("LargeImage" + url,largeImageUrl);
		}

		if (obj.HasData("Description"))
			PlayerPrefs.SetString("Description" + url,obj.GetData<string>("Description"));

	}
}
