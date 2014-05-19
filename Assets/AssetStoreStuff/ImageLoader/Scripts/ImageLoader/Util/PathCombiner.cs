using System;
using System.IO;
using System.Collections;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Com.Plsr.ImageLoader.Util {

	/// <summary>
	/// This is a class that helps combining string paths.
	/// </summary>
	public class PathCombiner {

		/// <summary>
		/// Initializes a new instance of the <see cref="Com.Plsr.ImageLoader.Util.PathCombiner"/> class.
		/// </summary>
		private PathCombiner () {
		}

		/// <summary>
		/// Combine the specified paths into one string.
		/// </summary>
		/// <param name="paths">Paths.</param>
		public static string Combine(params string[] paths) {
			if (paths.Length > 1) {
				string firstItem = paths[0];
				List<string> newList = paths.ToList();
				newList.RemoveAt(0);
				newList[0] = Path.Combine(firstItem, newList[0]).Replace("\\", "/");
				return Combine(newList.ToArray());
			}
			else {
				if (paths.Length > 0) {
					return paths[0];
				}
				else {
					return "";
				}
			}
		}
		
	}

}