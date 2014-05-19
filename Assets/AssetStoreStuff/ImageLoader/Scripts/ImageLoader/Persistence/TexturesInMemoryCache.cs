using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Plsr.ImageLoader.Persistence {

	/// <summary>
	/// Class to handle memory allocated images.
	/// </summary>
	public class TexturesInMemoryCache {

		/// <summary>
		/// List of in memory textures.
		/// </summary>
		private static Dictionary<string, WWW> cachedTextures = new Dictionary<string, WWW>();

		/// <summary>
		/// Initializes a new instance of the <see cref="Com.Plsr.ImageLoader.Persistence.TexturesInMemoryCache"/> class.
		/// </summary>
		private TexturesInMemoryCache() {
		}

		/// <summary>
		/// Sets the texture reference on cache.
		/// </summary>
		/// <param name="key">Key for texture.</param>
		/// <param name="texture">Texture data.</param>
		public static void SetOnCache(string key, WWW texture) {
			if (cachedTextures.ContainsKey(key)) {
				cachedTextures.Remove(key);
			}
			cachedTextures.Add(key, texture);
		}

		/// <summary>
		/// Gets the texture from cache with the given key.
		/// </summary>
		/// <returns>The texture if exists, null otherwise.</returns>
		/// <param name="key">Key associated to the texture.</param>
		public static WWW GetFromCache(string key) {
			if (cachedTextures.ContainsKey(key)) {
				return cachedTextures[key];
			}
			return null;
		}

		/// <summary>
		/// Clears the memory cache. Be careful because this destroys all textures loaded/downloaded through WWW.
		/// </summary>
		public static void ClearCache() {
			foreach (WWW texture in cachedTextures.Values) {
				UnityEngine.Object.Destroy(texture.texture);
			}
			cachedTextures.Clear();
			Resources.UnloadUnusedAssets();
		}

	}

}