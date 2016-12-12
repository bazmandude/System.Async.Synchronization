using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Async.Synchronization
{
    /// Task reader writer lock with writer prioritization.
    /// Won't cause a thread pool thread to be hogged while wait to the synchronization object to be signaled
    /// based on Stephen Toub's article below
    /// </summary>
    /// <see cref="https://blogs.msdn.microsoft.com/pfxteam/2012/02/12/building-async-coordination-primitives-part-7-asyncreaderwriterlock/"/>
    public class AsyncReaderWriterLock
    {
        private readonly Task<Releaser> _readerReleaser;
        private readonly Task<Releaser> _writerReleaser;
        private readonly Queue<TaskCompletionSource<Releaser>> _waitingWriters = new Queue<TaskCompletionSource<Releaser>>();
        private TaskCompletionSource<Releaser> _waitingReader = new TaskCompletionSource<Releaser>();
        private int _readersWaiting;
        private int _status;

        /// <summary>
        /// Reader/writer lock releaser. Dispose of the release when the read/write lock is no longer required
        /// </summary>
        public struct Releaser : IDisposable
        {
            private readonly AsyncReaderWriterLock _toRelease;
            private readonly bool _writer;

            internal Releaser(AsyncReaderWriterLock toRelease, bool writer)
            {
                _toRelease = toRelease;
                _writer = writer;
            }
            public void Dispose()
            {
                if (_toRelease != null)
                {
                    if (_writer)
                    {
                        _toRelease.WriterRelease();
                    }
                    else
                    {
                        _toRelease.ReaderRelease();
                    }
                }
            }
        }

        /// <summary>
        /// Object constructor
        /// </summary>
        public AsyncReaderWriterLock()
        {
            _readerReleaser = Task.FromResult(new Releaser(this, false));
            _writerReleaser = Task.FromResult(new Releaser(this, true));
        }

        /// <summary>
        /// Acquire reader lock.
        /// </summary>
        /// <returns>Reader lock releaser</returns>
        public Task<Releaser> ReaderLockAsync()
        {
            lock (_waitingWriters)
            {
                if (_status >= 0 && _waitingWriters.Count == 0)
                {
                    ++_status;
                    return _readerReleaser;
                }
                else
                {
                    ++_readersWaiting;
                    return _waitingReader.Task.ContinueWith(t => t.Result);
                }
            }
        }

        /// <summary>
        /// Attempt to acquire writer lock
        /// </summary>
        /// <returns>Writer lock releaser</returns>
        public Task<Releaser> WriterLockAsync()
        {
            lock (_waitingWriters)
            {
                if (_status == 0)
                {
                    _status = -1;
                    return _writerReleaser;
                }
                else
                {
                    var waiter = new TaskCompletionSource<Releaser>();
                    _waitingWriters.Enqueue(waiter);
                    return waiter.Task;
                }
            }
        }

        #region Private methods

        private void ReaderRelease()
        {
            TaskCompletionSource<Releaser> toWake = null;
            lock (_waitingWriters)
            { 
                --_status;
                if (_status == 0 && _waitingWriters.Count > 0)
                {
                    _status = -1;
                    toWake = _waitingWriters.Dequeue();
                }
            }
            if (toWake != null)
            {
                toWake.SetResult(new Releaser(this, true));
            }
        }

        private void WriterRelease()
        {
            TaskCompletionSource<Releaser> toWake = null;
            bool toWakeIsWriter = false;
            lock (_waitingWriters)
            {
                if (_waitingWriters.Count > 0)
                {
                    toWake = _waitingWriters.Dequeue();
                    toWakeIsWriter = true;
                }
                else if (_readersWaiting > 0)
                {
                    toWake = _waitingReader;
                    _status = _readersWaiting;
                    _readersWaiting = 0;
                    _waitingReader = new TaskCompletionSource<Releaser>();
                }
                else
                {
                    _status = 0;
                }
            }
            if (toWake != null)
            {
                toWake.SetResult(new Releaser(this, toWakeIsWriter));
            }
        }

        #endregion
    }
}
