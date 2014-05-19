using System;
using UnityEngine;
using Com.Plsr.ImageLoader.Configuration;
using Com.Plsr.ImageLoader.Util;
using Com.Plsr.ImageLoader.Persistence;
using Com.Plsr.ImageLoader.Model;
using Com.Plsr.ImageLoader.Network;

namespace Com.Plsr.ImageLoader.Loader {

	/// <summary>
	/// Image builder class to help image loading with configurable options.
	/// </summary>
	public class ImageBuilder {

		private string url;
		private GameObject gameObject;
		private Material material;
		private bool tryFromCache;
		private bool saveInCache;
		private int retries;
		private ASPECT_MODE aspectMode;
		private Texture2D placeHolderTexture;
		private Action<WWW> callback;
		private string textureName;

		/// <summary>
		/// Gets or sets the URL.
		/// </summary>
		/// <value>The URL.</value>
		public string Url {
			get {
				return url;
			}
			set {
				url = value;
			}
		}

		public enum ASPECT_MODE {
			/// <summary>
			/// Aspect fit mode: it will scale gameobject to keep image aspect ratio.
			/// </summary>
			FIT,
			/// <summary>
			/// Aspect fill mode: gameobject will keep intact and image will fill holder.
			/// </summary>
			FILL
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Com.Plsr.ImageLoader.Loader.ImageBuilder"/> class.
		/// Here will be set default options.
		/// </summary>
		private ImageBuilder() {
			this.aspectMode = Constants.DEFAULT_IMAGE_RESIZE;
			this.tryFromCache = Constants.TRY_FROM_CACHE_BY_DEFAULT;
			this.saveInCache = Constants.SAVE_IN_CACHE_BY_DEFAULT;
			this.retries = Constants.DEFAULT_HTTP_RETRIES;
			this.textureName = Constants.DEFAULT_TEXTURE_NAME;
		}

		/// <summary>
		/// Callback to process image got through WWW class.
		/// </summary>
		/// <param name="www">WWW object which will contain loaded texture.</param>
		/// <param name="timeout">Indicates if request has gone timeout.</param>
		private void GotImageFromWWW(WWW www, bool timeout) {
			if (www.error == null && !timeout) {
				if (callback != null) {
					callback(www);
				} else {
					string key = KeyGenerator.getKeyFor (this.url);
					Texture2D texture = www.texture;
					if (Constants.USE_MEMORY_CACHE) {
						TexturesInMemoryCache.SetOnCache (key, www);
					}
					if (Application.isPlaying) {
						if (www.url.StartsWith ("http") && this.saveInCache) {
							ImagesCacheManager.CacheImage (texture, www);
						}
					}
					GotTexture (texture);
				}
			}
			RequestFinished();
		}

		/// <summary>
		/// Method that will be called when texture has been retrieved.
		/// </summary>
		/// <param name="texture">Loaded texture.</param>
		private void GotTexture(Texture2D texture) {
			ChangeTexture (texture);
			if (this.aspectMode.Equals (ASPECT_MODE.FIT)) {
				this.ResizeGameObjectToFitImage(texture);
			}
		}

		/// <summary>
		/// Notifies that image load has finished.
		/// </summary>
		private void RequestFinished() {
			ImagePool.Instance.LoadFinished(this);
		}

		/// <summary>
		/// Resizes the game object to fit image ratio.
		/// </summary>
		/// <param name="texture">Texture.</param>
		private void ResizeGameObjectToFitImage (Texture2D texture) {
			Vector3 newScale = this.gameObject.transform.localScale;
			float imageRatio = (float)texture.width / (float)texture.height;
			Vector3 vectorWidth = Vector3.Scale(Vector3.right, newScale);
			Vector3 vectorHeight = Vector3.Scale(Vector3.forward, newScale);
			float gameObjectRatio = (float)(vectorWidth.x + vectorWidth.y + vectorWidth.z) / (float)(vectorHeight.x + vectorHeight.y + vectorHeight.z);
			Vector3 large = newScale - vectorWidth - vectorHeight;
			if (gameObjectRatio < imageRatio) {
				newScale = vectorHeight * gameObjectRatio / imageRatio + vectorWidth + large;
			}
			else if (imageRatio < gameObjectRatio) {
				newScale = vectorWidth * imageRatio / gameObjectRatio + vectorHeight + large;
			}
			this.gameObject.transform.localScale = newScale;
		}

		/// <summary>
		/// Sets the aspect behaviour.
		/// </summary>
		/// <param name="aspectMode">Aspect mode.</param>
		public ImageBuilder Aspect(ASPECT_MODE aspectMode) {
			this.aspectMode = aspectMode;
			return this;
		}

		/// <summary>
		/// Allows to set if image can be loaded from cache.
		/// </summary>
		/// <returns>Self ImageBuilder object.</returns>
		/// <param name="tryFromCache">If set to <c>true</c> then cache will be tried.</param>
		public ImageBuilder TryFromCache(bool tryFromCache) {
			this.tryFromCache = tryFromCache;
			return this;
		}

