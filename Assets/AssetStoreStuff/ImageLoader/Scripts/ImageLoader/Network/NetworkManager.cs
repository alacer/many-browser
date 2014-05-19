using UnityEngine;
using System.Collections;
using System.Threading;
using System.Timers;
using Com.Plsr.ImageLoader.Util;
using Com.Plsr.ImageLoader.Model;
using Com.Plsr.ImageLoader.Configuration;

namespace Com.Plsr.ImageLoader.Network {

	/// <summary>
	/// Network manager class as singleton that handles HTTP requests.
	/// </summary>
	public class NetworkManager : Singleton<NetworkManager> {

		/// <summary>
		/// Manages if the current active requests has been canceled.
		/// </summary>
		private bool requestCanceled = false;
		/// <summary>
		/// Timer for timeouts.
		/// </summary>
		System.Timers.Timer timer;

		/// <summary>
		/// Initializes a new instance of the <see cref="Com.Plsr.ImageLoader.Network.NetworkManager"/> class.
		/// </summary>
		private NetworkManager() {
		}

		/// <summary>
		/// Does the request synchronously.
		/// </summary>
		/// <param name="request">Request object containing request info.</param>
		private void DoRequestSync(Request request) {
			request.Retries--;
			NetworkPool.Instance.AddActiveRequest(request);
			WWW www = new WWW(request.Url);
			while (!www.isDone && !this.requestCanceled) {}
			if (this.requestCanceled) {
				Debug.LogWarning ("Request for URL: " + request.Url + " has been cancelled because timeout for HTTP requests has been set to " + Constants.EDITOR_REQUEST_TIMEOUT + " milliseconds");
			}
			this.timer.Stop();
			this.timer = null;
			if (www.error == null && !requestCanceled) {
				ProcessRequestSucceeded(request, www);
			}
			else {
				ProcessRequestFailed(request, www);
			}
		}

		/// <summary>
		/// Does the request asynchronously.
		/// </summary>
		/// <param name="request">Request object containing request info.</param>
		private IEnumerator DoRequestAsync(Request request) {
			request.Retries--;
			NetworkPool.Instance.AddActiveRequest(request);
			WWW www = new WWW(request.Url);
			yield return www;
			if (www.error == null) {
				ProcessRequestSucceeded(request, www);
			}
			else {
				ProcessRequestFailed(request, www);
			}
		}

		/// <summary>
		/// Processes the request when it has succeeded.
		/// </summary>
		/// <param name="request">Request object.</param>
		/// <param name="www">WWW with the request result.</param>
		private void ProcessRequestSucceeded(Request request, WWW www) {
			NetworkPool.Instance.RequestFinished(request);
			if (request.Callback != null) {
				request.Callback(www, false);
			}
		}

		/// <summary>
		/// Processes the request when it has failed.
		/// </summary>
		/// <param name="request">Request object.</param>
		/// <param name="www">WWW with the request result.</param>
		private void ProcessRequestFailed(Request request, WWW www) {
			Debug.LogWarning("Error: " + www.error + " " + request.Id + " remaining: " + request.Retries);
			NetworkPool.Instance.RequestFinished(request);
			if (request.Retries > 0) {
				this.requestCanceled = false;
				DoGet(request);
			}
			else {
				Debug.Log("No more retries for request: " + request.Url);
				DoRequestCallback(request, www);
			}
		}

		/// <summary>
		/// Makes the callback for the given request
		/// </summary>
		/// <param name="request">Request object.</param>
		/// <param name="www">WWW with the request result.</param>
		private void DoRequestCallback(Request request, WWW www) {
			if (request.Callback != null) {
				request.Callback(www, this.requestCanceled);
			}
		}

		/// <summary>
		/// This method manages if the request should be sync or async and starts it.
		/// </summary>
		/// <param name="request">Request object.</param>
		public void DoGet(Request request) {
			if (NetworkPool.Instance.CanDoRequest()) {
				if (!Application.isPlaying) {
					this.timer = new System.Timers.Timer(Constants.EDITOR_REQUEST_TIMEOUT);
					timer.Enabled = true;
					timer.Elapsed += new ElapsedEventHandler(CancelRequest);
					DoRequestSync (request);
				} else {
					StartCoroutine (DoRequestAsync (request));
				}
			} else {
				NetworkPool.Instance.EnqueueRequest(request);
			}
		}

		/// <summary>
		/// Sets the requestCanceled to true in order to notify that request has been canceled.
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="e">ElapsedEventArgs.</param>
		private void CancelRequest(object source, ElapsedEventArgs e) {
			this.requestCanceled = true;
		}

		/// <summary>
		/// Restart this instance.
		/// </summary>
		public void Restart () {
			NetworkPool.Instance.Restart();
		}
	}

}