using UnityEngine;
using System.Collections;

public enum Scene
{
	Default,
	Browse,
	Helix,
	Selected,
	InTransition

}

public class SceneManager : MonoBehaviour {

	public GameObject CenterPanel;

	public static SceneManager Instance;
	Scene _currentScene;
	Scene _lastScene  = Scene.Default;
	bool _isTransitioning;

	// Use this for initialization
	void Start () {
		Instance = this;
			
		PushScene(Scene.Default);
	}

	public static bool IsInHelixOrBrowse()
	{
		return (Instance.GetScene() == Scene.Helix || Instance.GetScene() == Scene.Browse);
	}

	public Scene GetScene()
	{
		if (_isTransitioning)
			return Scene.InTransition;
		else
			return _currentScene;
	}

	public void PushScene(Scene scene)
	{
		_isTransitioning = false;

		_lastScene = _currentScene;

		_currentScene = scene;
		Utils.SendMessageToAll("OnSceneChange",_currentScene);
	}

	public void PopScene()
	{
		_isTransitioning = false;
		_currentScene = _lastScene;
		Utils.SendMessageToAll("OnSceneChange",_currentScene);
	}

	public void TransitionToBrowseView()
	{
		GridManager.Instance.SetAllVisible(true);


		GameObject camera = Camera.main.gameObject;

		Vector3 startPos = camera.transform.position;
		Vector3 startRotation = camera.transform.rotation.eulerAngles;

		camera.transform.rotation = Quaternion.Euler(Vector3.zero);
		camera.transform.position +=  CameraManager.Instance.GetForward() * 10;//GridManager.Instance.GetZPadding()/2.0f;

		LeanTween.scale(CenterPanel,Vector3.zero,1).setOnComplete(() => 
		{ 
			LeanTween.delayedCall(1,() => 
			{ 
				LeanTween.rotate(camera,startRotation,1).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() => 
				                                                                                            { 
					PushScene(Scene.Browse);
				});
			});
			LeanTween.move(camera,startPos,2).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() => 
			                                                                                     { 
				camera.transform.position = startPos;
			});;
		});

		OnSceneTransition();

	}

	public void OnSceneTransition()
	{
		_isTransitioning = true;
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
				PushScene(Scene.Default);
			});

		});

		OnSceneTransition();
		
	}
}
