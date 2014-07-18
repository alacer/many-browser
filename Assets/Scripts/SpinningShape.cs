using UnityEngine;
using System.Collections;

public class SpinningShape : Community {

	public float Friction = 0.003f;
	public float MaxAngularSpeed = 2;
	
	protected float _maxY;
	protected float _minY;
	
	protected Vector3 _velocity;
	protected Vector3 _lastWorldPos;
	
	protected Vector3 _targetPos;
	protected Quaternion _targetRotation;


	protected void FixedUpdate()
	{
		if (Community.CurrentCommunity is SpinningShape == false) // SceneManager.Instance.GetScene() != Scene.Helix)
			return;
		
		if (InputManager.Instance.IsTouchingWithOneFinger())
		{

			_velocity =  GetVelocity();

		}
		
		// apply velocity and friction
		float magnitude = LimitVelocity();
		
		
		if (magnitude > 0)
		{
			transform.position += new Vector3(0,_velocity.y,0);
			if (_velocity.y != 0)
				SetZoomOutPos();
			
			transform.RotateAround(transform.position,Vector3.up,_velocity.x);


			// clamp bounds
			if (transform.position.y > GetMaxY())
				transform.position = new Vector3(transform.position.x, GetMaxY(), transform.position.z);
			else if (transform.position.y < GetMinY())
				transform.position = new Vector3(transform.position.x, GetMinY(), transform.position.z);
			
		}
		
		
		ApplyFriction(magnitude);
	}

//	protected virtual float GetAngleDelta()
//	{
//		
//	}
//
//	protected virtual Vector3 GetRotationsAxis()
//	{
//		return Vector3.up;
//
//	}


	float GetHorizontalAngleDelta(Vector3 worldPos)
	{
		Vector3 lastPos = _lastWorldPos - transform.position;
		Vector3 currentPos = worldPos - transform.position;
		
		lastPos.y = 0;
		currentPos.y = 0;
		
		float dir = (Vector3.Cross(lastPos,currentPos).y > 0) ? 1 : -1;
		float angleDelta = Vector3.Angle(lastPos,currentPos) * dir;

		return angleDelta;
	}

	float GetVerticalAngleDelta(Vector3 worldPos)
	{
		Vector3 lastPos = _lastWorldPos - transform.position;
		Vector3 currentPos = worldPos - transform.position;

		lastPos.x = 0;
		currentPos.x = 0;
		
		float dir = (Vector3.Cross(lastPos,currentPos).x > 0) ? 1 : -1;
		float angleDelta = Vector3.Angle(lastPos,currentPos) * dir;
		
		return angleDelta;
	}


	protected virtual Vector3 GetVelocity()
	{
		Vector3 worldPos = InputManager.Instance.GetTouchWorldPos();
		
		
		Vector3 vel = _velocity;
		
		if (InputManager.Instance.IsFingerMoving() && InputManager.Instance.GetTouchWorldPos() != Vector3.zero && InputManager.Instance.GetLastTouchWorldPos() != Vector3.zero)
		{
			float yVel = _lastWorldPos.y - worldPos.y;
			

//			if (this is HelixManager)
//			{
				vel = new Vector3( GetHorizontalAngleDelta(worldPos) , -yVel, 0);
//			}
//			if (this is FavoritesSphere)
//			{
//				vel.x *= .5f;// = new Vector3( GetHorizontalAngleDelta(worldPos), GetVerticalAngleDelta(worldPos), 0);
//
//			}

			
		}
		else if (InputManager.Instance.IsFingerStationary())
			vel = Vector3.zero;
		
		
		
		_lastWorldPos = InputManager.Instance.GetTouchWorldPos();
		return vel;
	}
	
	protected void ApplyFriction(float magnitude)
	{
		// apply friction
		magnitude -= Friction ;
		magnitude = Mathf.Max(0,magnitude);
		_velocity = _velocity.normalized * magnitude;
	}
	
	
	protected float LimitVelocity()
	{		
		float magnitude =_velocity.magnitude;
		float maxSpeed = MaxAngularSpeed;
		
		
		if (magnitude > maxSpeed)
		{
			_velocity = _velocity.normalized * maxSpeed;
			magnitude = maxSpeed;
		}
		
		return magnitude;
		
	}

	public float GetMaxY()
	{
		return _maxY;
	}
	
	public float GetMinY()
	{
		return _minY;
	}

	protected void SetZoomOutPos()
	{
		
		float midYPos = transform.position.y - (_maxY / 2.0f);
		
		
		Community.CurrentCommunity.SetZoomedOutYPos(midYPos);
	}

}
