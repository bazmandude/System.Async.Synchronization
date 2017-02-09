using Machine.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Async.Synchronization.Tests.AsyncSemaphoreTests
{
    [Subject(typeof(AsyncSemaphore))]
    class When_acquiring_a_semaphore_with_less_than_max_resource_count
    {
        protected static AsyncSemaphore Subject;
        protected static Task Result;

        Establish context = () =>
        {
            Subject = new AsyncSemaphore(2);
        };

        Cleanup after = () =>
        {
        };

        Because of = () =>
        {
            Result = Subject.WaitAsync();
        };

        It should_acquire_the_lock = () => Result.IsCompleted.ShouldBeTrue();
    }
}
