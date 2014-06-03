using UnityEngine;
using System.Collections;

public class CubeRotator : MonoBehaviour {

	public Transform CubeTransform;
	public float _rotationSpeed = 1;
	public float MinAngleToRotate = 5;
	Vector3 forward = Vector3.back;

	float _totalAngleChange;
	Vector3 _startRotation = new Vector3(0,270,0);
	float _lastSpeed;

	GameObject _description;
	GameObject _buttonPanel;
	bool _fingerLiftedAfterSwipe = true;
	bool _selected;

	void Start()
	{

		_description = GameObject.Find("ObjectDescriptionLabel");
		_buttonPanel = GameObject.Find("ActionButtonPanel");

		_description.GetComponent<UILabel>().text = transform.parent.GetComponent<ImageObj>().GetData<string>("Description");
	}
	
	// Update is called once per frame
	void Update () {


		Vector3 touchDelta = InputManager.Instance.GetOneFingerTouchDelta();



		if (InputManager.Instance.GetTouchCount() == 0)
		{
	//		Debug.Log("touch count 0");
			_totalAngleChange = 0;
			_fingerLiftedAfterSwipe = true;
		}

		if (touchDelta.x != 0 && LeanTween.isTweening(CubeTransform.gameObject) == false)
		{
	
			_totalAngleChange += touchDelta.x;

//			Debug.Log("total angle: " + _totalAngleChange + " Min angle: " + MinAngleToRotate + "fingerlifted " + _fingerLiftedAfterSwipe);
		}

		if (Mathf.Abs(_totalAngleChange) > MinAngleToRotate && _fingerLiftedAfterSwipe)
		{
			TweenAlpha.Begin(_description,0,0);
			FadeButtonPanel(0,0);

			float dir = (_totalAngleChange > 0) ? -1 : 1;

			Debug.Log("rotating");

			LeanTween.rotateY(CubeTransform.gameObject,CubeTransform.rotation.eulerAngles.y + (dir * 90),.3f).setOnComplete( () => 
			{
				forward = Quaternion.AngleAxis(-dir * 90,Vector3.up) * forward;
				Debug.Log("forward: " + forward);

				if (forward == Vector3.right)
					TweenAlpha.Begin(_description,.3f,1);
				else if (forward == Vector3.left)
					FadeButtonPanel(1,.3f);

				_fingerLiftedAfterSwipe = false;
				_totalAngleChange = 0;
			});


			_fingerLiftedAfterSwipe = false;
			_totalAngleChange = 0;
		}
	
	}

	void FadeButtonPanel(float alpha, float time)
	{
		for (int i=0; i < _buttonPanel.transform.childCount; i++)
		{
		//	Debug.Log("child name: " + 
			TweenAlpha.Begin(_buttonPanel.transform.GetChild(i).gameObject,time,alpha);

		}

	}
	

	void OnSelect()
	{	
		_selected = true;
	//	LeanTween.rotate(CubeTransform.gameObject,_startRotation,.3f);
	}

	
	void OnUnselect()
	{

		Debug.Log("cube rototor unselect");
		FadeButtonPanel(0,0);
		TweenAlpha.Begin(_description,0,0);
		TweenAlpha.Begin(_buttonPanel,0,0);

		LeanTween.cancel(CubeTransform.gameObject);

		LeanTween.rotateLocal(CubeTransform.gameObject,_startRotation,.3f);
		forward = Vector3.back;

		_selected = false;
	}
}
