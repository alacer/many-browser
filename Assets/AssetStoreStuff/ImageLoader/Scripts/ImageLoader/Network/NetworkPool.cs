using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Com.Plsr.ImageLoader.Model;
using Com.Plsr.ImageLoader.Configuration;

namespace Com.Plsr.ImageLoader.Network {

	/// <summary>
	/// Network pool class that handles active and pending HTTP requests.
	/// </summary>
	public class NetworkPool {

		/// <summary>
		/// Singleton instance.
		/// </summary>
		private static NetworkPool _instance;
		/// <summary>
		/// Lock object for singleton creation.
		/// </summary>
		private static object _lock = new object();

		/// <summary>
		/// List of active requests.
		/// </summary>
		private List<Request> activeRequests;
		/// <summary>
		/// List of enqueued requests.
		/// </summary>
		private List<Request> enqueuedRequests;

		/// <summary>
		/// Initializes a new instance of the <see cref="Com.Plsr.ImageLoader.Network.NetworkPool"/> class.
		/// </summary>
		private NetworkPool() {
			this.activeRequests = new List<Request>();
			this.enqueuedRequests = new List<Request>();
		}

		/// <summary>
		/// Gets or sets the instance.
		/// </summary>
		/// <value>The instance.</value>
		public static NetworkPool Instance {
			get {			
				lock(_lock) {
					if (_instance == null) {
						_instance = new NetworkPool();
					}
					return _instance;
				}
			}
			set {
				_instance = value;
			}
		}

		/// <summary>
		/// Determines whether this instance can do a request.
		/// </summary>
		/// <returns><c>true</c> if this instance can do request; otherwise, <c>false</c>.</returns>
		public bool CanDoRequest() {
			return this.activeRequests.Count < Constants.MAX_ACTIVE_REQUESTS;
		}

		/// <summary>
		/// Adds an active request.
		/// </summary>
		/// <param name="request">Request.</param>
		public void AddActiveRequest(Request request) {
			this.activeRequests.Add(request);
		}

		/// <summary>
		/// Enqueues the request.
		/// </summary>
		/// <param name="request">Request.</param>
		public void EnqueueRequest(Request request) {
			this.enqueuedRequests.Add(request);
		}

		/// <summary>
		/// Dequeues any pending requests.
		/// </summary>
		private void DequeueRequests() {
			if (CanDoRequest() && this.enqueuedRequests.Count > 0) {
				int range = this.enqueuedRequests.Count - this.activeRequests.Count;
				if (range > 0) {
					if (range > Constants.MAX_ACTIVE_REQUESTS) {
						range = Constants.MAX_ACTIVE_REQUESTS;
					}
					List<Request> newActiveRequests = enqueuedRequests.GetRange(0, range);
					this.enqueuedRequests.RemoveRange(0, range);
					newActiveRequests.ForEach(request => NetworkManager.Instance.DoGet(request));
				}
			}
		}

		/// <summary>
		/// Removes the Request object from the active requests.
		/// </summary>
		/// <param name="request">Request.</param>
		public void RequestFinished(Request request) {
			this.activeRequests.Remove(request);
			DequeueRequests();
		}

		/// <summary>
		/// Restart this instance.
		/// </summary>
		public void Restart() {
			this.activeRequests.Clear();
			this.enqueuedRequests.Clear();
		}
	}

}