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
		float numLayers = 3;
		float angleDelta = 360.0f / (numObjs / numLayers);
		float itemsPerLayer = Mathf.Floor( 360.0f / angleDelta );
		float angle = 0;
		float heightDelta = .15f;

		float _lastLayer=0;
		float layer=-1;

		Quaternion rotation;

		
		// Create lists of new positions and rotations
		for (int i=0; i < numObjs; i++)
		{
			string url = _urls[i];

			bool isCommunityItem = (Resources.Load(url) != null);

			_lastLayer = layer;

			dir = Quaternion.AngleAxis(layer*30,Vector3.right) * Vector3.back;

			dir = Quaternion.AngleAxis(angle,Vector3.up) * dir;

			angle += angleDelta;

			if (angle >= 360)
			{
				layer++;
				angle -= 360;
			}
			
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
