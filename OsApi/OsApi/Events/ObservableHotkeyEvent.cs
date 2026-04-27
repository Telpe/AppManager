using System;
using System.Threading;

namespace AppManager.OsApi.Events
{
    public class ObservableHotkeyEvent<TSender, TArgs>
    {
        private event TypedEvent<TSender, TArgs>? HandlersValue;
        private int CountValue;

        /// <summary>
        /// object?: sender - The ObservableEvent instance whose count has changed.
        /// int: oldCount - The old count of handlers subscribed to the event.
        /// </summary>
        public event TypedEvent<object?, int>? CountChangedEvent;

        public int Count { get => CountValue; }

        public static ObservableHotkeyEvent<TSender, TArgs> operator +(
            ObservableHotkeyEvent<TSender, TArgs> evt,
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
            evt.CountChangedEvent?.Invoke(evt, evt.CountValue - 1);

            return evt;
        }

        public static ObservableHotkeyEvent<TSender, TArgs> operator -(
            ObservableHotkeyEvent<TSender, TArgs> evt,
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
            evt.CountChangedEvent?.Invoke(evt, evt.CountValue + 1);

            return evt;
        }

        public void Invoke(TSender sender, TArgs args)
            => HandlersValue?.Invoke(sender, args);
    }

}
