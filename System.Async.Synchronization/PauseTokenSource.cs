using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Async.Synchronization
{
    /// Enables a task to pause if the token is signaled
    /// Based on Stephen Toub's article below
    /// </summary>
    /// <see cref="https://blogs.msdn.microsoft.com/pfxteam/2013/01/13/cooperatively-pausing-async-methods/"/>
    public class PauseTokenSource
    {
        internal static readonly Task _completedTask = Task.FromResult(true);
        private volatile TaskCompletionSource<bool> _paused;

        public PauseToken Token
        {
            get
            {
                return new PauseToken(this);
            }
        }

        /// <summary>
        /// Gets/sets if the pause signal
        /// </summary>
        public bool IsPaused
        {
            get
            {
                return _paused != null;
            }
            set
            {
                if (value == this.IsPaused)
                {
                    return;
                }

                if (value)
                {
                    Interlocked.CompareExchange(ref _paused, new TaskCompletionSource<bool>(), null);
                }
                else
                {
                    while (true)
                    {
                        var tcs = _paused;
                        if (tcs == null) return;
                        if (Interlocked.CompareExchange(ref _paused, null, tcs) == tcs)
                        {
                            tcs.SetResult(true);
                            break;
                        }
                    }
                }
            }
        }

        internal Task WaitWhilePausedAsync()
        {
            var cur = _paused;
            return (cur != null) ? cur.Task : _completedTask;
        }
    }

    /// <summary>
    /// Pause token created from a pause token source
    /// </summary>
    public struct PauseToken
    {
        private readonly PauseTokenSource _source;

        internal PauseToken(PauseTokenSource source)
        {
            _source = source;
        }

        /// <summary>
        /// Gets is a pause has been signaled
        /// </summary>
        public bool IsPaused
        {
            get
            {
                return (_source != null) && (_source.IsPaused);
            }
        }

        /// <summary>
        /// Gets a task that will block until the pause is reset
        /// </summary>
        /// <returns>A task</returns>
        public Task WaitWhilePausedAsync()
        {
            return IsPaused ? _source.WaitWhilePausedAsync() : PauseTokenSource._completedTask;
        }
    }
}
