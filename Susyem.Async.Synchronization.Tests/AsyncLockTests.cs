using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Machine.Specifications;
using System.Async.Synchronization;

namespace Susyem.Async.Synchronization.Tests
{
    [Subject(typeof(AsyncLock))]
    public class When_acquiring_a_free_lock
    {
        protected static AsyncLock Subject;
        protected static AsyncLock.Releaser Result;

        Establish context = () =>
        {
            Subject = new AsyncLock();
        };

        Cleanup after = () =>
        {
            Result.Dispose();
        };

        Because of = async () =>
        {
            Result = await Subject.LockAsync();
        };

        It should_acquire_lock_immediately = () => Result.ShouldNotBeNull();
    }
}
