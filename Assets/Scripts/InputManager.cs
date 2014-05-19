using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour {

	public float Speed = 10;

	int _lastTouchCount;
	Vector2 _lastTouchPos;
	float _lastTouchDist;

	Vector3 _forward;
	Vector3 _right;
	Vector3 _moveDir;

	public static InputManager Instance;

	// Use this for initialization
	void Awake () {
		Instance = this;
		_forward = transform.right;
		_right = -transform.forward;

		_forward = transform.rotation * _forward;
		_right = transform.rotation * _right;

	}

	public Vector3 GetMoveDir()
	{
		return _moveDir;
	}
	
	// Update is called once per frame
	void Update () {

		_moveDir =  _forward * Input.GetAxis("Vertical") + _right * Input.GetAxis("Horizontal");
		transform.Translate( _moveDir * Time.deltaTime * Speed);


		if (_lastTouchCount == 1 && GetTouchCount() == 1) // && Input.touches[0].deltaPosition.magnitude > 0)
		{
			Vector3 delta = (GetWorldPos(_lastTouchPos) - GetWorldPos(GetTouchPos()));
		

			transform.position += delta;
		}
	

		if (GetTouchCount() == 1)
			_lastTouchPos = GetTouchPos();

		_lastTouchCount = GetTouchCount();
	}

	int GetTouchCount()
	{
		if (Application.isEditor)
			return (Input.GetMouseButton(0)) ? 1 : 0;
		else
			return Input.touchCount;
	}

	Vector2 GetTouchPos()
	{
		if (Application.isEditor)
			return new Vector2(Input.mousePosition.x,Input.mousePosition.y);
		else
			return Input.touches[0].position;
	}

	Vector3 GetWorldPos(Vector2 touchPos)
	{
		return Camera.main.ScreenToWorldPoint(new Vector3(touchPos.x,touchPos.y,10));

	}
}
