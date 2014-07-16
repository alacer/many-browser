using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ImageManager : MonoBehaviour {

	public static ImageManager Instance;

	Dictionary<string,ImageObj> _urlToImageObj = new Dictionary<string,ImageObj>();
	

	ImageObj[] _imageObjs;
	List< Dictionary<string,object> > _objData = new List<Dictionary<string, object>>();

	float _frameRate;
	float _frames;
	float _time;

	int _imageIndex;

	void Awake()
	{
		Instance = this;
	}

	public void Initialize(List< Dictionary<string,object> > objData)
	{
		_objData = objData;
		_imageObjs = HelixManager.Instance.GetAllObjs().ToArray();
		CreateDictionary();
		
		GetAllImages();
		
		Debug.Log("got urls");
		
	}

	void CreateDictionary()
	{

		foreach (Dictionary<string,object> data in _objData)
		{
			string url = (string)data["Url"];
			
			foreach (ImageObj obj in _imageObjs)
			{
				if (url == obj.GetData<string>("Url"))
				{
					_urlToImageObj[url] = obj;
				}
			}
		}
	}
	
	public void Clear()
	{
		_urlToImageObj.Clear();
		_objData.Clear();

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
		ImageObj obj = _urlToImageObj[data["Url"].ToString()];

		obj.Initialize(tex);
		_objData.Remove(data);

		if (_objData.Count == 0)
			OnLoadedAllImages();

	}

	void OnLoadedAllImages()
	{
//		GameObject.Find("SortButtons").SendMessage("Show");
	}

}
