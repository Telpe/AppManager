using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AppManager.OsApi.Events
{
    public class ObservableEvent<TSender, TArgs>
    {
        private event TypedEvent<TSender, TArgs>? HandlersValue;
        private int CountValue;

        public event TypedEvent<object?, int>? CountChangedEvent;

        public int Count { get => CountValue; }

        public static ObservableEvent<TSender, TArgs> operator +(
            ObservableEvent<TSender, TArgs> evt,
            TypedEvent<TSender, TArgs> handler)
        {
            // 1. Delegate combine via CompareExchange-loop
            TypedEvent<TSender, TArgs>? original;
            TypedEvent<TSender, TArgs>? updated;

            do
            {
                original = evt.HandlersValue;
                updated = (TypedEvent<TSender, TArgs>?)Delegate.Combine(original, handler);
            }
            while (Interlocked.CompareExchange(ref evt.HandlersValue, updated, original) != original);

            // 2. Atomar increment
            Interlocked.Increment(ref evt.CountValue);
            evt.CountChangedEvent?.Invoke(evt, evt.CountValue);

            return evt;
        }

        public static ObservableEvent<TSender, TArgs> operator -(
            ObservableEvent<TSender, TArgs> evt,
            TypedEvent<TSender, TArgs> handler)
        {
            // 1. Delegate remove via CompareExchange-loop
            TypedEvent<TSender, TArgs>? original;
            TypedEvent<TSender, TArgs>? updated;

            do
            {
                original = evt.HandlersValue;
                updated = (TypedEvent<TSender, TArgs>?)Delegate.Remove(original, handler);
            }
            while (Interlocked.CompareExchange(ref evt.HandlersValue, updated, original) != original);

            // 2. Atomar decrement
            Interlocked.Decrement(ref evt.CountValue);
            evt.CountChangedEvent?.Invoke(evt, evt.CountValue);

            return evt;
        }

        public void Invoke(TSender sender, TArgs args)
            => HandlersValue?.Invoke(sender, args);
    }

}
