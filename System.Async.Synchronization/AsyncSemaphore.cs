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
        private readonly Queue<TaskCompletionSource<bool>> _waiters = new Queue<TaskCompletionSource<bool>>();
        private int _maxCount;
        private int _currentCount;

        public AsyncSemaphore(int initialCount)
        {
            if (initialCount < 0) throw new ArgumentOutOfRangeException(nameof(initialCount));
            _maxCount = initialCount;
            _currentCount = _maxCount;
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
            var completionSource = this.AllocateCompletionSource();
            return completionSource?.Task ?? Task.CompletedTask;
        }

        /// <summary>
        /// Returns a task that won't be completed until signaled or timeout
        /// </summary>
        /// <param name="timeout">Timeout value in milliseconds</param>
        public async Task<bool> WaitAsync(int timeout)
        {
            // Get wait completion source and task
            var completionSource = this.AllocateCompletionSource();
            var waitTask = completionSource?.Task ?? Task.CompletedTask;

            // Handle infinite timeout
            if (timeout == Timeout.Infinite)
            {
                await waitTask;
            }
            else
            {
                try
                {
                    // Handle timed wait
                    await waitTask.TimeoutAfter(timeout);
                }
                catch (TimeoutException)
                {
                    // Cancel completion source on timeout
                    completionSource?.SetCanceled();
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
                // Get first waiting non cancelled completion source
                while ((_waiters.Count > 0) && (toRelease == null))
                {
                    toRelease = _waiters.Dequeue();
                    if (toRelease.Task.IsCanceled)
                    {
                        toRelease = null;
                    }
                }

                // If no completion sources then increment count
                if ((toRelease == null) && (_currentCount < _maxCount))
                {
                    ++_currentCount;
                }
            }

            // Signal waiting completion source
            if (toRelease != null)
            {
                toRelease.SetResult(true);
            }
        }

        #region Private methods

        private TaskCompletionSource<bool> AllocateCompletionSource()
        {
            lock (_waiters)
            {
                if (_currentCount > 0)
                {
                    --_currentCount;
                    return null;
                }
                else
                {
                    var waiter = new TaskCompletionSource<bool>();
                    _waiters.Enqueue(waiter);
                    return waiter;
                }
            }
        }

        #endregion
    }
}
