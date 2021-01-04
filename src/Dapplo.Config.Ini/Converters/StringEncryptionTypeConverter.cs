// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Dapplo.Config.Ini.Converters
{
	/// <summary>
	///     This converter makes it possible to store values in an ini file, somewhat secure.
	///     Please make sure to initialize the RgbKey/RgbIv before it is used.
	/// </summary>
	public class StringEncryptionTypeConverter : TypeConverter
	{
		/// <summary>
		///     The constructor validates all settings
		/// </summary>
		public StringEncryptionTypeConverter()
		{
			if (RgbKey is null || RgbIv is null)
			{
				throw new InvalidOperationException("Please make sure the StringEncryptionTypeConverter.RgbKey & RgbIv are set!");
			}
			var currentKeySize = RgbKey.Length*8;
			var validKeySizes = ValidKeySizes();
			if (validKeySizes.Contains(currentKeySize))
			{
				return;
			}
			var keySizes = string.Join(",", validKeySizes);
			throw new InvalidOperationException($"Bit-Length of StringEncryptionTypeConverter.RgbKey {currentKeySize} is invalid, valid bit sizes are: {keySizes}");
		}

		/// <summary>
		///     The algorithm to use for the encrypt/decrypt, default is Rijndael
		/// </summary>
		public static string Algorithm { get; set; } = "Rijndael";

		/// <summary>
		///     The initialization vector to use for the symmetric algorithm.
		/// </summary>
		public static string RgbIv { get; set; }

		/// <summary>
		///     The secret key to use for the symmetric algorithm.
		/// </summary>
		public static string RgbKey { get; set; }

		/// <summary>
		///     Can we convert from? As we decrypt from a string, the sourceType needs to be string
		/// </summary>
		/// <param name="context">ITypeDescriptorContext</param>
		/// <param name="sourceType">Type</param>
		/// <returns>bool</returns>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
			{
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}

		/// <summary>
		///     Can we convert to? As we create a string, the destinationType needs to be string
		/// </summary>
		/// <param name="context">ITypeDescriptorContext</param>
		/// <param name="destinationType">Type</param>
		/// <returns>bool</returns>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				return true;
			}
			return base.CanConvertTo(context, destinationType);
		}

		/// <summary>
		///     To decrypt call this
		/// </summary>
		/// <param name="context">ITypeDescriptorContext</param>
		/// <param name="culture">CultureInfo</param>
		/// <param name="value">object</param>
		/// <returns>object</returns>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is null)
			{
				return null;
			}
			var valueString = value as string;
			if (!string.IsNullOrWhiteSpace(valueString))
			{
				// Try to decrypt, ignore FormatException
				try
				{
					return Decrypt(valueString);
				}
				catch (FormatException)
				{
					return valueString;
				}
			}
			if (valueString != null)
			{
				return valueString;
			}

			return base.ConvertFrom(context, culture, value);
		}

		/// <summary>
		///     To encrypt call this
		/// </summary>
		/// <param name="context"></param>
		/// <param name="culture"></param>
		/// <param name="value"></param>
		/// <param name="destinationType"></param>
		/// <returns></returns>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType != typeof(string))
			{
				return base.ConvertTo(context, culture, value, destinationType);
			}
			var plaintext = (string) value;
			if (string.IsNullOrEmpty(plaintext))
			{
				return null;
			}
			return Encrypt(plaintext);
		}

		/// <summary>
		///     A simply decryption, can be used to store passwords
		/// </summary>
		/// <param name="encryptedText">a base64 encoded encrypted string</param>
		/// <returns>Decrypeted text</returns>
		private static string Decrypt(string encryptedText)
		{
			string returnValue;
			var encryptedTextBytes = Convert.FromBase64String(encryptedText);

			using (var symmetricAlgorithm = SymmetricAlgorithm.Create(Algorithm))
			{
                var memoryStream = new MemoryStream();
				var rgbIv = Encoding.ASCII.GetBytes(RgbIv);
				var key = Encoding.ASCII.GetBytes(RgbKey);

				using (var cryptoStream = new CryptoStream(memoryStream, symmetricAlgorithm.CreateDecryptor(key, rgbIv), CryptoStreamMode.Write))
				{
					cryptoStream.Write(encryptedTextBytes, 0, encryptedTextBytes.Length);
					cryptoStream.FlushFinalBlock();
					returnValue = Encoding.ASCII.GetString(memoryStream.ToArray());
				}
			}

			return returnValue;
		}

		/// <summary>
		///     A simply encryption, can be used to store passwords
		/// </summary>
		/// <param name="clearText">the string to call upon</param>
		/// <returns>an encryped string in base64 form</returns>
		private static string Encrypt(string clearText)
		{
			string returnValue;
			var clearTextBytes = Encoding.ASCII.GetBytes(clearText);
			using (var symmetricAlgorithm = SymmetricAlgorithm.Create(Algorithm))
			{
				var memoryStream = new MemoryStream();
				var rgbIv = Encoding.ASCII.GetBytes(RgbIv);
				var key = Encoding.ASCII.GetBytes(RgbKey);
				using (var cryptoStream = new CryptoStream(memoryStream, symmetricAlgorithm.CreateEncryptor(key, rgbIv), CryptoStreamMode.Write))
				{
					cryptoStream.Write(clearTextBytes, 0, clearTextBytes.Length);
					cryptoStream.FlushFinalBlock();
					returnValue = Convert.ToBase64String(memoryStream.ToArray());
				}
			}
			return returnValue;
		}

		private static IList<int> ValidKeySizes()
		{
			IList<int> validSizes = new List<int>();
			using (var symmetricAlgorithm = SymmetricAlgorithm.Create(Algorithm))
			{
				foreach (var keySize in symmetricAlgorithm.LegalKeySizes)
				{
					for (var valueSize = keySize.MinSize; valueSize <= keySize.MaxSize; valueSize += keySize.SkipSize)
					{
						validSizes.Add(valueSize);
					}
				}
			}
			return validSizes;
		}
	}
}