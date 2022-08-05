namespace WpfApp.Logic
{
    using System;
    using System.Threading.Tasks;
    using Interfaces;

    public class AsyncTaskRunner : IAsyncTaskRunner
    {
        public Task RunActionAsync(Action action)
        {
            var task = new Task(action);
            task.Start();
            return task;
        }

        public Task<TResult> RunFuncAsync<TResult>(Func<TResult> function)
        {
            var task = new Task<TResult>(function);
            task.Start();
            return task;
        }
    }
}