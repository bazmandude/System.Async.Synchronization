using System;
using System.Async.Collections.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Async.Collections
{
    public class AsyncQueue<T> : IAsyncQueue<T>
    {
        private Queue<T> _queue;

        public int Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsEmpty
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public AsyncQueue()
        {
            _queue = new Queue<T>();
        }

        public Task<T> Dequeue()
        {
            throw new NotImplementedException();
        }

        public Task Enqueue(T item)
        {
            throw new NotImplementedException();
        }

        public Task<T> Peek()
        {
            throw new NotImplementedException();
        }
    }
}
