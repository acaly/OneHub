using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OneHub.Common.Definitions
{
    //TODO currently InterfaceBuilder uses this class for its events, but the returned Task is not awaitted.
    //We may need to log the error at the end instead of throwing it (no one will catch it).
    //See event dispatcher's HandleRecv method (for example, in OneXEventDispatcher).
    public class AsyncEventManager<T>
    {
        private class DelegateWithInvocationList
        {
            public AsyncEventHandler<T> Delegate { get; }
            public Delegate[] InvocationList { get; }

            public DelegateWithInvocationList(AsyncEventHandler<T> d)
            {
                Delegate = d;
                InvocationList = d?.GetInvocationList() ?? Array.Empty<Delegate>();
            }
        }

        private DelegateWithInvocationList _delegate = null;

        public void Add(AsyncEventHandler<T> e)
        {
            DelegateWithInvocationList old, check;
            do
            {
                old = _delegate;
                check = Interlocked.CompareExchange(ref _delegate, new(old?.Delegate + e), old);
            } while (check != old);
        }

        public void Remove(AsyncEventHandler<T> e)
        {
            DelegateWithInvocationList old, check;
            do
            {
                old = _delegate;
                check = Interlocked.CompareExchange(ref _delegate, new(old?.Delegate - e), old);
            } while (check != old);
        }

        public async Task InvokeAsync(object sender, T e)
        {
            List<Exception> list = null;
            var delegateObj = _delegate;
            if (delegateObj is null) return;
            foreach (var h in delegateObj.InvocationList)
            {
                try
                {
                    await ((AsyncEventHandler<T>)h)(sender, e);
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
