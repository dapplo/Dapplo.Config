using Dapplo.Config.Tests.ConfigTests.Interfaces;
using Dapplo.Ini;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Dapplo.Config.Tests.ConfigTests.Entities
{
    public class IniConfigTestImpl : DictionaryConfigurationBase<IIniConfigTest>, IIniConfigTest
    {
        public long Age { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IDictionary<string, IList<int>> DictionaryOfLists { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string FirstName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public uint Height { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IDictionary<string, Uri> ListOfUris { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Size MySize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string NotWritten { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Rectangle PropertyArea { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Size PropertySize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IDictionary<string, int> SomeValues { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IList<IniConfigTestValues> TestEnums { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IniConfigTestValues TestWithEnum { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IList<int> WindowCornerCutShape { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Uri[] MyUris { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string SubValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string SubValuewithDefault { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IniConfigTestValues TestWithEnumSubValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void AfterLoad()
        {
            throw new NotImplementedException();
        }

        public void AfterSave()
        {
            throw new NotImplementedException();
        }

        public void BeforeSave()
        {
            throw new NotImplementedException();
        }

        public IniValue GetIniValue(string propertyName)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyDictionary<string, IniValue> GetIniValues()
        {
            throw new NotImplementedException();
        }

        public string GetSectionDescription()
        {
            throw new NotImplementedException();
        }

        public string GetSectionName()
        {
            throw new NotImplementedException();
        }

        public bool TryGetIniValue(string propertyName, out IniValue value)
        {
            throw new NotImplementedException();
        }
    }
}
