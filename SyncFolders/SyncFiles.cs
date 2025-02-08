using System.Collections.Concurrent;
using System.Diagnostics;

namespace SyncFolders
{
    // copy missing/changed files to destination folder
    internal class SyncFiles
    {
        public SyncFiles(CompareFolders compareFolders)
        {
            CopyMissingFiles(compareFolders);
            DeleteExtraFiles(compareFolders);
        }

        // Copy files to destination folder
        private void CopyMissingFiles(CompareFolders compareFolders)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            ConcurrentBag<(string sourcePath, string destinationPath)> missingFiles = compareFolders.GetMissingFiles();
            if (missingFiles.IsEmpty) 
                return;

            LoggerMessages.ConsoleAndLogFile("--- Copy files to destination folder ---");
            // Copy files in parallel
            Parallel.ForEach(missingFiles, filePair =>
            {
                try
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    File.Copy(filePair.sourcePath, filePair.destinationPath, overwrite: true);
                    stopwatch.Stop();
                    LoggerMessages.ConsoleAndLogFile($"Copied: {filePair.sourcePath} -> {filePair.destinationPath} - {stopwatch.ElapsedMilliseconds} ms");
                }
                catch (Exception ex)
                {
                    LoggerMessages.ConsoleAndLogFile($"Error copying {filePair.sourcePath}: {ex.Message}");
                }
            });

            stopwatch.Stop();
            LoggerMessages.ConsoleAndLogFile($"Copied total time: {stopwatch.ElapsedMilliseconds} ms");
        }

        // Delete extra files in destination folder
        private void DeleteExtraFiles(CompareFolders compareFolders) 
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            ConcurrentBag<string> extraFiles = compareFolders.GetExtraFiles();
            if (extraFiles.IsEmpty)
                return;

            LoggerMessages.ConsoleAndLogFile("--- Delete extra files in destination folder ---");

            // Delete extra files in parallel
            Parallel.ForEach(extraFiles, file =>
            {
                try
                {
                    File.Delete(file);
                    LoggerMessages.ConsoleAndLogFile($"Deleted: {file}");
                }
                catch (Exception ex)
                {
                    LoggerMessages.ConsoleAndLogFile($"Error deleting {file}: {ex.Message}");
                }
            });

            stopwatch.Stop();
            LoggerMessages.ConsoleAndLogFile($"Deleted total time: {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}
