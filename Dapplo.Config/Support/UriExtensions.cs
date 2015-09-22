using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace Dapplo.Config.Support
{
	public static class UriExtensions
	{
		/// <summary>
		/// Create a NameValueCollection from the query part of the uri
		/// </summary>
		/// <param name="uri"></param>
		/// <returns>NameValueCollection</returns>
		public static NameValueCollection QueryToNameValues(this Uri uri)
		{
			if (!string.IsNullOrEmpty(uri.Query))
			{
				return HttpUtility.ParseQueryString(uri.Query);
			}
			return new NameValueCollection();
		}

		/// <summary>
		/// Create a IDictionary string,string from the query part of the uri
		/// </summary>
		/// <param name="uri"></param>
		/// <returns>IDictionary string,string</returns>
		public static IDictionary<string, string> QueryToDictionary(this Uri uri)
		{
			var returnValue = new Dictionary<string, string>();
			var nameValues = uri.QueryToNameValues();
			for(int i=0; i<nameValues.Count; i++) {
				var value = nameValues[i];
				var key = nameValues.AllKeys[i];
                if (returnValue.ContainsKey(key))
				{
					returnValue[key] = value;
				}
				else
				{
					returnValue.Add(key, value);
				}
			}
			return returnValue;
		}
	}
}
