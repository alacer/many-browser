using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour {

	public float Speed = 10;
	public float SwipeSpeed = .5f;
	public float Friction = 2;

	int _lastTouchCount;
	Vector2 _lastTouchPos;
	float _lastTouchDist;

	Vector3 _forward;
	Vector3 _right;
	Vector3 _moveDir;
	Vector3 _velocity;

	public static InputManager Instance;

	// Use this for initialization
	void Awake () {
		Instance = this;
//		transform.rotation = Quaternion.LookRotation( Quaternion.AngleAxis(-30,Vector3.up) * Vector3.forward);

		_forward = Quaternion.AngleAxis(transform.rotation.y,Vector3.up) * Vector3.forward;
		_right = Quaternion.AngleAxis(transform.rotation.y,Vector3.up) * Vector3.right;

	}

	public Vector3 GetMoveDir()
	{
		Vector3 movDir = Quaternion.AngleAxis(-transform.rotation.y,Vector3.up) *_velocity;

		return movDir;
	}


	
	// Update is called once per frame
	void Update () {

		if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0)
			_velocity =  (_forward * Input.GetAxis("Vertical") + _right * Input.GetAxis("Horizontal")) * Speed;


		if (_lastTouchCount == 1 && GetTouchCount() == 1) // && Input.touches[0].deltaPosition.magnitude > 0)
		{
			Vector3 delta = (GetWorldPos(_lastTouchPos) - GetWorldPos(GetTouchPos()));
			float sign = (delta.x > 0) ? 1 : -1;
			delta = delta.magnitude * _right * sign;
			_velocity = delta * SwipeSpeed;

		}

		float magnitude = _velocity.magnitude;

		if (magnitude > 0)
		{
			transform.position += _velocity * Time.deltaTime;
				
			magnitude -= Friction * Time.deltaTime;

			magnitude = Mathf.Max(0,magnitude);

			_velocity = _velocity.normalized * magnitude;
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
