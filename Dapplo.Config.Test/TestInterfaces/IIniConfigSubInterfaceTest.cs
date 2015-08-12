using System.ComponentModel;

namespace Dapplo.Config.Test.TestInterfaces {
	public interface IIniConfigSubInterfaceTest {
		string SubValue {
			get;
			set;
		}
		[DefaultValue("It works!")]
		string SubValuewithDefault {
			get;
			set;
		}
	}
}
