using UnityEngine;
using System.Collections;

public class SceneTransition : MonoBehaviour {

	public GameObject CenterPanel;

	public static SceneTransition Instance;
	bool _isInBrowseMode;

	// Use this for initialization
	void Start () {
		Instance = this;
	
	}
	
	public void TransitionToBrowseView()
	{
		LeanTween.scale(CenterPanel,Vector3.zero,1);

		GameObject camera = Camera.main.gameObject;

		Vector3 startPos = camera.transform.position;
		Vector3 startRotation = camera.transform.rotation.eulerAngles;

		camera.transform.rotation = Quaternion.Euler(Vector3.zero);
		camera.transform.position +=  InputManager.Instance.GetForward() * 10;
			
		LeanTween.move(camera,startPos,2).setEase(LeanTweenType.easeInOutQuad);
		LeanTween.rotate(camera,startRotation,2).setEase(LeanTweenType.easeInOutQuad);

		_isInBrowseMode = true;

	}

	public void TransitionToDefaultView()
	{
		LeanTween.scale(CenterPanel,Vector3.one,1);
		
		GameObject camera = Camera.main.gameObject;
		
		Vector3 startPos = camera.transform.position;
		
		camera.transform.position -=  InputManager.Instance.GetForward() * 10;
		
		LeanTween.move(camera,startPos,2).setEase(LeanTweenType.easeInOutQuad);
		
		_isInBrowseMode = true;
		
	}
}
