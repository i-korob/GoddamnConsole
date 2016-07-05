using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using GoddamnConsole.Controls;

namespace GoddamnConsole.DataBinding
{
    internal class Binding
    {
        private class BindingNode
        {
            public object Object { get; set; }
            public Type ObjectType { get; set; }
            public PropertyInfo Property { get; set; }
            public PropertyChangedEventHandler Handler { get; set; }
        }

        private readonly string _path;
        private readonly Control _control;
        private readonly PropertyInfo _property;
        
        private readonly List<BindingNode> _nodes = new List<BindingNode>();
        private readonly bool _strict;
        private readonly BindingMode _mode;

        public Binding(Control control, PropertyInfo property, string path, BindingMode mode, bool strict)
        {
            _path = path;
            _property = property;
            _control = control;
            _mode = mode;
            _strict = strict;
            if (mode == BindingMode.OneWayToSource || mode == BindingMode.TwoWay)
                control.PropertyChanged += OnTargetPropertyChanged;
            Refresh();
        }

        private void OnTargetPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var last = _nodes.LastOrDefault();
            if (last == null) return;
            last.Property.SetValue(last.Object, _property.GetValue(_control));
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var node = _nodes.FirstOrDefault(x => x.Object == sender && x.Property.Name == e.PropertyName);
            if (node != null) Refresh();
        }

        public void Cleanup(bool unbindTarget = false)
        {
            if (unbindTarget && (_mode == BindingMode.OneWayToSource || _mode == BindingMode.TwoWay))
                _control.PropertyChanged -= OnTargetPropertyChanged;
            foreach (var node in _nodes.Where(x => x.Handler != null))
            {
                node.ObjectType
                    .GetEvent(nameof(INotifyPropertyChanged.PropertyChanged))
                    .RemoveEventHandler(node.Object, node.Handler);
            }
            _nodes.Clear();
        }

        public void Refresh()
        {
            Cleanup();
            var data = _control.DataContext;
            try
            {
                foreach (var pathNode in _path.Split('.'))
                {
                    var type = data.GetType();
                    var property = type.GetProperty(pathNode);
                    if (property == null) throw new Exception("Invalid path");
                    var propValue = property.GetValue(data);
                    PropertyChangedEventHandler handler = null;
                    if (data is INotifyPropertyChanged)
                    {
                        handler = OnPropertyChanged;
                        (data as INotifyPropertyChanged).PropertyChanged += handler;
                    }
                    _nodes.Add(new BindingNode
                    {
                        Object = data,
                        Property = property,
                        Handler = handler,
                        ObjectType = type
                    });
                    data = propValue;
                }
                try
                {
                    if (_mode == BindingMode.OneWay || _mode == BindingMode.TwoWay)
                        _property.SetValue(_control, data);
                }
                catch
                {
                    if (_strict) throw; // invalid property value
                }
            }
            catch
            {
                Cleanup();
                if (_strict) throw; // invalid path
            }
        }
    }

    public enum BindingMode
    {
        OneWay,
        TwoWay,
        OneWayToSource
    }
}
