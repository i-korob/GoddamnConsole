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

        public Binding(Control control, PropertyInfo property, string path)
        {
            _path = path;
            _property = property;
            _control = control;
            Refresh();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var node = _nodes.FirstOrDefault(x => x.Object == sender && x.Property.Name == e.PropertyName);
            if (node != null) Refresh();
        }

        public void Cleanup()
        {
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
                    _property.SetValue(_control, data);
                }
                catch
                {
                    // invalid property value
                }
            }
            catch
            {
                Cleanup();
                // throw; // invalid path
            }
        }
    }
}
