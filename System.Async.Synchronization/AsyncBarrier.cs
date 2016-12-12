using System;
using System.Async.Synchronization.Extensions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Async.Synchronization
{
    /// Task friendly barrier.
    /// Won't cause a thread pool thread to be hogged while wait to the synchronization object to be signaled
    /// based on Stephen Toub's article below
    /// </summary>
    /// <see cref="https://blogs.msdn.microsoft.com/pfxteam/2012/02/11/building-async-coordination-primitives-part-4-asyncbarrier/"/>
    public class AsyncBarrier
    {
        private readonly int _participantCount;
        private int _remainingParticipants;
        private ConcurrentStack<TaskCompletionSource<bool>> _waiters;

        /// <summary>
        /// Object constructor
        /// </summary>
        /// <param name="participantCount">Number of participants</param>
        public AsyncBarrier(int participantCount)
        {
            if (participantCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(participantCount));
            }
            _participantCount = participantCount;
            _remainingParticipants = _participantCount;
            _waiters = new ConcurrentStack<TaskCompletionSource<bool>>();
        }

        /// <summary>
        /// Signal and wait for all other participants to signal
        /// </summary>
        public Task SignalAndWait()
        {
            var tcs = new TaskCompletionSource<bool>();
            _waiters.Push(tcs);
            if (Interlocked.Decrement(ref _remainingParticipants) == 0)
            {
                _remainingParticipants = _participantCount;
                var waiters = _waiters;
                _waiters = new ConcurrentStack<TaskCompletionSource<bool>>();
                Parallel.ForEach(waiters, w => w.SetResult(true));
            }
            return tcs.Task;
        }

        /// <summary>
        /// Signal and wait for all other participants to signal or timeout
        /// </summary>
        /// <param name="timeout">Timeout value in milliseconds</param>
        public async Task<bool> SignalAndWait(int timeout)
        {
            // Handle infinite timeout
            if (timeout == Timeout.Infinite)
            {
                await this.SignalAndWait();
            }
            else
            {
                try
                {
                    await this.SignalAndWait().TimeoutAfter(timeout);
                }
                catch (TimeoutException)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