		/// <summary>
		/// Allows to set if image will be cached.
		/// </summary>
		/// <returns>Self ImageBuilder object.</returns>
		/// <param name="saveInCache">If set to <c>true</c> image will be cached.</param>
		public ImageBuilder SaveInCache(bool saveInCache) {
			this.saveInCache = saveInCache;
			return this;
		}

		/// <summary>
		/// Allows to set the number of retries.
		/// </summary>
		/// <returns>Self ImageBuilder object.</returns>
		/// <param name="retries">Retries.</param>
		public ImageBuilder Retries(int retries) {
			this.retries = retries;
			return this;
		}

		/// <summary>
		/// Allows to set a holder while image is loaded.
		/// </summary>
		/// <returns>Self ImageBuilder object.</returns>
		/// <param name="placeHolderTexture">Place holder texture.</param>
		public ImageBuilder PlaceHolder(Texture2D placeHolderTexture) {
			this.placeHolderTexture = placeHolderTexture;
			return this;
		}

		/// <summary>
		/// Starts the process of loading/downloading image into the specified gameobject and material. When image is retrieved, texture of the given material will be replaced.
		/// </summary>
		/// <param name="gameObject">Game object in which draw the texture.</param>
		/// <param name="material">Material in which set the texture.</param>
		public void Into(GameObject gameObject, Material material) {
			this.gameObject = gameObject;
			this.material = material;
			PutPlaceHolder();
			if (!String.IsNullOrEmpty(this.url) && this.gameObject != null && this.material != null) {
				ExecuteOrEnqueue();
			}
	    }

		/// <summary>
		/// Starts the process of loading/downloading image into the specified gameobject, material and texture. When image is retrieved, texture of the given material will be replaced.
		/// </summary>
		/// <param name="gameObject">Game object in which draw the texture.</param>
		/// <param name="material">Material in which set the texture.</param>
		/// <param name="textureName">Texture name.</param>
		public void Into(GameObject gameObject, Material material, string textureName) {
			this.gameObject = gameObject;
			this.material = material;
			this.textureName = textureName;
			PutPlaceHolder();
			if (!String.IsNullOrEmpty(this.url) && this.gameObject != null && this.material != null) {
				ExecuteOrEnqueue();
			}
		}

		/// <summary>
		/// Starts the process of loading/downloading image. When image is retrieved, callback with WWW object will be called.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public void Into(Action<WWW> callback) {
			if (callback != null) {
				this.callback = callback;
				PutPlaceHolder();
				if (!String.IsNullOrEmpty(this.url)) {
					ExecuteOrEnqueue();
				}
			}
		}

		/// <summary>
		/// Executes or enqueues an image load.
		/// </summary>
		public void ExecuteOrEnqueue() {
			if (ImagePool.Instance.CanDoRequest()) {
				StartImageLoad();
			} else {
				ImagePool.Instance.EnqueueLoad(this);
			}
		}

		/// <summary>
		/// Puts the place holder.
		/// </summary>
		private void PutPlaceHolder() {
			if (this.placeHolderTexture != null) {
				ChangeTexture (this.placeHolderTexture);
			}
		}

		/// <summary>
		/// Changes the texture.
		/// </summary>
		/// <param name="texture">Texture.</param>
		private void ChangeTexture(Texture2D texture) {
			this.material.SetTexture (this.textureName, texture);
		}

		/// <summary>
		/// Starts the image load/download process.
		/// </summary>
		private void StartImageLoad() {
			ImagePool.Instance.AddActiveLoad(this);
			if (!Application.isPlaying) {
				GetFromNetwork();
			} else {
				if (this.tryFromCache) {
					if (Constants.USE_MEMORY_CACHE) {
						string key = KeyGenerator.getKeyFor (this.url);
						if (TexturesInMemoryCache.GetFromCache (key) != null) {
							GotImageFromWWW(TexturesInMemoryCache.GetFromCache (key), false);
							/*GotTexture (TexturesInMemoryCache.GetFromCache (key));
							RequestFinished();*/
						} else {
							if (!TryToGetFromCache ()) {
								GetFromNetwork();
							}
						}
					} else {
						if (!TryToGetFromCache ()) {
							GetFromNetwork();
						}
					}
				} else {
					GetFromNetwork();
				}
			}
		}

		/// <summary>
		/// Tries to get tha image from cache.
		/// </summary>
		/// <returns>Returns <c>true</c> if image is in cache</returns>
		private bool TryToGetFromCache() {
			return ImagesCacheManager.GetImage(this.url, GotImageFromWWW);
		}

		/// <summary>
		/// Retrieves the image from network.
		/// </summary>
		private void GetFromNetwork() {
			Request request = new Request() {
				Url = this.url,
				Callback = this.GotImageFromWWW,
				Retries = this.retries
			};
			NetworkManager.Instance.DoGet(request);
	    }

		public static ImageBuilder Load(string url) {
			ImageBuilder imageBuilder = new ImageBuilder() {
				Url = url
			};
			return imageBuilder;
		}
	    
	}

}