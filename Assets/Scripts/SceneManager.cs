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
	Scene _transitioningToScene;

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

	public Scene GetLastScene()
	{
		return _lastScene;
	}

	public Scene GetTransitioningToScene()
	{
		return _transitioningToScene;
	}

	public void SetTransitioning(bool isTransitioning)
	{
		_isTransitioning = isTransitioning;
		Debug.Log("set transitioning: " + isTransitioning);

	}

	public void PushScene(Scene scene)
	{
		_isTransitioning = false;

		_lastScene = _currentScene;

		_currentScene = scene;
		Debug.Log("setting scene: " + scene);

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
		HelixManager.Instance.SetAllVisible(true);


		PushScene(Scene.Browse);
	}

	public void OnSceneTransition(Scene toScene)
	{
		_isTransitioning = true;
		_transitioningToScene = toScene;

		Debug.Log("onscenetransition");
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
				HelixManager.Instance.SetAllVisible(false);
				camera.transform.position = startPos;
				camera.transform.rotation = Quaternion.Euler(startRotation);
				PushScene(Scene.Default);
			});

		});

		OnSceneTransition(Scene.Default);
		
	}
}
