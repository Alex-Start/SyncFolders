using System.Collections.Concurrent;

namespace SyncFolders
{
    internal class LoggerMessages : IDisposable
    {
        private static readonly object lockObj = new object();
        private static LoggerMessages instance = null;
        private static string logFilePath;
        private readonly ConcurrentQueue<string> logQueue;
        private readonly SemaphoreSlim semaphore;
        private readonly Task logTask;
        private bool isRunning;

        private LoggerMessages(string logFilePath) 
        {
            if (string.IsNullOrWhiteSpace(logFilePath))
            {
                throw new ArgumentNullException("Log path is null/empty or need to Init LoggerMessages first.");
            }
            LoggerMessages.logFilePath = logFilePath;
            logQueue = new ConcurrentQueue<string>();
            semaphore = new SemaphoreSlim(0);
            isRunning = true;

            // Start background log processing
            logTask = Task.Run(ProcessLogQueue);
        }


        public static LoggerMessages GetInstance()
        {
            return GetInstance(logFilePath);
        }

        public static LoggerMessages GetInstance(string logFilePath)
        {
            if (instance == null)
            {
                lock (lockObj)  // Thread-safe singleton initialization
                {
                    if (instance == null)
                    {
                        instance = new LoggerMessages(logFilePath);
                    }
                }
            }

            return instance;
        }

        public static void ConsoleAndLogFile(string message)
        {
            Console.WriteLine(message);
            GetInstance().Log(message);
        }

        public void Log(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            logQueue.Enqueue(logEntry);
            semaphore.Release();  // Signal that a log message is available
        }

        private async Task ProcessLogQueue()
        {
            while (isRunning || !logQueue.IsEmpty)
            {
                await semaphore.WaitAsync();

                if (logQueue.TryDequeue(out string logMessage))
                {
                    try
                    {
                        await File.AppendAllTextAsync(logFilePath, logMessage + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Logger Error: {ex.Message}");
                    }
                }
            }
        }

        public void Dispose()
        {
            isRunning = false;
            semaphore.Release();  // Ensure last log messages are written
            logTask.Wait();  // Wait for logging to complete
        }
    }

}
