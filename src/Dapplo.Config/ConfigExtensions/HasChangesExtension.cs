using System.Collections.Generic;
using Dapplo.Config.Intercepting;
using Dapplo.Config.Interfaces;

namespace Dapplo.Config.ConfigExtensions
{
    internal class HasChangesExtension<TProperty> : IHasChanges, IConfigExtension<TProperty>
    {
        private readonly ConfigurationBase<TProperty> _parent;
        private bool _trackChanges;
        // This boolean has the value true if we have changes since the last "reset"
        private readonly ISet<string> _changedValues = new HashSet<string>(new AbcComparer());


        public HasChangesExtension(ConfigurationBase<TProperty> parent)
        {
            _parent = parent;
        }

        /// <inheritdoc />
        public int GetOrder { get; } = -1;

        /// <inheritdoc />
        public int SetOrder { get; } = (int)SetterOrders.HasChanges;

        /// <inheritdoc />
        public void Getter(GetInfo<TProperty> getInfo)
        {
            // This will not be called, as the getter order is negative
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void Setter(SetInfo<TProperty> setInfo)
        {
            if (!_trackChanges)
            {
                return;
            }

            if (!setInfo.HasOldValue || !Equals(setInfo.NewValue, setInfo.OldValue))
            {
                _changedValues.Add(setInfo.PropertyInfo.Name.ToLowerInvariant());
            }
        }

        /// <inheritdoc />
        public void TrackChanges()
        {
            _trackChanges = true;
        }

        /// <inheritdoc />
        public void DoNotTrackChanges()
        {
            _trackChanges = false;
        }

        /// <inheritdoc />
        public bool HasChanges()
        {
            return _changedValues.Count > 0;
        }

        /// <inheritdoc />
        public void ResetHasChanges()
        {
            _changedValues.Clear();
        }

        /// <inheritdoc />
        public ISet<string> Changes()
        {
            return new HashSet<string>(_changedValues, new AbcComparer());
        }

        /// <inheritdoc />
        public bool IsChanged(string propertyName)
        {
            return _changedValues.Contains(propertyName);
        }

    }
}
