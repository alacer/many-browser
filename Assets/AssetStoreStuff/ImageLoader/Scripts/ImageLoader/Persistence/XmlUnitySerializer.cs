using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;

namespace Com.Plsr.ImageLoader.Persistence {

	/// <summary>
	/// Xml serializer class to save XML objects.
	/// </summary>
	public class XmlUnitySerializer<T> {

		/// <summary>
		/// Serializer object.
		/// </summary>
		private XmlSerializer serializer;
			
		/// <summary>
		/// Initializes a new instance of the <see cref="Com.Plsr.ImageLoader.Persistence.XmlUnitySerializer`1"/> class.
		/// </summary>
		public XmlUnitySerializer() {
			this.serializer = new XmlSerializer(typeof(T));
		}

		/// <summary>
		/// Loads an XML in the given path into an object.
		/// </summary>
		/// <returns>The representation of the XML file in the given T type object.</returns>
		/// <param name="path">Path of the XML file to load.</param>
		public T LoadContainer(string path) {
			if (!File.Exists(path)) {
				return default(T);
			}
			T obj = default(T);
			using(var stream = new FileStream(path, FileMode.Open)) {
				obj = ((T)serializer.Deserialize(stream));
				stream.Close();
			}
			
			return obj;
		}

		/// <summary>
		/// Serializes anobject into XML and saves it.
		/// </summary>
		/// <param name="obj">Object to save.</param>
		/// <param name="savePath">ath of the XML file.</param>
		public void SaveContainer(T obj, String savePath) {		
			using(var stream = new FileStream(savePath, FileMode.Create)) {
				serializer.Serialize(stream, obj);
				stream.Close();
			}
		}
	}

}