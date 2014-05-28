using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ImageManager : MonoBehaviour {

	public static ImageManager Instance;

	List<ImageObj> _objsToLoad = new List<ImageObj>();
	List<ImageObj> _objsInQueue = new List<ImageObj>();
	Dictionary<ImageObj,string> _imageObjToUrl = new Dictionary<ImageObj, string>();
	Dictionary<string,List<ImageObj>> _urlToImageObj = new Dictionary<string, List<ImageObj>>();

	ImageObj[] _imageObjs;
	List<string> _urls = new List<string>();
	float _frameRate;
	float _frames;
	float _time;
	float _checkTime = .5f;
	int _imageIndex;

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

	public void Clear()
	{
		_urlToImageObj.Clear();
		_imageObjToUrl.Clear();

		_objsToLoad.Clear();
		_objsInQueue.Clear();
		_urls.Clear();
	}

	public void Initialize(List<string> urls)
	{
		_urls = urls;
		_imageObjs = FindObjectsOfType<ImageObj>();
		CreateDictionaries();

		GetAllImages();

		Debug.Log("got urls");

	}

	void CreateDictionaries()
	{
		for (int i=0; i < _imageObjs.Length; i++)
		{
			_imageObjToUrl.Add(_imageObjs[i], _urls[i % _urls.Count]);
		}

		foreach (string url in _urls)
		{
			foreach (ImageObj obj in _imageObjs)
			{
				if (url == _imageObjToUrl[obj])
				{
					if (_urlToImageObj.ContainsKey(url) == false)
						_urlToImageObj[url] = new List<ImageObj>();

					_urlToImageObj[url].Add(obj);

				}
				
			}
		}
	}

	void GetAllImages()
	{
		
		var textureCache = WebTextureCache.InstantiateGlobal ();


		foreach (string url in _urls)
		{
			StartCoroutine (textureCache.GetTexture (url, OnGotTexture));

		}
	}

	void OnGotTexture(string url, Texture2D tex)
	{
		foreach (ImageObj obj in _urlToImageObj[url])
		{

			obj.SetTexture(tex);

		}
	}

//	void Update()
//	{
//		if (_urls.Count == 0)
//			return;
//
//		CalculateFramerate();
//
////		if (_frameRate > 50)
////		{
//			if (_objsToLoad.Count > 0 && _urls != null)
//			{
//
//				for (int i = _objsToLoad.Count-1; i >= 0; i--)
//				{
//					ImageObj obj = _objsToLoad[i];
//					obj.GetImage(_urls[_imageIndex]);
//					_imageIndex = (_imageIndex + 1) % _urls.Count;
//					_objsToLoad.Remove(obj);
//				}
//
//			}
//			else 
//			{
//				// if all visible objs are loaded take a few invisible ones at a time and load them
//				for (int i=_objsInQueue.Count - 1; i >= Mathf.Max(0, _objsInQueue.Count - 5); i--)
//				{
//					ImageObj obj = _objsInQueue[i];
//					_objsToLoad.Add(obj);
//					_objsInQueue.Remove(obj);
//
//				}
//
//
//			}
////		}
//
//	}



}
