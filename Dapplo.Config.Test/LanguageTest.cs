using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapplo.Config.Test.TestInterfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dapplo.Config.Test {
	/// <summary>
	/// This test class tests the transactional capabilities of the proxy
	/// </summary>
	[TestClass]
	public class LanguageTest
	{
		private IPropertyProxy<ILanguageTest> _propertyProxy;

		[TestInitialize]
		public void Initialize()
		{
			_propertyProxy = ProxyBuilder.CreateProxy<ILanguageTest>();
		}

		[TestMethod]
		public void TestTransactionCommit() {
		}
	}
}
