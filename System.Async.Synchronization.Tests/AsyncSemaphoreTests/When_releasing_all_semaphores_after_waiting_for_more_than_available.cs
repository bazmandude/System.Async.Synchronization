using Machine.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Async.Synchronization.Tests.AsyncSemaphoreTests
{
    [Subject(typeof(AsyncSemaphore))]
    class When_releasing_all_semaphores_after_waiting_for_more_than_available
    {
        protected static AsyncSemaphore Subject;
        protected static Task Blocking;
        protected static Task<bool> Result;

        Establish context = () =>
        {
            Subject = new AsyncSemaphore(1);
            Blocking = Subject.WaitAsync();
        };

        Cleanup after = () =>
        {
        };

        Because of = () =>
        {
            Result = Subject.WaitAsync(10);
        };

        It should_fail_to_acquire_lock = () => Result.Result.ShouldBeFalse();
    }
}
