using System;
using System.ComponentModel;
using System.Reflection;
using GoddamnConsole.Controls;

namespace GoddamnConsole.DataBinding
{
    internal class Binding : IDisposable
    {
        private readonly string _path;
        private readonly BindingMode _mode;
        private readonly Control _control;
        private readonly PropertyInfo _property;
        private object _sourceObject;
        private PropertyChangedEventHandler _handler;

        public Binding(Control control, PropertyInfo property, string path, BindingMode mode)
        {
            _path = path;
            _mode = mode;
            _property = property;
            _control = control;
            Refresh();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var lastDot = _path.LastIndexOf('.');
            var last = lastDot == -1 ? _path : _path.Substring(lastDot + 1);
            if (e.PropertyName == last)
            {
                try
                {
                    _property.SetValue(_control,
                                       _sourceObject.GetType()
                                                    .GetProperty(e.PropertyName)
                                                    .GetValue(_sourceObject));
                }
                catch
                {
                    // invalid cast, ignore
                }
            }
        }

        public void Dispose()
        {

        }

        public void Refresh()
        {
            if (_sourceObject != null)
            {
                var objType = _sourceObject.GetType();
                if (objType.GetInterface(nameof(INotifyPropertyChanged)) != null)
                    objType.GetEvent(nameof(INotifyPropertyChanged.PropertyChanged))
                           .RemoveEventHandler(_sourceObject,
                                               _handler);
                _sourceObject = null;
            }
            var path = _path;
            var dataContext = _control.DataContext;
            while (true)
            {
                var dot = path.IndexOf('.');
                if (dot < 0)
                {
                    var last = dataContext.GetType().GetProperty(path);
                    if (last == null)
                    {
                        return;
                    }
                    _sourceObject = dataContext;
                    _sourceObject.GetType().GetEvent(nameof(INotifyPropertyChanged.PropertyChanged)).AddEventHandler(_sourceObject, _handler = new PropertyChangedEventHandler(OnPropertyChanged));
                    return;
                }
                var current = path.Remove(dot);
                path = path.Substring(dot + 1);
                var prop = dataContext.GetType().GetProperty(current);
                if (prop == null)
                {
                    return;
                }
                dataContext = prop.GetValue(dataContext);
            }
        }
    }

    public enum BindingMode
    {
        OneWay,
        // TwoWay,
        // OneWayToSource,
        // OneTime
    }
}
