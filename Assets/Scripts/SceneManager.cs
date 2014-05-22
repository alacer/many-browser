using UnityEngine;
using System.Collections;

public enum Scene
{
	Default,
	Browse,
	InTransition

}

public class SceneManager : MonoBehaviour {

	public GameObject CenterPanel;

	public static SceneManager Instance;
	Scene _currentScene = Scene.Default;

	// Use this for initialization
	void Start () {
		Instance = this;
			
	}

	public Scene GetScene()
	{
		return _currentScene;
	}

	public void TransitionToBrowseView()
	{
		GridManager.Instance.SetAllVisible(true);


		GameObject camera = Camera.main.gameObject;

		Vector3 startPos = camera.transform.position;
		Vector3 startRotation = camera.transform.rotation.eulerAngles;

		camera.transform.rotation = Quaternion.Euler(Vector3.zero);
		camera.transform.position +=  CameraManager.Instance.GetForward() * 10;

		LeanTween.scale(CenterPanel,Vector3.zero,1).setOnComplete(() => 
		{ 
			LeanTween.delayedCall(1,() => 
			{ 
				LeanTween.rotate(camera,startRotation,1).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() => 
				                                                                                            { 
					_currentScene = Scene.Browse;
				});
			});
			LeanTween.move(camera,startPos,2).setEase(LeanTweenType.easeInOutQuad);
		});

		_currentScene = Scene.InTransition;

	}

	public void TransitionToDefaultView()
	{

		GameObject camera = Camera.main.gameObject;
		Vector3 startRotation = camera.transform.rotation.eulerAngles;
		Vector3 startPos = camera.transform.position;
		
		Vector3 movePos = camera.transform.position + CameraManager.Instance.GetForward() * 10;
		 
		LeanTween.move(camera,movePos,1).setEase(LeanTweenType.easeInOutQuad);
		LeanTween.rotate(camera,Vector3.zero,1).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() => 
		{ 

			LeanTween.scale(CenterPanel,Vector3.one,1).setEase(LeanTweenType.easeOutQuint).setOnComplete(() => 
			{ 
				GridManager.Instance.SetAllVisible(false);
				camera.transform.position = startPos;
				camera.transform.rotation = Quaternion.Euler(startRotation);
				_currentScene = Scene.Default;
			});

		});
		
		_currentScene = Scene.InTransition;
		
	}
}
