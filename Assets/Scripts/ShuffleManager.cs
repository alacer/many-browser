using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShuffleManager : MonoBehaviour {

	GameObject[] _centerObjs;

	List<GameObject> _objsToDeal = new List<GameObject>();
	
	float _shuffleDelay = .3f;

	// Use this for initialization
	void Start () {
		_centerObjs = GameObject.FindGameObjectsWithTag("CenterObj");

		foreach(GameObject obj in _centerObjs)
			_objsToDeal.Add(obj);

		StartCoroutine(Deal ());
	}

	IEnumerator Deal()
	{
		int count = _objsToDeal.Count;
		for (int i=0; i < count; i++)
		{
			GameObject obj = _objsToDeal[Random.Range(0,_objsToDeal.Count)];

			obj.SendMessage("Deal");
			_objsToDeal.Remove(obj);

			yield return new WaitForSeconds(_shuffleDelay);
		}
	}
	

}
