using UnityEngine;
using System.Collections;

public class FavoritesSphere : SpinningShape {

	public GameObject FavoriteObjPrefab;
	string[] _urls;

	void Start()
	{

		_urls = PlayerPrefsX.GetStringArray("FavoritesUrls");

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
			dir = Quaternion.AngleAxis(Random.Range(-1,2)*10,Vector3.right) * Vector3.back;

			dir = Quaternion.AngleAxis(angle,Vector3.up) * dir;

			angle += angleDelta;
			
			Vector3 pos = center + (radius * dir);
			rotation = Quaternion.LookRotation(-dir.normalized);
			
			GameObject obj = (GameObject)Instantiate(FavoriteObjPrefab,pos,rotation);
			obj.transform.parent = transform;
			obj.transform.forward = (transform.position - pos).normalized;
		}


	}
}
