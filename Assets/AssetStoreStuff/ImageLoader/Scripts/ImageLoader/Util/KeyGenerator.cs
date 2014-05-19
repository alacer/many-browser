using System;
using System.Security.Cryptography;
using System.Text;

namespace Com.Plsr.ImageLoader.Util {

	/// <summary>
	/// This class generate keys for given strings.
	/// </summary>
	public class KeyGenerator {
			
		/// <summary>
		/// Initializes a new instance of the <see cref="Com.Plsr.ImageLoader.Util.KeyGenerator"/> class.
		/// </summary>
		private KeyGenerator () {
		}

		/// <summary>
		/// Gets the key for a given string.
		/// </summary>
		/// <returns>The key.</returns>
		/// <param name="name">String to generate a key for.</param>
		public static string getKeyFor(string name) {
			return CalculateSHA1(name, Encoding.UTF8);
		}

		/// <summary>
		/// Calculates the SHA1 for a given string and encoding.
		/// </summary>
		/// <returns>Tha SHA1 hash.</returns>
		/// <param name="text">String to hash.</param>
		/// <param name="enc">Encoding type.</param>
		private static string CalculateSHA1(string text, Encoding enc) {
			byte[] buffer = enc.GetBytes(text);
			SHA1CryptoServiceProvider cryptoTransformSHA1 = new SHA1CryptoServiceProvider();  
			string hash = BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).Replace("-", "");
			return hash;
		}

	}

}