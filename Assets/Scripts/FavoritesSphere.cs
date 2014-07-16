using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FavoritesSphere : SpinningShape {

	public GameObject HelixObjPrefab;
	public GameObject CommunityObjPrefab;
	string[] _urls;

	void Start()
	{

		_urls = PlayerPrefsX.GetStringArray("FavoritesUrls");

		Debug.Log("urls length: " + _urls.Length);
		AddFavoriteObjs();
	}

	void AddFavoriteObjs()
	{
		float numObjs = _urls.Length;

		Vector3 center = transform.position;
		Vector3 dir = Vector3.right;

		float radius = GetComponent<SphereCollider>().bounds.size.x / 2.0f;
		float angleDelta = 360.0f / numObjs;
		float angle = 0;
		float heightDelta = .15f;

		Quaternion rotation;

		
		// Create lists of new positions and rotations
		for (int i=0; i < numObjs; i++)
		{
			string url = _urls[i];
			Debug.Log("loading obj url: " + url);
			bool isCommunityItem = (Resources.Load(url) != null);

			dir = Quaternion.AngleAxis(Random.Range(-1,2)*10,Vector3.right) * Vector3.back;

			dir = Quaternion.AngleAxis(angle,Vector3.up) * dir;

			angle += angleDelta;
			
			Vector3 pos = center + (radius * dir);
			rotation = Quaternion.LookRotation(-dir.normalized);


			GameObject prefab = (isCommunityItem) ? CommunityObjPrefab : HelixObjPrefab;
			GameObject obj = (GameObject)Instantiate(prefab,pos,rotation);
			obj.transform.parent = transform;
			obj.transform.forward = (transform.position - pos).normalized;

			Dictionary<string,object> dict = new Dictionary<string, object>();
			dict["Url"] = _urls[i];
			if (PlayerPrefs.HasKey("LargeImage" + url))
				dict["LargeUrl"] = PlayerPrefs.GetString("LargeImage" + url);
			   	
			if (PlayerPrefs.HasKey("Description" + url))
				dict["Description"] = PlayerPrefs.GetString("Description" + url);


			obj.SendMessage("SetTexture",url);
			obj.SendMessage("InitData",dict);

		}


	}
}
