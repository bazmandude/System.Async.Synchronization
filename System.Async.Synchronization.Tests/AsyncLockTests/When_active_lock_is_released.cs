using Machine.Specifications;
using System;
using System.Async.Synchronization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Async.Synchronization.Tests.AsyncLockTests
{
    [Subject(typeof(AsyncLock))]
    class When_active_lock_is_released
    {
        protected static AsyncLock Subject;
        protected static AsyncLock.Releaser HoldingLock;
        protected static Task<AsyncLock.Releaser> Result;

        Establish context = async () =>
        {
            Subject = new AsyncLock();
            HoldingLock = await Subject.LockAsync();
        };

        Cleanup after = () =>
        {
            Result.Dispose();
        };

        Because of = () =>
        {
            Result = Subject.LockAsync();
            HoldingLock.Dispose();
        };

        It should_not_acquire_the_lock = () => Result.IsCompleted.ShouldBeTrue();
    }
}
