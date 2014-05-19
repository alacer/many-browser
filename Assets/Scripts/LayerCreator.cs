using UnityEngine;
using System.Collections;

public class LayerCreator : MonoBehaviour {

	public GameObject LayerPrefab;
	public float CreationTime = 3;

	float _currentTime;
	int numLayers = 0;

	// Use this for initialization
	void Start () {
		Application.targetFrameRate = 60;
	}
	
	// Update is called once per frame
	void Update () {

		_currentTime += Time.deltaTime;

		if (_currentTime >= CreationTime)
		{
			Instantiate(LayerPrefab,new Vector3(0,0,numLayers * 2),Quaternion.identity);
			numLayers++;
			_currentTime = 0;
		}
	
	}
}
