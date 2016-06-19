using System;
using System.Collections;
using System.Collections.Generic;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public class ChildrenControl : Control, IChildrenControl
    {
        private class ChildrenCollection : IList<Control>
        {
            public ChildrenCollection(ChildrenControl parent)
            {
                _parent = parent;
            }

            private readonly ChildrenControl _parent;
            private readonly List<Control> _internal = new List<Control>();

            public IEnumerator<Control> GetEnumerator()
            {
                return _internal.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _internal.GetEnumerator();
            }

            public void Add(Control item)
            {
                if (item.Parent != _parent)
                {
                    item.Parent = _parent;
                    return;
                }
                _internal.Add(item);
                _parent.Invalidate();
            }

            public void Clear()
            {
                var copy = _internal.ToArray();
                _internal.Clear();
                foreach (var item in copy)
                {
                    _parent.ChildRemoved?.Invoke(_parent, new ChildRemovedEventArgs(item));
                }
                _parent.Invalidate();
            }

            public bool Contains(Control item)
            {
                return _internal.Contains(item);
            }

            public void CopyTo(Control[] array, int arrayIndex)
            {
                _internal.CopyTo(array, arrayIndex);
            }

            public bool Remove(Control item)
            {
                if (!Contains(item)) return false;
                _internal.Remove(item);
                _parent.ChildRemoved?.Invoke(_parent, new ChildRemovedEventArgs(item));
                _parent.Invalidate();
                return true;
            }

            public int Count => _internal.Count;
            public bool IsReadOnly { get; } = false;

            public int IndexOf(Control item)
            {
                return _internal.IndexOf(item);
            }

            public void Insert(int index, Control item)
            {
                _internal.Insert(index, item);
            }

            public void RemoveAt(int index)
            {
                _internal.RemoveAt(index);
            }

            public Control this[int index]
            {
                get { return _internal[index]; }
                set { _internal[index] = value; }
            }
        }

        public ChildrenControl()
        {
            Children = new ChildrenCollection(this);
        }
        
        public virtual void OnChildrenUpdated() { }
        public virtual Size MeasureBoundingBox(Control child) => new Size(ActualWidth, ActualHeight);

        public IList<Control> Children { get; }
        public virtual IList<Control> FocusableChildren => Children;
        public event EventHandler<ChildRemovedEventArgs> ChildRemoved;
    }
}
