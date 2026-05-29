// AI Summary: 2026-05-29 - Removed WintoHDD, Win10 LTSC IOT 21H2, and Win10 22H2 2024 December constants, click handlers, and install methods.
﻿// AI Summary: 2026-05-02 - Windows setup tab: Ventoy flow moved to MainWindow.SystemArchiveFlow.cs and checkbox wiring remains here
// AI Summary: 2026-05-02 - Renamed the Windows Setup partial and kept Ventoy/WintoHDD flows
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GMTPC.Tool
{
// =============================================================================
// MainWindow.SystemWindowsSetup.cs
// Windows Setup
// Updated: 2026-05-02 - Kept Ventoy archive flow in a dedicated file
// Updated: 2026-05-02 - Ventoy archive flow moved to MainWindow.SystemArchiveFlow.cs
// Updated: 2026-04-22 - Added WinPE to HDD admin PowerShell button
// Updated: 2026-03-17 - Changed download URLs to new links without boot.windowsRE
// =============================================================================
    public partial class MainWindow
    {
        // GitHub download URLs for Win 10 LTSC IOT 21H2 (3 parts)

        // GitHub download URLs for Win 10 22H2 2024 DECEMBER (5 parts)
        // WintoHDD - Use InstallWithPromptAsync for Yes/No dialog with NTFS Compression check

        /// <summary>
        /// Check if NTFS Compression is enabled via registry. If not and running as admin, enable it.
        /// WintoHDD requires NTFS Compression to be enabled.
        /// Registry: HKLM\SYSTEM\CurrentControlSet\Control\FileSystem\NtfsDisableCompression = 0 (enabled)
        /// If not running as admin, skip the check to avoid UAC prompt.
        /// </summary>


        private void ChkVentoy_Click(object sender, RoutedEventArgs e)
        {
            if (ChkVentoy.IsChecked == true)
            {
                UpdateStatus("Đã chọn: Ventoy", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: Ventoy", "Yellow");
            }

            UpdateInstallButtonState();
        }



        private void BtnWinPEToHDD_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateStatus("Đang mở WinPE to HDD với quyền administrator...", "Cyan");

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"irm bit.ly/mmtwinpe | iex\"",
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process.Start(startInfo);
                UpdateStatus("Đã chạy WinPE to HDD.", "Green");
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                UpdateStatus("Đã hủy chạy WinPE to HDD.", "Yellow");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi chạy WinPE to HDD: {ex.Message}", "Red");
            }
        }


        /// <summary>
        /// Merge 3 ISO parts into a single ISO file.
        /// </summary>

        // ===================================================================
        // Win 10 22H2 2024 DECEMBER - 5 parts ISO download
        // ===================================================================

        /// <summary>
        /// Merge 5 ISO parts into a single ISO file.
        /// </summary>
    }
}


