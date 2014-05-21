using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ImageManager : MonoBehaviour {

	public static ImageManager Instance;

	List<ImageObj> _objsToLoad = new List<ImageObj>();
	List<ImageObj> _objsInQueue = new List<ImageObj>();

	List<string> _urls = new List<string>();
	float _frameRate;
	float _frames;
	float _time;
	float _checkTime = .5f;

	void CalculateFramerate()
	{
		_frames++;
		_time += Time.deltaTime;

		if (_time >= _checkTime)
		{
			_frameRate = _frames / _checkTime;

			_frames = 0;
			_time = 0;
		}
	}

	public float GetFPS()
	{
		return _frameRate;
	}

	void Awake()
	{
		Instance = this;
	}

	public void AddObjToLoad(ImageObj obj)
	{
		if (_objsInQueue.Contains(obj))
			_objsInQueue.Remove(obj);

		if (!_objsToLoad.Contains(obj))
			_objsToLoad.Add(obj);

	}

	public void AddObjToQueue(ImageObj obj)
	{
		if (_objsToLoad.Contains(obj))
			_objsToLoad.Remove(obj);
		
		if (!_objsInQueue.Contains(obj))
			_objsInQueue.Add(obj);
		
	}


	public void Initialize(List<string> urls)
	{
		_urls = urls;
	}

	void Update()
	{
		CalculateFramerate();

		if (_frameRate > 50)
		{
			if (_objsToLoad.Count > 0 && _urls != null)
			{
				for (int i = _objsToLoad.Count-1; i >= 0; i--)
				{
					ImageObj obj = _objsToLoad[i];
					StartCoroutine(obj.LoadTexture(_urls[Random.Range(0,_urls.Count)]));
					_objsToLoad.Remove(obj);
				}

			}
			else 
			{
				// if all visible objs are loaded take a few invisible ones at a time and load them
				for (int i=_objsInQueue.Count - 1; i >= Mathf.Max(0, _objsInQueue.Count - 5); i--)
				{
					ImageObj obj = _objsInQueue[i];
					_objsToLoad.Add(obj);
					_objsInQueue.Remove(obj);

				}


			}
		}

	}



}
