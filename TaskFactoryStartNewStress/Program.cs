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
            var startNew = (args.Length >= 1) ? bool.Parse(args[0]) : true;

            _tasksStarted = new int[_threads];
            _tasksExecuted = new int[_threads];

            var printStatusTask = PrintStatus();

            for (var i=0; i < _threads; i++)
            {
                StartTasks(i, startNew);
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

        private static void StartTasks(int index, bool startNew)
        {
            var t = new Thread(() =>
            {
                while (true)
                {
                    StartTask(index, startNew);
                }
            });
            t.Start();
        }

        private static void StartTask(int index, bool startNew)
        {
            _tasksStarted[index]++;
            if (startNew)
            {
                _taskFactory.StartNew(_ =>
                {
                    Interlocked.Increment(ref _tasksExecuted[index]);
                },
                    new object(),
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
            }
            else
            {
                var thread = new Thread(_ =>
                {
                    Interlocked.Increment(ref _tasksExecuted[index]);
                });
                thread.Start();
            }
        }
    }
}
