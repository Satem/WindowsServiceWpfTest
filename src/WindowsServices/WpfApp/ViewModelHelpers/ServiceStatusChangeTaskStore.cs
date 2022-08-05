namespace WpfApp.ViewModelHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class ServiceStatusChangeTaskStore
    {
        private readonly Dictionary<string, TaskInfo> _serviceNameToTaskInfo;

        public ServiceStatusChangeTaskStore()
        {
            _serviceNameToTaskInfo = new Dictionary<string, TaskInfo>();
        }

        public Task StartAndStoreNewTask(string serviceName, Func<string, CancellationToken, Task> statusChangeMethod)
        {
            StopAndRemoveTaskIfExists(serviceName);

            var cancellationTokenSource = new CancellationTokenSource();
            var task = new Task(() => statusChangeMethod(serviceName, cancellationTokenSource.Token));

            var taskInfo = new TaskInfo(task, cancellationTokenSource);
            _serviceNameToTaskInfo[serviceName] = taskInfo;

            task.Start();
            return task;
        }

        public void StopAndRemoveTaskIfExists(string serviceName)
        {
            if (_serviceNameToTaskInfo.ContainsKey(serviceName) == false)
                return;

            var taskInfo = _serviceNameToTaskInfo[serviceName];
            if (taskInfo.Task.IsCompleted == false)
                taskInfo.CancellationTokenSource.Cancel();

            _serviceNameToTaskInfo.Remove(serviceName);
        }

        public bool IsThereAnyRunningTask(string serviceName)
        {
            return _serviceNameToTaskInfo.ContainsKey(serviceName);
        }

        private class TaskInfo
        {
            public TaskInfo(Task task, CancellationTokenSource cancellationTokenSource)
            {
                Task = task;
                CancellationTokenSource = cancellationTokenSource;
            }

            public Task Task { get; }
            public CancellationTokenSource CancellationTokenSource { get; }
        }
    }
}