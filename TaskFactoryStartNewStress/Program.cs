using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskFactoryStartNewStress
{
    class Program
    {
        private static readonly int _threads = Environment.ProcessorCount;

        private static readonly TaskFactory _taskFactory = new TaskFactory();
        private static int[] _tasksStarted;
        private static int[] _tasksExecuted;

        static void Main(string[] args)
        {
            _tasksStarted = new int[_threads];
            _tasksExecuted = new int[_threads];

            var printStatusTask = PrintStatus();

            for (var i=0; i < _threads; i++)
            {
                StartTasks(i);
            }

            printStatusTask.Wait();
        }

        private static async Task PrintStatus()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Last Updated: {DateTime.Now.ToString("h:mm:ss.fff")}");

                for (var i=0; i < _threads; i++)
                {
                    Console.WriteLine($"[{i}] Started: {_tasksStarted[i]}, Executed: {_tasksExecuted[i]}" +
                        $" Executing: {_tasksStarted[i] - _tasksExecuted[i]}");
                }

                await Task.Delay(1000);
            }
        }

        private static void StartTasks(int index)
        {
            var t = new Thread(() =>
            {
                while (true)
                {
                    StartTask(index);
                }
            });
            t.Start();
        }

        private static void StartTask(int index)
        {
            _tasksStarted[index]++;
            _taskFactory.StartNew(_ =>
                {
                    Interlocked.Increment(ref _tasksExecuted[index]);
                },
                new object(),
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }
    }
}
