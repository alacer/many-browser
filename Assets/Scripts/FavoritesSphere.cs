using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FavoritesSphere : SpinningShape {

	public GameObject HelixObjPrefab;
	public GameObject CommunityObjPrefab;
	string[] _urls;
	float _numLayers = 3;
	List<List<string>> _layers = new List<List<string>>();


	void Start()
	{
		_urls = PlayerPrefsX.GetStringArray("FavoritesUrls");

		Debug.Log("urls length: " + _urls.Length);
		CreateLayers();

		AddFavoriteObjs();


	}

	void CreateLayers()
	{
		List<string> _urlsLeft = new List<string>(_urls);

		float smallestLayerCount = Mathf.Floor((float)_urlsLeft.Count / _numLayers);


		for (int layer=0; layer < _numLayers; layer++)
		{
			Debug.Log("layer: " + layer);

			_layers.Add (new List<string>());

			for (int i=0; i < smallestLayerCount; i++)
			{
				_layers[layer].Add (_urlsLeft[0]);
				_urlsLeft.RemoveAt(0);

			}
		}

		// if there are any items left add them to the middle layer so it looks symetical 
		int count = _urlsLeft.Count-1;
		for (int i=count; i >= 0; i--)
		{
			_layers[1].Add(_urlsLeft[0]);
			_urlsLeft.RemoveAt(0);

		}
	}

	protected override void ClampToBounds()
	{
		// do nothing here
	}

	protected override Vector3 GetVelocity()
	{
		Vector3 vel = base.GetVelocity();
		
		vel.y = 0;

		return vel;
	}

	void AddFavoriteObjs()
	{
		float numObjs = _urls.Length;

		Vector3 center = transform.position;
		Vector3 dir = Vector3.right;

		float radius = GetComponent<SphereCollider>().bounds.size.x / 2.0f;

		float angle = 0;

		Quaternion rotation;

		
		// Create lists of new positions and rotations
		for (int layer=0; layer < _numLayers; layer++)
		{
			float numItemsOnLayer = _layers[layer].Count;
			float angleDelta = 360.0f / numItemsOnLayer;
			angle = 0;

			for (int i=0; i < numItemsOnLayer; i++)
			{
				string url = _layers[layer][i];

				bool isCommunityItem = (Resources.Load(url) != null);

				dir = Quaternion.AngleAxis((layer-1)*30,Vector3.right) * Vector3.back;

				dir = Quaternion.AngleAxis(angle,Vector3.up) * dir;

				angle += angleDelta;

				Vector3 pos = center + (radius * dir);
				rotation = Quaternion.LookRotation(-dir.normalized);


				GameObject prefab = (isCommunityItem) ? CommunityObjPrefab : HelixObjPrefab;
				GameObject obj = (GameObject)Instantiate(prefab,pos,rotation);
				obj.transform.parent = transform;
				obj.transform.forward = (transform.position - pos).normalized;

				Dictionary<string,object> dict = new Dictionary<string, object>();
				dict["Url"] = url;
			

				if (PlayerPrefs.HasKey("LargeImage" + url))
					dict["LargeUrl"] = PlayerPrefs.GetString("LargeImage" + url);
				   	
				if (PlayerPrefs.HasKey("Description" + url))
					dict["Description"] = PlayerPrefs.GetString("Description" + url);


				obj.SendMessage("SetTexture",url);
				obj.SendMessage("InitData",dict);
			}

		}



	}
}
