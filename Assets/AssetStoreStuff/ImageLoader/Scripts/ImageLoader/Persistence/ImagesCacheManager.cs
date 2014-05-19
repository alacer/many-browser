using System;
using UnityEngine;
using Com.Plsr.ImageLoader.Util;
using Com.Plsr.ImageLoader.Model;
using Com.Plsr.ImageLoader.Configuration;

namespace Com.Plsr.ImageLoader.Persistence {

	/// <summary>
	/// Images cache manager to handle image saving and retreiving from persistent storage.
	/// </summary>
	public class ImagesCacheManager {
			
		/// <summary>
		/// Initializes a new instance of the <see cref="Com.Plsr.ImageLoader.Persistence.ImagesCacheManager"/> class.
		/// </summary>
		private ImagesCacheManager () {
		}

		/// <summary>
		/// Caches the image file into disk.
		/// </summary>
		/// <returns><c>true</c>, if image was cached, <c>false</c> otherwise.</returns>
		/// <param name="texture">Texture.</param>
		/// <param name="www">Www.</param>
		public static bool CacheImage(Texture2D texture, WWW www) {
			string key = KeyGenerator.getKeyFor(www.url);
			if (!SavedImages.Instance.containsKey(key)) {
				// Delete oldest image until we have enough free space to store the new one
				while (SavedImages.Instance.GetTotalSize() + www.bytes.Length > Constants.PERSISTENCE_STORAGE_SIZE && SavedImages.Instance.Images.Count > 0) {
					DeleteOldestImage();
				}
				if (SavedImages.Instance.GetTotalSize() + www.bytes.Length > Constants.PERSISTENCE_STORAGE_SIZE) {
					return false;
				}
				SaveImage(texture, www, key);
				return true;
	        }
			return true;
		}

		/// <summary>
		/// Gets the image asynchronously from the cache.
		/// </summary>
		/// <returns><c>true</c>, if image exists, <c>false</c> otherwise.</returns>
		/// <param name="url">URL pointing the image.</param>
		/// <param name="callback">Callback when the image is loaded.</param>
		public static bool GetImage(string url, Action<WWW, bool> callback) {
			string key = KeyGenerator.getKeyFor(url);
			if (!SavedImages.Instance.containsKey(key)) {
				return false;
			}
			else {
				StorageManager.Instance.GetFile(key, callback);
				return true;
			}
		}

		/// <summary>
		/// Deletes the oldest image.
		/// </summary>
		private static void DeleteOldestImage() {
			ImageInfo oldestImage = SavedImages.Instance.GetOldestImage();
			string oldestImagePath = PathCombiner.Combine(PathCombiner.Combine(Application.persistentDataPath, Constants.CACHE_FOLDER_NAME), oldestImage.Key);
			StorageManager.Instance.DeleteFile(oldestImagePath);
			SavedImages.Instance.Images.Remove(oldestImage);
			SavedImages.Instance.Save();
		}

		/// <summary>
		/// Saves the image.
		/// </summary>
		/// <param name="texture">Texture to save.</param>
		/// <param name="www">WWW of the downloaded object.</param>
		/// <param name="key">Key for the image.</param>
		private static void SaveImage(Texture2D texture, WWW www, string key) {
			StorageManager.Instance.SaveFile(www.bytes, key);
			ImageInfo imageInfo = new ImageInfo() {
				Height = texture.height,
				Width = texture.width,
				Key = key,
				Name = www.url,
				Size = www.bytes.Length
			};
			SavedImages.Instance.Images.Add(imageInfo);
			SavedImages.Instance.Save();
		}

	}

}