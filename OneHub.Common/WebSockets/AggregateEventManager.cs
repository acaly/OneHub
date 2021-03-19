using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OneHub.Common.WebSockets
{
    internal sealed class AggregateEventManager<T>
    {
        private class DelegateWithInvocationList
        {
            public EventHandler<T> Delegate { get; }
            public Delegate[] InvocationList { get; }

            public DelegateWithInvocationList(EventHandler<T> d)
            {
                Delegate = d;
                InvocationList = d?.GetInvocationList() ?? Array.Empty<Delegate>();
            }
        }

        private DelegateWithInvocationList _delegate = null;

        public void Add(EventHandler<T> e)
        {
            DelegateWithInvocationList old, check;
            do
            {
                old = _delegate;
                check = Interlocked.CompareExchange(ref _delegate, new(old?.Delegate + e), old);
            } while (check != old);
        }

        public void Remove(EventHandler<T> e)
        {
            DelegateWithInvocationList old, check;
            do
            {
                old = _delegate;
                check = Interlocked.CompareExchange(ref _delegate, new(old?.Delegate - e), old);
            } while (check != old);
        }

        public void Invoke(object sender, T e)
        {
            List<Exception> list = null;
            foreach (var h in _delegate.InvocationList)
            {
                try
                {
                    ((EventHandler<T>)h)(sender, e);
                }
                catch (Exception ex)
                {
                    list ??= new();
                    list.Add(ex);
                }
            }
            if (list is not null)
            {
                if (list.Count == 1)
                {
                    throw list[0];
                }
                else
                {
                    throw new AggregateException(list);
                }
            }
        }
    }
}
