using SyncFolders;

class Program
{
    // sync/copy files from source to destination folder
    // delete extra files in destination folder
    // do the actions periodicaly
    static async Task Main(string[] args)
    {
        ArgumentsParser argParser = new ArgumentsParser(args);
        LoggerMessages.GetInstance(argParser.GetLogPath());
        int intervalSec = argParser.GetIntervalSec();

        Console.WriteLine("--- Starting periodic folder sync... ---");
        while (true)
        {
            CompareFolders compare = new CompareFolders(argParser.GetFromPath(), argParser.GetToPath());
            compare.ShowDiff();

            new SyncFiles(compare);

            Console.WriteLine($"[{DateTime.Now}] Sync completed. Waiting {intervalSec} seconds...");
            await Task.Delay(intervalSec * 1000); // Wait before next check
        }
        
    }
        
}