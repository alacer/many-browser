using UnityEngine;
using System.Collections;
using System;

namespace Com.Plsr.ImageLoader.Model {

	/// <summary>
	/// Request class containing info of HTPP requests.
	/// </summary>
	public class Request {

		/// <summary>
		/// The identifier of the request (Guid).
		/// </summary>
		private string id;
		/// <summary>
		/// Request priority.
		/// </summary>
		private int priority;
		/// <summary>
		/// Number of retries in case of failure.
		/// </summary>
		private int retries;
		/// <summary>
		/// Request timeout (milliseconds).
		/// </summary>
		private int timeout;
		/// <summary>
		/// URL pointing to the image.
		/// </summary>
		private string url;
		/// <summary>
		/// Callback when image is loaded.
		/// </summary>
		private Action<WWW, bool> callback;

		/// <summary>
		/// Initializes a new instance of the <see cref="Com.Plsr.ImageLoader.Model.Request"/> class.
		/// </summary>
		public Request() {
			this.id = Guid.NewGuid().ToString();
		}

		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public string Id {
			get {
				return this.id;
			}
			set {
				;
			}
		}

		/// <summary>
		/// Gets or sets the priority.
		/// </summary>
		/// <value>The priority.</value>
		public int Priority {
			get {
				return this.priority;
			}
			set {
				this.priority = value;
			}
		}

		/// <summary>
		/// Gets or sets the retries.
		/// </summary>
		/// <value>The retries.</value>
		public int Retries {
			get {
				return this.retries;
			}
			set {
				this.retries = value;
			}
		}

		/// <summary>
		/// Gets or sets the timeout.
		/// </summary>
		/// <value>The timeout.</value>
		public int Timeout {
			get {
				return this.timeout;
			}
			set {
				this.timeout = value;
			}
		}

		/// <summary>
		/// Gets or sets the URL.
		/// </summary>
		/// <value>The URL.</value>
		public string Url {
			get {
				return this.url;
			}
			set {
				this.url = value;
			}
		}

		/// <summary>
		/// Gets or sets the callback.
		/// </summary>
		/// <value>The callback.</value>
		public Action<WWW, bool> Callback {
			get {
				return this.callback;
			}
			set {
				this.callback = value;
			}
		}

	}

}