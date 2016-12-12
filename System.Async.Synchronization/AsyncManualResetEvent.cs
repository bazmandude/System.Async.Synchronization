using System.Async.Synchronization.Extensions;
using System.Threading;
using System.Threading.Tasks;

namespace System.Async.Synchronization
{
    /// <summary>
    /// Task friendly manual reset event.
    /// Won't cause a thread pool thread to be hogged while wait to the synchronization object to be signaled
    /// based on Stephen Toub's article below
    /// </summary>
    /// <see cref="https://blogs.msdn.microsoft.com/pfxteam/2012/02/11/building-async-coordination-primitives-part-1-asyncmanualresetevent/"/>
    public class AsyncManualResetEvent
    {
        private volatile TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="initialState">The initial state, TRUE if signaled; FALSE if not</param>
        public AsyncManualResetEvent(bool initialState = false)
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
            return _tcs.Task;
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
            var tcs = _tcs;
            Task.Factory.StartNew(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default);
            tcs.Task.Wait();
        }

        /// <summary>
        /// Resets the event
        /// </summary>
        public void Reset()
        {
            while (true)
            {
                var tcs = _tcs;
#pragma warning disable 420 //Reference to a volatile field will not be treated as volatile
                if (!tcs.Task.IsCompleted || Interlocked.CompareExchange(ref _tcs, new TaskCompletionSource<bool>(), tcs) == tcs)
#pragma warning restore 420
                {
                    return;
                }
            }
        }
    }
}
