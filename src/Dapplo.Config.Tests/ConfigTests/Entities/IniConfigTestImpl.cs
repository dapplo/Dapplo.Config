using AutoProperties;
using Dapplo.Config.Tests.ConfigTests.Interfaces;
using Dapplo.Ini.NewImpl;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Dapplo.Config.Tests.ConfigTests.Entities
{
    public class IniConfigTestImpl : IniSectionBase<IIniConfigTest>, IIniConfigTest
    {
        public long Age { get; set; }
        public IDictionary<string, IList<int>> DictionaryOfLists { get; set; }
        public string FirstName { get; set; }
        public uint Height { get; set; }
        public IDictionary<string, Uri> ListOfUris { get; set; }
        public Size MySize { get; set; }
        public string Name { get; set; }
        public string NotWritten { get; set; }
        public Rectangle PropertyArea { get; set; }
        public Size PropertySize { get; set; }
        public IDictionary<string, int> SomeValues { get; set; }
        public IList<IniConfigTestValues> TestEnums { get; set; }
        public IniConfigTestValues TestWithEnum { get; set; }
        public IList<int> WindowCornerCutShape { get; set; }
        public Uri[] MyUris { get; set; }
        public string SubValue { get; set; }
        public string SubValuewithDefault { get; set; }
        public IniConfigTestValues TestWithEnumSubValue { get; set; }

        [InterceptIgnore]
        public Action<IIniConfigTest> OnLoad { get; set; }
        public override void AfterLoad()
        {
            OnLoad?.Invoke(this);
        }
    }
}
