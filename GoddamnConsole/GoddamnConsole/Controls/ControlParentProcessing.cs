using System;

namespace GoddamnConsole.Controls
{
    public abstract partial class Control
    {
        private ParentControl _parent;

        [AlsoNotifyFor(nameof(ActualWidth))]
        [AlsoNotifyFor(nameof(ActualHeight))]
        [AlsoNotifyFor(nameof(ActualVisibility))]
        public virtual ParentControl Parent
        {
            get { return _parent; }
            set
            {
                var prev = _parent;
                _parent = value;
                try
                {
                    if (value == null)
                    {
                        RemoveFromParent();
                        _parent = null;
                        return;
                    }
                    var cctl = value as IContentControl;
                    if (cctl != null && cctl.Content != this)
                    {
                        cctl.ContentDetached += OnDetach;
                        cctl.Content = this;
                    }
                    else
                    {
                        var pctl = value as IChildrenControl;
                        if (pctl != null)
                        {
                            pctl.ChildRemoved += OnDetach;
                            pctl.Children.Add(this);
                        }
                        else throw new NotSupportedException("Unsupported IParentControl");
                    }
                }
                catch
                {
                    _parent = prev;
                    throw;
                }
                OnPropertyChanged();
            }
        }

        private void OnDetach(object sender, ChildRemovedEventArgs args)
        {
            var ctrl = args?.Child;
            if (sender != _parent || ctrl != this) return;
            var cctl = sender as IContentControl;
            if (cctl != null && cctl.Content != this)
            {
                cctl.ContentDetached -= OnDetach;
            }
            else
            {
                var pctl = sender as IChildrenControl;
                if (pctl != null)
                {
                    pctl.ChildRemoved -= OnDetach;
                }
                else throw new Exception("Unknown IParentControl");
            }
            try
            {
                DetachedFromParent?.Invoke(this, EventArgs.Empty);
            }
            catch
            {
                /* */
            }
        }

        private void RemoveFromParent()
        {
            var cctl = _parent as IContentControl;
            if (cctl != null && cctl.Content != this)
            {
                cctl.Content = null;
            }
            else
            {
                var pctl = _parent as IChildrenControl;
                if (pctl != null)
                {
                    pctl.Children.Remove(this);
                }
                else throw new Exception("Unknown IParentControl");
            }
            try
            {
                DetachedFromParent?.Invoke(this, EventArgs.Empty);
            }
            catch
            {
                /* */
            }
        }

        public event EventHandler DetachedFromParent;
    }
}
