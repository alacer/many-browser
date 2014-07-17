using UnityEngine;
using System.Collections;

public class Advertisement : MonoBehaviour {

	Vector3 _startPos;

	void Awake()
	{
		_startPos = transform.localPosition;
	}

	public void Show()
	{
//		if (transform.localPosition != _startPos)
//			return;
//
//		transform.localPosition = _startPos;
//
//		LeanTween.moveLocal(gameObject,Vector3.zero,1).setOnComplete( () => {
//			LeanTween.moveLocal(gameObject,new Vector3(0,100,0),1).setDelay(2).setOnComplete ( () => {
//				transform.localPosition = _startPos;
//			});
//		});

	}
}
