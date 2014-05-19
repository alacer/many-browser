using UnityEngine;
using System.Collections;
using System;

namespace Com.Plsr.ImageLoader.Model {

	/// <summary>
	/// Image info class containing info about cached images.
	/// </summary>
	[Serializable]
	public class ImageInfo {

		/// <summary>
		/// Size of the image (bytes).
		/// </summary>
		private int size;
		/// <summary>
		/// Generated hash key identifying the image.
		/// </summary>
		private string key;
		/// <summary>
		/// Width of the image (pixels).
		/// </summary>
		private int width;
		/// <summary>
		/// Height of the image (pixels).
		/// </summary>
		private int height;
		/// <summary>
		/// Name of the image (URL pointing to the original source).
		/// </summary>
		private string name;
		/// <summary>
		/// Image caching date.
		/// </summary>
		private DateTime created;

		/// <summary>
		/// Initializes a new instance of the <see cref="Com.Plsr.ImageLoader.Model.ImageInfo"/> class.
		/// </summary>
		public ImageInfo() {
			this.created = DateTime.Now;
		}

		/// <summary>
		/// Gets or sets the size.
		/// </summary>
		/// <value>The size.</value>
		public int Size {
			get {
				return size;
			}
			set {
				size = value;
			}
		}

		/// <summary>
		/// Gets or sets the key.
		/// </summary>
		/// <value>The key.</value>
		public string Key {
			get {
				return key;
			}
			set {
				key = value;
			}
		}

		/// <summary>
		/// Gets or sets the width.
		/// </summary>
		/// <value>The width.</value>
		public int Width {
			get {
				return width;
			}
			set {
				width = value;
			}
		}

		/// <summary>
		/// Gets or sets the height.
		/// </summary>
		/// <value>The height.</value>
		public int Height {
			get {
				return height;
			}
			set {
				height = value;
			}
		}

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name {
			get {
				return this.name;
			}
			set {
				this.name = value;
			}
		}

		/// <summary>
		/// Gets or sets the created.
		/// </summary>
		/// <value>The created.</value>
		public DateTime Created {
			get {
				return this.created;
			}
			set {
				this.created = value;
			}
		}

	}

}