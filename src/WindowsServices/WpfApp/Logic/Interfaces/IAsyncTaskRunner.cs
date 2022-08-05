namespace WpfApp.Logic.Interfaces
{
    using System;
    using System.Threading.Tasks;

    public interface IAsyncTaskRunner
    {
        Task RunActionAsync(Action action);
        Task<TResult> RunFuncAsync<TResult>(Func<TResult> function);
    }
}