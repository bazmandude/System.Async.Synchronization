using Machine.Specifications;
using System;
using System.Async.Synchronization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Susyem.Async.Synchronization.Tests.AsyncLockTests
{
    [Subject(typeof(AsyncLock))]
    class When_acquiring_an_active_lock
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
            HoldingLock.Dispose();
            Result.Dispose();
        };

        Because of = () =>
        {
            Result = Subject.LockAsync();
        };

        It should_not_acquire_the_lock = () => Result.IsCompleted.ShouldBeFalse();
    }
}
