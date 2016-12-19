using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Async.Collections.Interfaces
{
    public interface IAsyncQueue<T>
    {
        int Count { get; }

        bool IsEmpty { get; }

        Task Enqueue(T item);

        Task<T> Dequeue();

        Task<T> Peek();
    }
}
