using System.Async.Synchronization.Extensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Async.Synchronization
{
    /// <summary>
    /// Task friendly auto reset event.
    /// Won't cause a thread pool thread to be hogged while wait to the synchronization object to be signaled
    /// based on Stephen Toub's article below
    /// </summary>
    /// <see cref="https://blogs.msdn.microsoft.com/pfxteam/2012/02/11/building-async-coordination-primitives-part-2-asyncautoresetevent/"/>
    public class AsyncAutoResetEvent
    {
        private readonly Task _completedTask = Task.FromResult(true);
        private readonly Queue<TaskCompletionSource<bool>> _waits = new Queue<TaskCompletionSource<bool>>();
        private bool _signaled;

        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="initialState">The initial state, TRUE if signaled; FALSE if not</param>
        public AsyncAutoResetEvent(bool initialState = false)
        {
            if (initialState)
            {
                this.Set();
            }
        }

        /// <summary>
        /// Returns a task that won't be completed until the event is signaled
        /// </summary>
        public Task WaitAsync()
        {
            lock (_waits)
            {
                if (_signaled)
                {
                    _signaled = false;
                    return _completedTask;
                }
                else
                {
                    var tcs = new TaskCompletionSource<bool>();
                    _waits.Enqueue(tcs);
                    return tcs.Task;
                }
            }
        }

        /// <summary>
        /// Returns a task that won't be completed until the event is signaled or timeout
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
        /// Signals the event
        /// </summary>
        public void Set()
        {
            TaskCompletionSource<bool> toRelease = null;
            lock (_waits)
            {
                if (_waits.Count > 0)
                {
                    toRelease = _waits.Dequeue();
                }
                else if (!_signaled)
                {
                    _signaled = true;
                }
            }
            toRelease?.SetResult(true);
        }
    }
}
