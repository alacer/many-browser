using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WebTextureCache : MonoBehaviour
{
	private Dictionary<string, Texture2D> imageCache = new Dictionary<string, Texture2D> ();
	private Dictionary<string, WWW> requestCache = new Dictionary<string, WWW> ();
	private static WebTextureCache instance = null;



	/// <summary>
	/// Instantiates a global instance of this object in the scene
	/// </summary>
	/// <param name='name'>What to name the new global object </param>
	public static WebTextureCache InstantiateGlobal (string name = "WebTextureCache")
	{
		if (instance == null) {
			var gameobject = new GameObject (name);
			gameobject.AddComponent<WebTextureCache> ();
			instance = gameobject.GetComponent<WebTextureCache> ();
		}
		
		return instance;
	}
	
	public IEnumerator GetTexture (string url, Dictionary<string,object> data, Action<string, Dictionary<string,object>,Texture2D> callback)
	{

		if (!this.imageCache.ContainsKey (url)) {
			int retryTimes = 3; // Number of time to retry if we get a web error
			WWW request;
			do {
				--retryTimes;
				if (!this.requestCache.ContainsKey (url)) {
					// Create a new web request and cache is so any additional
					// calls with the same url share the same request.
					this.requestCache [url] = new WWW (url);
				}
				
				request = this.requestCache [url];
				yield return request;
				
				// Remove this request from the cache if it is the first to finish
				if (this.requestCache.ContainsKey (url)&& this.requestCache [url] == request) {
					this.requestCache.Remove (url);
				}
			} while(request.error != null && retryTimes >= 0);
			
			// If there are no errors add this is the first to finish,
			// then add the texture to the texture cache.
			if (request.error == null && !this.imageCache.ContainsKey (url)) {
				this.imageCache [url] = request.texture;
			}
		}
		
		if (callback != null) {
			// By the time we get here there is either a valid image in the cache
			// or we were not able to get the requested image.
			Texture2D texture = null;
			this.imageCache.TryGetValue (url, out texture);
			callback (url, data,texture);
		}
	}
}