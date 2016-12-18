using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Machine.Specifications;
using System.Async.Synchronization;
using System.Threading.Tasks;

namespace Susyem.Async.Synchronization.Tests.AsyncLockTests
{
    [Subject(typeof(AsyncLock))]
    public class When_acquiring_an_inactive_lock
    {
        protected static AsyncLock Subject;
        protected static Task<AsyncLock.Releaser> Result;

        Establish context = () =>
        {
            Subject = new AsyncLock();
        };

        Cleanup after = () =>
        {
            Result.Dispose();
        };

        Because of = () =>
        {
            Result = Subject.LockAsync();
        };

        It should_acquire_lock_immediately = () => Result.IsCompleted.ShouldBeTrue();
    }
}
