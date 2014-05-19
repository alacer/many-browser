using UnityEngine;
using System.Collections;
using Com.Plsr.ImageLoader.Configuration;

namespace Com.Plsr.ImageLoader.Loader {

	/// <summary>
	/// Preview Image load class to preview in Editor.
	/// </summary>
	[ExecuteInEditMode]
	public class PreviewImageLoad : MonoBehaviour {

		public string url;
		public ImageBuilder.ASPECT_MODE gameObjectAspectMode;
		public int materialIndex;
		public bool preview;
		public Texture2D placeHolderTexture;
		public string textureName;
		private bool previewing = false;
		private bool skip = false;
		[HideInInspector]
		public Material[] originalMaterials;
		[HideInInspector]
		public Vector3 originalScale;

		/// <summary>
		/// Gets the name of the texture.
		/// </summary>
		/// <value>The name of the texture.</value>
		public string TextureName {
			get {
				if (this.textureName == null || this.textureName.Trim().Equals("")) {
					return Constants.DEFAULT_TEXTURE_NAME;
				}
				return this.textureName;
			}
		}

		/// <summary>
		/// Starts image loading process if gameobject containing this script has any materials.
		/// </summary>
		void Awake() {
			this.skip = true;
			if (Application.isPlaying) {
				if (this.url != null && materialIndex >= 0 && materialIndex < this.renderer.materials.Length) {
					ImageBuilder.Load(this.url).Aspect(this.gameObjectAspectMode).PlaceHolder(this.placeHolderTexture).Into(this.gameObject, this.renderer.materials [materialIndex], this.TextureName);
				}
			}
		}

		#if UNITY_EDITOR

		/// <summary>
		/// Starts the preview when user clicks the "Preview" check in Editor.
		/// </summary>
		private void StartPreview () {
			this.originalMaterials = this.renderer.sharedMaterials;
			if (this.url != null && materialIndex >= 0 && materialIndex < this.renderer.sharedMaterials.Length) {
				this.originalScale = this.transform.localScale;
				Material[] newSharedMaterials = new Material[originalMaterials.Length];
				Material useMaterial = null;
				int i = 0;
				foreach (Material mat in originalMaterials) {
					if (mat != null) {
						newSharedMaterials[i] = new Material(mat);
					}
					else {
						newSharedMaterials[i] = null;
					}
					if (i == materialIndex) {
						useMaterial = newSharedMaterials[i];
					}
					i++;
				}
				this.renderer.sharedMaterials = newSharedMaterials;
				ImageBuilder.Load(this.url).Aspect(this.gameObjectAspectMode).PlaceHolder(this.placeHolderTexture).Into(this.gameObject, useMaterial, this.TextureName);
			}
		}

		/// <summary>
		/// Stops the preview.
		/// </summary>
		private void StopPreview() {
			this.renderer.sharedMaterials = this.originalMaterials;
			this.transform.localScale = this.originalScale;
		}

		/// <summary>
		/// Handles when the user clicks "Preview".
		/// </summary>
		void Update () {
			if (!Application.isPlaying) {
				if (skip) {
					skip = false;
					if (preview) {
						preview = false;
						previewing = false;
						//this.transform.localScale = originalScale;
						StopPreview();
					}
					return;
				}
				if (!previewing && preview) {
					StartPreview();
					previewing = true;
				}
				else if (previewing && !preview) {
					previewing = false;
					StopPreview();
				}
			}
		}

		void OnApplicationQuit() {
			//StopPreview();
		}

		#endif
	}

}