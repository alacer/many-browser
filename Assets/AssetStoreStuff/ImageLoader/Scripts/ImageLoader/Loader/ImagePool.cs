using System;
using System.Collections.Generic;
using Com.Plsr.ImageLoader.Configuration;

namespace Com.Plsr.ImageLoader.Loader {

	/// <summary>
	/// Image pool class that handles active and pending image loads.
	/// </summary>
	public class ImagePool {

		/// <summary>
		/// Singleton instance.
		/// </summary>
		private static ImagePool _instance;
		/// <summary>
		/// Lock object for singleton creation.
		/// </summary>
		private static object _lock = new object();

		/// <summary>
		/// List of active loads.
		/// </summary>
		private List<ImageBuilder> activeLoads;
		/// <summary>
		/// List of enqueued loads.
		/// </summary>
		private List<ImageBuilder> enqueuedLoads;

		/// <summary>
		/// Initializes a new instance of the <see cref="Com.Plsr.ImageLoader.Loader.ImagePool"/> class.
		/// </summary>
		private ImagePool() {
			this.activeLoads = new List<ImageBuilder>();
			this.enqueuedLoads = new List<ImageBuilder>();
		}

		/// <summary>
		/// Gets or sets the singleton instance.
		/// </summary>
		/// <value>The instance.</value>
		public static ImagePool Instance {
			get {			
				lock(_lock) {
					if (_instance == null) {
						_instance = new ImagePool();
					}
					return _instance;
				}
			}
			set {
				_instance = value;
			}
		}

		/// <summary>
		/// Determines whether a new image load can be done.
		/// </summary>
		/// <returns><c>true</c> if image load can be done.</returns>
		public bool CanDoRequest() {
			return this.activeLoads.Count < Constants.MAX_ACTIVE_LOADS;
		}

		/// <summary>
		/// Adds the active load.
		/// </summary>
		/// <param name="load">ImageBuilder reference to be added to the active image loads.</param>
		public void AddActiveLoad(ImageBuilder load) {
			this.activeLoads.Add(load);
		}

		/// <summary>
		/// Enqueues the load.
		/// </summary>
		/// <param name="load">ImageBuilder reference to be added to the enqueued image loads.</param>
		public void EnqueueLoad(ImageBuilder load) {
			this.enqueuedLoads.Add(load);
		}

		/// <summary>
		/// Dequeues any enqueued loads.
		/// </summary>
		private void DequeueLoads() {
			if (CanDoRequest() && this.enqueuedLoads.Count > 0) {
				int range = this.enqueuedLoads.Count - this.activeLoads.Count;
				if (range > 0) {
					if (range > Constants.MAX_ACTIVE_LOADS) {
						range = Constants.MAX_ACTIVE_LOADS;
					}
					List<ImageBuilder> newActiveLoads = enqueuedLoads.GetRange(0, range);
					this.enqueuedLoads.RemoveRange(0, range);
					newActiveLoads.ForEach(imageBuilder => imageBuilder.ExecuteOrEnqueue());
				}
			}
		}

		/// <summary>
		/// Removes the ImageBuilder object from the active loads.
		/// </summary>
		/// <param name="load">ImageBulider reference to the finished image load.</param>
		public void LoadFinished(ImageBuilder load) {
			this.activeLoads.Remove(load);
			this.DequeueLoads();
		}

		/// <summary>
		/// Restart this instance.
		/// </summary>
		public void Restart() {
			this.activeLoads.Clear();
			this.enqueuedLoads.Clear();
		}

	}

}