using UnityEngine;
using System.Collections;

public class CubeRotator : MonoBehaviour {

	public Transform CubeTransform;
	float _rotationSpeed = 10;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		Vector3 touchDelta = InputManager.Instance.GetOneFingerTouchDelta();

		transform.Rotate(0,touchDelta.x * _rotationSpeed, 0);
	
	}
}
