using System.Collections;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace SyncFolders
{
    // compare folders and prepare missing/diff files list
    internal class CompareFolders
    {
        private Dictionary<string, FileInfo> dictSourceFiles;
        private Dictionary<string, FileInfo> dictDestinationFiles;
        //missing files in dest.
        private ConcurrentBag<(string sourcePath, string destinationPath)> missingFiles = new ConcurrentBag<(string, string)>(); // Thread-safe list for missing files
        //extra files in dest.
        private ConcurrentBag<string> extraFiles = new ConcurrentBag<string>();

        public CompareFolders(string fromPath, string toPath) {
            if (string.IsNullOrWhiteSpace(fromPath) || string.IsNullOrWhiteSpace(toPath))
                throw new ArgumentException("Error: Arguments are null or empty.");

            dictSourceFiles = GetFilesFromFolder(fromPath);
            dictDestinationFiles = GetFilesFromFolder(toPath);

            CompareDictionary(toPath);
        }

        private Dictionary<string, FileInfo> GetFilesFromFolder(string pathFolder)
        {
            string[] files = Directory.GetFiles(pathFolder, "*.*", SearchOption.AllDirectories);
            Dictionary<string, FileInfo> dictFiles = new Dictionary<string, FileInfo>();

            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                dictFiles.Add(fileInfo.Name, fileInfo);
            }

            return dictFiles;
        }

        private void CompareDictionary(string toPath)
        {
            LoggerMessages.ConsoleAndLogFile("--- Compare Folders ---");
            // Use Parallel.ForEach for multi-threaded processing
            Parallel.ForEach(dictSourceFiles.Keys, fileName =>
            {
                FileInfo srcFile = dictSourceFiles[fileName];

                // missing
                if (!dictDestinationFiles.ContainsKey(fileName))
                {
                    string sourcePath = srcFile.FullName;
                    string destinationPath = Path.Combine(toPath, fileName);
                    missingFiles.Add((sourcePath, destinationPath));
                    LoggerMessages.ConsoleAndLogFile($"File is missing: {srcFile.FullName}, Size: {srcFile.Length} bytes");
                }
                else
                {
                    FileInfo destFile = dictDestinationFiles[fileName];
                    // diff.
                    if (!CompareFilesByHash(srcFile.FullName, destFile.FullName))
                    {
                        missingFiles.Add((srcFile.FullName, destFile.FullName));
                        LoggerMessages.ConsoleAndLogFile($"File is diff: " +
                            $"\n Source: {srcFile.FullName}, Size: {srcFile.Length} bytes" +
                            $"\n Destination: {destFile.FullName}, Size: {destFile.Length} bytes");
                    }
                }
            });

            // Find extra files (exist in destination but NOT in source)
            Parallel.ForEach(dictDestinationFiles.Keys, fileName =>
            {
                FileInfo destFile = dictDestinationFiles[fileName];
                if (!dictSourceFiles.ContainsKey(fileName))
                    extraFiles.Add(dictDestinationFiles[fileName].FullName);
            });
        }

        static bool CompareFilesByHash(string filePath1, string filePath2)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash1 = ComputeFileHash(filePath1, sha256);
                byte[] hash2 = ComputeFileHash(filePath2, sha256);
                return StructuralComparisons.StructuralEqualityComparer.Equals(hash1, hash2);
            }
        }

        static byte[] ComputeFileHash(string filePath, HashAlgorithm hashAlgorithm)
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                return hashAlgorithm.ComputeHash(stream);
            }
        }

        public ConcurrentBag<(string, string)> GetMissingFiles()
        {
            return missingFiles;
        }

        public ConcurrentBag<string> GetExtraFiles()
        {
            return extraFiles;
        }

        public void ShowDiff()
        {
            Console.WriteLine("Missing files in destination (Source => Destination):");
            foreach (var (sourceFile, distFile) in missingFiles)
                Console.WriteLine($"- {sourceFile} => {distFile}");
            if (missingFiles.IsEmpty)
                Console.WriteLine("No any missing files");
        }
    }
}
