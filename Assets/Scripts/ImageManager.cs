using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ImageManager : MonoBehaviour {

	public static ImageManager Instance;
	
	Dictionary<ImageObj,string> _imageObjToUrl = new Dictionary<ImageObj, string>();
	Dictionary<string,List<ImageObj>> _urlToImageObj = new Dictionary<string, List<ImageObj>>();

//	Dictionary<string,string> _largeImageDict = new Dictionary<string, string>();

	ImageObj[] _imageObjs;
	List< Dictionary<string,object> > _objData = new List<Dictionary<string, object>>();

	float _frameRate;
	float _frames;
	float _time;
	float _checkTime = .5f;
	int _imageIndex;


	public void Initialize(List< Dictionary<string,object> > objData)
	{
		_objData = objData;
		_imageObjs = FindObjectsOfType<ImageObj>();
		CreateDictionaries();
		
		GetAllImages();
		
		Debug.Log("got urls");
		
	}

	
	void Awake()
	{
		Instance = this;
	}

	
	public void Clear()
	{
		_urlToImageObj.Clear();
		_imageObjToUrl.Clear();

		_objData.Clear();

	}

	public List<ImageObj> GetUniqueImageObjList()
	{
		List<ImageObj> uniqueList = new List<ImageObj>();

		foreach (List<ImageObj> list in _urlToImageObj.Values)
		{
			uniqueList.Add(list[0]);

		}

		return uniqueList;
	}


	void CreateDictionaries()
	{
		for (int i=0; i < _imageObjs.Length; i++)
		{
			string url = (string)_objData[i % _objData.Count]["Url"];
		
			_imageObjToUrl.Add(_imageObjs[i], url);
		}

		foreach (Dictionary<string,object> data in _objData)
		{
			string url = (string)data["Url"];

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



		for (int i= _objData.Count-1; i >= 0; i--)
		{
			Dictionary<string,object> data = _objData[i];
			StartCoroutine (textureCache.GetTexture ((string)data["Url"], data, OnGotTexture));

		}
	}

	void OnGotTexture(string url, Dictionary<string,object> data, Texture2D tex)
	{
		foreach (ImageObj obj in _urlToImageObj[data["Url"].ToString()])
		{

			obj.Initialize(tex,data);
			_objData.Remove(data);
			if (_objData.Count == 0)
				OnLoadedAllImages();
		}
	}

	void OnLoadedAllImages()
	{
//		GameObject.Find("SortButtons").SendMessage("Show");
	}

}
