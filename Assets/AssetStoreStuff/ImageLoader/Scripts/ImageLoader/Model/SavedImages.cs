using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml.Serialization;
using System.Linq;
using Com.Plsr.ImageLoader.Util;
using Com.Plsr.ImageLoader.Configuration;
using Com.Plsr.ImageLoader.Persistence;

namespace Com.Plsr.ImageLoader.Model {

	/// <summary>
	/// Saved images singleton class containing all caches images.
	/// </summary>
	[Serializable]
	public class SavedImages {

		/// <summary>
		/// The images.
		/// </summary>
		private List<ImageInfo> images;
		/// <summary>
		/// The file path.
		/// </summary>
		[NonSerialized]
		private static string filePath;

		/// <summary>
		/// Singleton instance.
		/// </summary>
		private static SavedImages _instance;
		/// <summary>
		/// Lock object for singleton creation.
		/// </summary>
		private static object _lock = new object();

		/// <summary>
		/// Initializes a new instance of the <see cref="Com.Plsr.ImageLoader.Model.SavedImages"/> class.
		/// </summary>
		private SavedImages() {
		}

		/// <summary>
		/// Initializes the list of saved images.
		/// </summary>
		/// <returns>The new saved images.</returns>
		private static SavedImages CreateNewSavedImages() {
			SavedImages savedImages = new SavedImages() {
				images = new List<ImageInfo>()
			};
			savedImages.Save();
			return savedImages;
		}

		/// <summary>
		/// Loads the list of saved images from file.
		/// </summary>
		/// <returns>The saved images from file.</returns>
		private static SavedImages LoadSavedImagesFromFile() {
			return new XmlUnitySerializer<SavedImages>().LoadContainer(filePath); 
		}

		/// <summary>
		/// Gets or sets the singleton instance.
		/// </summary>
		/// <value>The instance.</value>
		public static SavedImages Instance {
			get {			
				lock(_lock) {
					if (_instance == null) {
						filePath = PathCombiner.Combine(Application.persistentDataPath, Constants.PERSISTENCE_FILE_NAME);
						_instance = LoadSavedImagesFromFile();
						if (_instance == null) {
							_instance = CreateNewSavedImages();
						}
					}
					return _instance;
				}
			}
		}

		/// <summary>
		/// Gets or sets the images list.
		/// </summary>
		/// <value>The images.</value>
		public List<ImageInfo> Images {
			get {
				return images;
			}
			set {
				images = value;
			}
		}

		/// <summary>
		/// Save the images list.
		/// </summary>
		public void Save() {
			new XmlUnitySerializer<SavedImages>().SaveContainer(this, filePath);
		}

		/// <summary>
		/// Check if image list contains any image with the given key.
		/// </summary>
		/// <returns><c>true</c>, if key is contained if any of the images.</returns>
		/// <param name="key">Key.</param>
		public bool containsKey(string key) {
			foreach (ImageInfo imageInfo in this.images) {
				if (imageInfo.Key.Equals(key)) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Gets the oldest image.
		/// </summary>
		/// <returns>The oldest image.</returns>
		public ImageInfo GetOldestImage() {
			if (this.images.Count == 0) {
				return null;
			}
			return this.images.OrderBy(image => image.Created).Take(1).ElementAt(0);
		}

		/// <summary>
		/// Gets the total size of the cache.
		/// </summary>
		/// <returns>The total size.</returns>
		public int GetTotalSize() {
			return this.images.Sum(image => image.Size);
		}

	}

}