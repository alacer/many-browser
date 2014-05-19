using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using Com.Plsr.ImageLoader.Util;
using Com.Plsr.ImageLoader.Configuration;

namespace Com.Plsr.ImageLoader.Persistence {

	/// <summary>
	/// Storage manager singleton to save objects.
	/// </summary>
	public class StorageManager : Singleton<StorageManager> {

		/// <summary>
		/// Storage path.
		/// </summary>
		private string rootFilesPath;

		/// <summary>
		/// Initializes a new instance of the <see cref="Com.Plsr.ImageLoader.Persistence.StorageManager"/> class.
		/// </summary>
		private StorageManager() {
			this.rootFilesPath = PathCombiner.Combine(Constants.PERSISTENT_DATA_PATH, Constants.CACHE_FOLDER_NAME);
			Directory.CreateDirectory(this.rootFilesPath);
		}

		/// <summary>
		/// Saves the file.
		/// </summary>
		/// <param name="data">Byte array of data.</param>
		/// <param name="key">Key identifying the file.</param>
		public void SaveFile(byte[] data, string key) {
			if (data != null) {
				string filePath = PathCombiner.Combine(this.rootFilesPath, key);
				FileStream fileStream = new FileStream(filePath, FileMode.Create);
				fileStream.Write(data, 0, data.Length);
				fileStream.Close();
			}
		}

		/// <summary>
		/// Deletes the file.
		/// </summary>
		/// <param name="key">Key associated to the file.</param>
		public void DeleteFile(string key) {
			string filePath = PathCombiner.Combine(this.rootFilesPath, key);
			File.Delete(filePath);
		}

		/// <summary>
		/// Gets the file asynchronously.
		/// </summary>
		/// <param name="key">Key of the file.</param>
		/// <param name="callback">Callback to return with the WWW loaded object.</param>
		public void GetFile(string key, Action<WWW, bool> callback) {
			string filePath = PathCombiner.Combine(this.rootFilesPath, key);
			if (!File.Exists(filePath)) {
				if (callback != null) {
					callback(null, false);
				}
			}
			else {
				StartCoroutine(LoadFileAsync(filePath, callback));
			}
		}

		/// <summary>
		/// Loads file asynchronously through WWW class.
		/// </summary>
		/// <param name="filePath">File path.</param>
		/// <param name="callback">Callback to call with the WWW loaded object.</param>
		private IEnumerator LoadFileAsync(String filePath, Action<WWW, bool> callback) {
			WWW www = new WWW("file://" + filePath);
			yield return www;
			if (callback != null) {
				callback(www, false);
			}
	    }
	    
	}

}