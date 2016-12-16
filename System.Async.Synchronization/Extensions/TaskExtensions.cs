using System.Threading;
using System.Threading.Tasks;

namespace System.Async.Synchronization.Extensions
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Allows a timeout to be set for task completion
        /// This is based on Stephen Toub implementation in Joe Hoag article below.
        /// </summary>
        /// <param name="millisecondsTimeout">The length of time to wait for the task to complete</param>
        /// <exception cref="TimeoutException">Thrown if the task does not complete within the specified timeout period</exception>
        /// <see cref="https://blogs.msdn.microsoft.com/pfxteam/2011/11/10/crafting-a-task-timeoutafter-method/"/>
        public static async Task TimeoutAfter(this Task task, int millisecondsTimeout)
        {
            if ((task.IsCompleted) || (task.IsCanceled) || (task.IsFaulted))
            {
                await task;
            }
            else if (millisecondsTimeout == 0)
            {
                throw new TimeoutException();
            }
            else
            {
                using (var tokenSource = new CancellationTokenSource())
                {
                    if (task == await Task.WhenAny(task, Task.Delay(millisecondsTimeout, tokenSource.Token)))
                    {
                        tokenSource.Cancel();
                        await task;
                    }
                    else
                    {
                        throw new TimeoutException();
                    }
                }
            }
        }

        /// <summary>
        /// Allows a timeout to be set for task completion
        /// This is based on Stephen Toub implementation in Joe Hoag article below.
        /// </summary>
        /// <param name="millisecondsTimeout">The length of time to wait for the task to complete</param>
        /// <exception cref="TimeoutException">Thrown if the task does not complete within the specified timeout period</exception>
        /// <see cref="https://blogs.msdn.microsoft.com/pfxteam/2011/11/10/crafting-a-task-timeoutafter-method/"/>
        public static async Task<T> TimeoutAfter<T>(this Task<T> task, int millisecondsTimeout)
        {
            if ((task.IsCompleted) || (task.IsCanceled) || (task.IsFaulted))
            {
                return await task;
            }
            else if (millisecondsTimeout == 0)
            {
                throw new TimeoutException();
            }
            else
            {
                using (var tokenSource = new CancellationTokenSource())
                {
                    if (task == await Task.WhenAny(task, Task.Delay(millisecondsTimeout, tokenSource.Token)))
                    {
                        tokenSource.Cancel();
                        return await task;
                    }
                    else
                    {
                        throw new TimeoutException();
                    }
                }
            }
        }
    }
}
