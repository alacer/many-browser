using UnityEngine;

namespace Com.Plsr.ImageLoader.Util {

	/// <summary>
	/// Unity singleton class.
	/// </summary>
	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {

		/// <summary>
		/// The instance object.
		/// </summary>
		private static T _instance;
		/// <summary>
		/// ock object for singleton creation.
		/// </summary>
		private static object _lock = new object();

		/// <summary>
		/// Gets or sets the instance.
		/// </summary>
		/// <value>The instance.</value>
		public static T Instance {
			get {
				if (applicationIsQuitting) {
					Object.DestroyImmediate(GameObject.Find("(singleton) "+ typeof(T).ToString()));
					_instance = null;
					applicationIsQuitting = false;
				}
				
				lock(_lock) {
					if (_instance == null) {
						_instance = (T) FindObjectOfType(typeof(T));
						if ( FindObjectsOfType(typeof(T)).Length > 1 ) {
							Debug.LogError("[Singleton] Something went really wrong " +
							               " - there should never be more than 1 singleton!" +
							               " Reopenning the scene might fix it.");
							return _instance;
						}
						if (_instance == null) {
							GameObject singleton = new GameObject();
							_instance = singleton.AddComponent<T>();
							singleton.name = "(singleton) "+ typeof(T).ToString();
							
							DontDestroyOnLoad(singleton);
							

						} else {

						}
					}
					return _instance;
				}
			}
			set {
				_instance = value;
			}
		}

		/// <summary>
		/// Variable to controll when the application is quiting.
		/// </summary>
		private static bool applicationIsQuitting = false;

		/// <summary>
		/// Raises the destroy event.
		/// </summary>
		public void OnDestroy () {
			applicationIsQuitting = true;
		}
	}

}