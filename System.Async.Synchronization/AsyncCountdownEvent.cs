using System.Async.Synchronization.Extensions;
using System.Threading;
using System.Threading.Tasks;

namespace System.Async.Synchronization
{
    /// Task friendly countdown event.
    /// Won't cause a thread pool thread to be hogged while wait to the synchronization object to be signaled
    /// based on Stephen Toub's article below
    /// </summary>
    /// <see cref="https://blogs.msdn.microsoft.com/pfxteam/2012/02/11/building-async-coordination-primitives-part-3-asynccountdownevent/"/>
    public class AsyncCountdownEvent
    {
        private readonly AsyncManualResetEvent _amre = new AsyncManualResetEvent();
        private int _count;

        /// <summary>
        /// Object constructor
        /// </summary>
        /// <param name="initialCount">The initial countdown count</param>
        public AsyncCountdownEvent(int initialCount)
        {
            if (initialCount <= 0) throw new ArgumentOutOfRangeException(nameof(initialCount));
            _count = initialCount;
        }

        /// <summary>
        /// Returns a task that won't be completed the count down event reaches zero
        /// </summary>
        public Task WaitAsync()
        {
            return _amre.WaitAsync();
        }

        /// <summary>
        /// Returns a task that won't be completed the count down event reaches zero or timeout
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
        /// Signals the event (decrements the count)
        /// </summary>
        public void Signal()
        {
            if (_count <= 0)
            {
                throw new InvalidOperationException();
            }

            int newCount = Interlocked.Decrement(ref _count);
            if (newCount == 0)
            {
                _amre.Set();
            }
            else if (newCount < 0)
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Signals the event then waits for the countdown to reach zero
        /// </summary>
        public Task SignalAndWait()
        {
            Signal();
            return WaitAsync();
        }

        /// <summary>
        /// Signals the event then waits for the countdown to reach zero or timeout
        /// </summary>
        /// <param name="timeout">Timeout value in milliseconds</param>
        public Task<bool> SignalAndWait(int timeout)
        {
            Signal();
            return WaitAsync(timeout);
        }
    }
}
