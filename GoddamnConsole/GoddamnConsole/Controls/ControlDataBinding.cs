using System;
using System.Collections.Generic;
using System.Reflection;
using GoddamnConsole.DataBinding;

namespace GoddamnConsole.Controls
{
    public abstract partial class Control
    {
        private readonly Dictionary<PropertyInfo, Binding> _bindings
            = new Dictionary<PropertyInfo, Binding>();

        private object _dataContext;
        
        public object DataContext
        {
            get { return _dataContext; }
            set
            {
                _dataContext = value;
                foreach (var binding in _bindings.Values) binding.Refresh();
            }
        }
        
        public void Bind(string propertyName, string bindingPath)
        {
            var property = GetType().GetProperty(propertyName);
            if (property == null) throw new ArgumentException("Property not found");
            Unbind(propertyName);
            _bindings.Add(property, new Binding(this, property, bindingPath));
        }

        public void Unbind(string propertyName)
        {
            var property = GetType().GetProperty(propertyName);
            Binding existingBinding;
            _bindings.TryGetValue(property, out existingBinding);
            if (existingBinding == null) return;
            existingBinding.Cleanup();
            _bindings.Remove(property);
        }
    }
}
