// AI Summary: 2026-04-22 - Added delete-on-exit cleanup registry for downloaded installer files.
using System;
using System.Collections.Generic;
using System.IO;

namespace GMTPC.Tool
{
    public partial class MainWindow
    {
        private readonly HashSet<string> _downloadedFilesToDeleteOnExit = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private bool _downloadCleanupAttached;

        private void AttachDownloadedFileCleanupOnClose()
        {
            if (_downloadCleanupAttached) return;

            Closing += MainWindow_DeleteDownloadedFilesOnClosing;
            _downloadCleanupAttached = true;
        }

        private void RegisterDownloadedFileForDeleteOnExit(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return;
            _downloadedFilesToDeleteOnExit.Add(filePath);
        }

        private void MainWindow_DeleteDownloadedFilesOnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (string filePath in _downloadedFilesToDeleteOnExit)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch
                {
                    // Ignore cleanup failures; the installer may still be running.
                }
            }
        }
    }
}
