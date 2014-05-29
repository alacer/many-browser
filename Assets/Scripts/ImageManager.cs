using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ImageManager : MonoBehaviour {

	public static ImageManager Instance;
	
	Dictionary<ImageObj,string> _imageObjToUrl = new Dictionary<ImageObj, string>();
	Dictionary<string,List<ImageObj>> _urlToImageObj = new Dictionary<string, List<ImageObj>>();

	Dictionary<string,string> _largeImageDict = new Dictionary<string, string>();

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

//	public void AddObjToLoad(ImageObj obj)
//	{
//		if (_objsInQueue.Contains(obj))
//			_objsInQueue.Remove(obj);
//
//		if (!_objsToLoad.Contains(obj))
//			_objsToLoad.Add(obj);
//
//	}
//
//	public void AddObjToQueue(ImageObj obj)
//	{
//		if (_objsToLoad.Contains(obj))
//			_objsToLoad.Remove(obj);
//		
//		if (!_objsInQueue.Contains(obj))
//			_objsInQueue.Add(obj);
//		
//	}

	public bool GetLargeImageUrl(string smallUrl, out string largeUrl)
	{
		if (smallUrl != null && _largeImageDict.ContainsKey(smallUrl))
		{
			largeUrl = _largeImageDict[smallUrl];
			return true;
		}

		largeUrl = null;
		return false;
	}

	public void Clear()
	{
		_urlToImageObj.Clear();
		_imageObjToUrl.Clear();
		_largeImageDict.Clear();

		_urls.Clear();
	}

	public void Initialize(List<string> urls, Dictionary<string,string> largeImageDict)
	{
		_largeImageDict = largeImageDict;
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

			obj.SetTexture(tex,url);

		}
	}





}
