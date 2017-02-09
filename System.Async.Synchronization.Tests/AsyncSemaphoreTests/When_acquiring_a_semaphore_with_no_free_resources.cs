using Machine.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Async.Synchronization.Tests.AsyncSemaphoreTests
{
    [Subject(typeof(AsyncSemaphore))]
    class When_acquiring_a_semaphore_with_no_free_resources
    {
        protected static AsyncSemaphore Subject;
        protected static Task Blocking;
        protected static Task Result;

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

        It should_acquire_the_lock = () => Result.IsCompleted.ShouldBeFalse();
    }
}
