using System.Async.Synchronization.Extensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Async.Synchronization
{
    /// Task friendly semaphore.
    /// Won't cause a thread pool thread to be hogged while wait to the synchronization object to be signaled
    /// based on Stephen Toub's article below
    /// </summary>
    /// <see cref="https://blogs.msdn.microsoft.com/pfxteam/2012/02/12/building-async-coordination-primitives-part-5-asyncsemaphore/"/>
    public class AsyncSemaphore
    {
        private readonly Task _completedTask = Task.FromResult(true);
        private readonly Queue<TaskCompletionSource<bool>> _waiters = new Queue<TaskCompletionSource<bool>>();
        private int _currentCount;

        /// <summary>
        /// Object constructor
        /// </summary>
        /// <param name="initialCount">The initial semaphore count</param>
        public AsyncSemaphore(int initialCount)
        {
            if (initialCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialCount));
            }
            _currentCount = initialCount;
        }

        /// <summary>
        /// Get the current semaphore count
        /// </summary>
        public int CurrentCount
        {
            get
            {
                lock (_waiters)
                {
                    return _currentCount;
                }
            }
        }

        /// <summary>
        /// Returns a task that won't be completed until signaled
        /// </summary>
        public Task WaitAsync()
        {
            lock (_waiters)
            {
                if (_currentCount > 0)
                {
                    --_currentCount;
                    return _completedTask;
                }
                else
                {
                    var waiter = new TaskCompletionSource<bool>();
                    _waiters.Enqueue(waiter);
                    return waiter.Task;
                }
            }
        }

        /// <summary>
        /// Returns a task that won't be completed until signaled or timeout
        /// </summary>
        /// <param name="timeout">Timeout value in milliseconds</param>
        public async Task<bool> WaitAsync(int timeout)
        {
            // Handle infinite timeout
            if (timeout == Timeout.Infinite)
            {
                await this.WaitAsync();
            }
            else
            {
                try
                {
                    await this.WaitAsync().TimeoutAfter(timeout);
                }
                catch (TimeoutException)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Release a resource
        /// </summary>
        public void Release()
        {
            TaskCompletionSource<bool> toRelease = null;
            lock (_waiters)
            {
                if (_waiters.Count > 0)
                {
                    toRelease = _waiters.Dequeue();
                }
                else
                {
                    ++_currentCount;
                }
            }

            if (toRelease != null)
            {
                toRelease.SetResult(true);
            }
        }
    }
}
