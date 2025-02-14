namespace SyncFolders
{
    // parse arguments: from="source folder" to="destination folder" interval=10 log="path to log file"
    internal class ArgumentsParser
    {
        private Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
        private const String FROMPARAM = "from";
        private const String TOPARAM = "to";
        private const String LOGPARAM = "log";
        private const String INTERVALPARAM = "interval";
        private String fromPath;
        private String toPath;
        private const int IntervalDefaultSec = 5;//by default in sec.
        private int intervalSec = IntervalDefaultSec;
        private String logPath;

        public ArgumentsParser(String[] args) 
        {
            keyValuePairs = ParseArguments(args);

            if (!GetAndCheckParams(keyValuePairs))
            {
                throw new ArgumentException("There is incorrect input argument.");
            }

            Console.WriteLine("--- Parse Arguments ---");
            Console.WriteLine($"Source: {fromPath}");
            Console.WriteLine($"Destination: {toPath}");
            Console.WriteLine($"Interval (seconds): {intervalSec}");
            Console.WriteLine($"Log path: {logPath}");
        }

        public string GetFromPath() { 
            return fromPath; 
        }

        public string GetToPath()
        {
            return toPath;
        }

        public int GetIntervalSec()
        {
            return intervalSec;
        }

        public string GetLogPath()
        {
            return logPath;
        }

        Dictionary<string, string> ParseArguments(string[] args)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (String arg in args)
            {
                String[] arr = arg.Split('=');
                if (arr.Length > 1)
                {
                    result[arr[0].ToLower()] = arr[1].Trim('"');
                }
            }
            return result;
        }

        Boolean GetAndCheckParams(Dictionary<string, string> parameters)
        {
            if (!parameters.ContainsKey(FROMPARAM) || !parameters.ContainsKey(TOPARAM) || !parameters.ContainsKey(LOGPARAM))
            {
                Console.WriteLine($"Usage input arguments: from=\"<source_folder>\" to=\"<destination_folder>\" interval=10 log=\"path to log file\" " +
                    $"\nwhere interval is > 0 in sec. By default interval={IntervalDefaultSec}");
                return false;
            }

            parameters.TryGetValue(FROMPARAM, out fromPath);
            parameters.TryGetValue(TOPARAM, out toPath);
            string str = parameters.GetValueOrDefault(INTERVALPARAM, $"{IntervalDefaultSec}");
            try
            {
                intervalSec = int.Parse(str);
                if (intervalSec <= 0)
                {
                    intervalSec = IntervalDefaultSec;
                }
            }
            catch (FormatException)
            {
                Console.WriteLine($"Incorrect interval value: {str}\ninterval is > 0 in sec.");
                return false;
            }
            parameters.TryGetValue(LOGPARAM, out logPath);

            // Check if source folder exists
            if (string.IsNullOrWhiteSpace(fromPath) || !Directory.Exists(fromPath))
            {
                Console.WriteLine($"Error: Source folder does not exist: {fromPath}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(toPath))
            {
                Console.WriteLine($"Error: Destination folder path is empty: {toPath}");
                return false;
            }

            // Check if destination folder exists, if not, create it
            if (!Directory.Exists(toPath))
            {
                Console.WriteLine($"Destination folder does not exist, creating it: {toPath}");
                Directory.CreateDirectory(toPath);
            }

            if (string.IsNullOrWhiteSpace(logPath))
            {
                Console.WriteLine($"Log path is empty: {logPath}");
                return false;
            }

            return true;
        }
    }
}
