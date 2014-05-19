using UnityEngine;
using Com.Plsr.ImageLoader.Loader;

namespace Com.Plsr.ImageLoader.Configuration {

	/*! \mainpage Image Loader
	 *
	 * \section gettingstarted_sec Getting Started
	 *
	 * \subsection editorusage Usage from Editor
	 *
	 * Follow these steps:
	 * <ol>
	 * 	<li>Add the PreviewImageLoad script to your gameObject.
	 *  <li>Set up script parameters.
	 * 		<ul>
	 * 		<li>Source URL.
	 *		<li>GameObject aspect mode (FIT or FILL).
	 *		<li>Material index in which put the texture.
	 *		<li>Place holder texture.
	 *		<li>Texture name (_MainTex if empty).
	 *		</ul>
	 *	<li>Check/uncheck preview to preview the image in the gameObject without entering Play mode.
	 * </ol>
	 * 
	 * \subsection scriptusage Usage from C# script
	 * 
	 * Some examples of usage from script:
	 * 
	 * <pre>
	 * · ImageBuilder.Load("http://example.com/pulsar.png").Aspect(ImageBuilder.ASPECT_MODE.FIT).Into(this.gameObject, this.renderer.material);
	 * · ImageBuilder.Load("http://example.com/pulsar.png").Aspect(ImageBuilder.ASPECT_MODE.FILL).Into(this.gameObject, this.renderer.materials[2]);
	 * · ImageBuilder.Load("http://example.com/pulsar.png").Aspect(ImageBuilder.ASPECT_MODE.FILL).Into(this.gameObject, this.renderer.materials[4], "_MyTex");
	 * · ImageBuilder.Load("http://example.com/pulsar.png").Aspect(ImageBuilder.ASPECT_MODE.FIT).PlaceHolder(myTexture).Retries(5).TryFromCache(false).SaveInCache(false).Into(this.gameObject, this.renderer.material);
	 * · ImageBuilder.Load("http://example.com/pulsar.png").Aspect(ImageBuilder.ASPECT_MODE.FILL).Into(<b>MyCallback</b>);
	 * </pre>
	 * In this last case, MyCallback will be defined as follows:
	 * <pre>	
	 * private void <b>MyCallback</b>(WWW www) {
	 *		this.renderer.material.mainTexture = www.texture;
	 * }
	 * </pre>
	 */

	/// <summary>
	/// This class contains some variables relative to the image loader options.
	/// </summary>
	public class Constants {

		/// <summary>
		/// Number of concurrent active image loads.
		/// </summary>
		public static readonly int MAX_ACTIVE_LOADS = 1;
		/// <summary>
		/// Number of concurrent HTTP requests.
		/// </summary>
		public static readonly int MAX_ACTIVE_REQUESTS = 1;
		/// <summary>
		/// Sets if memory cache will be used to speed up image loading.
		/// </summary>
		public static readonly bool USE_MEMORY_CACHE = true;
		/// <summary>
		/// Path in which will be stored all the cache.
		/// </summary>
		public static readonly string PERSISTENT_DATA_PATH = Application.persistentDataPath;
		/// <summary>
		/// Cache folder name under persistent data path.
		/// </summary>
		public static readonly string CACHE_FOLDER_NAME = "cache";
		/// <summary>
		/// Max cache size (in bytes).
		/// </summary>
		public static readonly int PERSISTENCE_STORAGE_SIZE = 10485760; // bytes = 10 MB
		/// <summary>
		/// Name of the file which will contain info about cached images.
		/// </summary>
		public static readonly string PERSISTENCE_FILE_NAME = "cache.xml";
		/// <summary>
		/// Number of retries for HTTP requests.
		/// </summary>
		public static readonly int DEFAULT_HTTP_RETRIES = 3;
		/// <summary>
		/// Default aspect mode behaviour.
		/// </summary>
		public static readonly ImageBuilder.ASPECT_MODE DEFAULT_IMAGE_RESIZE = ImageBuilder.ASPECT_MODE.FIT;
		/// <summary>
		/// Timeout for requests in preview mode (milliseconds)
		/// </summary>
		public static readonly double EDITOR_REQUEST_TIMEOUT = 3000; // Tiempo de espera a que cargue la imagen en el editor
		/// <summary>
		/// Sets if image will be asked in cache before downloading.
		/// </summary>
		public static readonly bool TRY_FROM_CACHE_BY_DEFAULT = true;
		/// <summary>
		/// Sets if downloaded images will be cached.
		/// </summary>
		public static readonly bool SAVE_IN_CACHE_BY_DEFAULT = true;
		/// <summary>
		/// Default texture name to set when changing material texture.
		/// </summary>
		public static readonly string DEFAULT_TEXTURE_NAME = "_MainTex";

	}

}