using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSmartProxy.Infrastructure.Extensions
{
    /// <summary>
    /// Thread-safe peekable async queue.
    /// Post/Receive/Peek must observe the same item ordering, otherwise
    /// heartbeat keep-alive may be written to a connection that has already
    /// been consumed into an active data tunnel.
    /// </summary>
    public class PeekableBufferBlock<T>
    {
        private readonly object _syncRoot = new object();
        private readonly Queue<T> _items = new Queue<T>();
        private readonly Queue<TaskCompletionSource<T>> _waiters = new Queue<TaskCompletionSource<T>>();

        public void Post(T item)
        {
            TaskCompletionSource<T> waiter = null;

            lock (_syncRoot)
            {
                while (_waiters.Count > 0)
                {
                    waiter = _waiters.Dequeue();
                    if (!waiter.Task.IsCompleted)
                    {
                        break;
                    }

                    waiter = null;
                }

                if (waiter == null)
                {
                    _items.Enqueue(item);
                    return;
                }
            }

            waiter.TrySetResult(item);
        }

        public Task<T> ReceiveAsync()
        {
            lock (_syncRoot)
            {
                if (_items.Count > 0)
                {
                    return Task.FromResult(_items.Dequeue());
                }

                var waiter = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
                _waiters.Enqueue(waiter);
                return waiter.Task;
            }
        }

        public T Receive()
        {
            return ReceiveAsync().GetAwaiter().GetResult();
        }

        public int Count
        {
            get
            {
                lock (_syncRoot)
                {
                    return _items.Count;
                }
            }
        }

        public T Peek()
        {
            lock (_syncRoot)
            {
                if (_items.Count == 0)
                {
                    return default;
                }

                return _items.Peek();
            }
        }
    }
}
