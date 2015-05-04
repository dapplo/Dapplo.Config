/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2015 Robin Krom
 * 
 * For more information see: http://dapplo.net/
 * dapplo repositories are hosted on GitHub: https://github.com/dapplo
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Dapplo.Config.Ini {
	/// <summary>
	/// This converter makes it possible to store values in an ini file, somewhat secure.
	/// Please make sure to initialize the RgbKey/RgbIv before it is used.
	/// </summary>
	public class StringEncryptionTypeConverter : TypeConverter {
		/// <summary>
		/// The secret key to use for the symmetric algorithm.
		/// </summary>
		public static string RgbKey {
			get;
			set;
		}

		/// <summary>
		/// The initialization vector to use for the symmetric algorithm.
		/// </summary>
		public static string RgbIv {
			get;
			set;
		}

		private static string _algorithm = "Rijndael";
		/// <summary>
		/// The algorithm to use for the encrypt/decrypt, default is Rijndael
		/// </summary>
		public static string Algorithm {
			get {
				return _algorithm;
			}
			set {
				_algorithm = value;
			}
		}

		private static IList<int> ValidKeySizes() {
			IList<int> validSizes = new List<int>();
			using (SymmetricAlgorithm symmetricAlgorithm = SymmetricAlgorithm.Create(Algorithm)) {
				foreach (var keySize in symmetricAlgorithm.LegalKeySizes) {
					for (int valueSize = keySize.MinSize; valueSize <= keySize.MaxSize; valueSize += keySize.SkipSize) {
						validSizes.Add(valueSize);
					}
				}
			}
			return validSizes;
		}

		/// <summary>
		/// The constructor validates all settings
		/// </summary>
		public StringEncryptionTypeConverter() {
			if (RgbKey == null || RgbIv == null) {
				throw new InvalidOperationException("Please make sure the StringEncryptionTypeConverter.RgbKey & RgbIv are set!");
			}
			var currentKeySize = RgbKey.Length * 8;
			var validKeySizes = ValidKeySizes();
			if (!validKeySizes.Contains(currentKeySize)) {
				throw new InvalidOperationException(string.Format("Bit-Length of StringEncryptionTypeConverter.RgbKey {0} is invalid, valid bit sizes are: {1}", currentKeySize, string.Join(",", validKeySizes)));
			}
		}

		/// <summary>
		/// Can we convert to? As we create a string, the destinationType needs to be string
		/// </summary>
		/// <param name="context"></param>
		/// <param name="destinationType"></param>
		/// <returns></returns>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			if (destinationType == typeof(string)) {
				return true;
			}
			return base.CanConvertTo(context, destinationType);
		}

		/// <summary>
		/// Can we convert from? As we decrypt from a string, the sourceType needs to be string
		/// </summary>
		/// <param name="context"></param>
		/// <param name="sourceType"></param>
		/// <returns></returns>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
			if (sourceType == typeof(string)) {
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}

		/// <summary>
		/// To encrypt call this
		/// </summary>
		/// <param name="context"></param>
		/// <param name="culture"></param>
		/// <param name="value"></param>
		/// <param name="destinationType"></param>
		/// <returns></returns>
		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) {
			if (destinationType == typeof(string)) {
				string plaintext = (string)value;
				if (string.IsNullOrEmpty(plaintext)) {
					return null;
				}
				return Encrypt(plaintext);
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}

		/// <summary>
		/// To decrypt call this
		/// </summary>
		/// <param name="context"></param>
		/// <param name="culture"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
			if (value == null) {
				return null;
			}
			string valueString = value as string;
			if (!string.IsNullOrEmpty(valueString)) {
				return Decrypt(valueString);
			}

			return base.ConvertFrom(context, culture, value);
		}

		/// <summary>
		/// A simply encryption, can be used to store passwords
		/// </summary>
		/// <param name="clearText">the string to call upon</param>
		/// <returns>an encryped string in base64 form</returns>
		private static string Encrypt(string clearText) {
			string returnValue = clearText;
			byte[] clearTextBytes = Encoding.ASCII.GetBytes(clearText);
			using (SymmetricAlgorithm symmetricAlgorithm = SymmetricAlgorithm.Create(Algorithm))
			using (MemoryStream memoryStream = new MemoryStream()) {
				byte[] rgbIV = Encoding.ASCII.GetBytes(RgbIv);
				byte[] key = Encoding.ASCII.GetBytes(RgbKey);
				CryptoStream cryptoStream = new CryptoStream(memoryStream, symmetricAlgorithm.CreateEncryptor(key, rgbIV), CryptoStreamMode.Write);

				cryptoStream.Write(clearTextBytes, 0, clearTextBytes.Length);
				cryptoStream.Flush();
				cryptoStream.Close();
				returnValue = Convert.ToBase64String(memoryStream.ToArray());
			}
			return returnValue;
		}

		/// <summary>
		/// A simply decryption, can be used to store passwords
		/// </summary>
		/// <param name="encryptedText">a base64 encoded encrypted string</param>
		/// <returns>Decrypeted text</returns>
		private static string Decrypt(string encryptedText) {
			string returnValue = encryptedText;
			byte[] encryptedTextBytes = Convert.FromBase64String(encryptedText);

			using (SymmetricAlgorithm symmetricAlgorithm = SymmetricAlgorithm.Create(Algorithm))
			using (MemoryStream memoryStream = new MemoryStream()) {

				byte[] rgbIV = Encoding.ASCII.GetBytes(RgbIv);
				byte[] key = Encoding.ASCII.GetBytes(RgbKey);

				CryptoStream cryptoStream = new CryptoStream(memoryStream, symmetricAlgorithm.CreateDecryptor(key, rgbIV), CryptoStreamMode.Write);

				cryptoStream.Write(encryptedTextBytes, 0, encryptedTextBytes.Length);
				cryptoStream.Flush();
				cryptoStream.Close();
				returnValue = Encoding.ASCII.GetString(memoryStream.ToArray());
			}

			return returnValue;
		}

	}
}
