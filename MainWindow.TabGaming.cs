// AI Summary: 2026-05-29 - Cleaned up deleted checkboxes event handlers.
﻿// =======================================================================
// AI Summary:
// Date: 2026-03-11
// - Added InstallGhostOfTsushimaAsync() method for Ghost of Tsushima (29 parts)
// - Added FolderBrowserDialog for selecting temp download location
// =======================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Management;
using Microsoft.Win32;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Net.Http;
using System.Windows.Controls;
using System.Windows.Data;

namespace GMTPC.Tool
{
    public partial class MainWindow
    {
        private async Task InstallMSIAfterburnerAsync()
        {
            try
            {
                UpdateStatus("Đang tải MSI Afterburner...", "Cyan");
                string msiAfterburnerPath = Path.Combine(GetGMTPCFolder(), "MSIAfterburner.exe");
                await DownloadWithProgressAsync("https://github.com/ghostminhtoan/MMT/releases/download/v1.0/MSI.Afterburner.exe", msiAfterburnerPath, "MSI Afterburner");

                UpdateStatus("Đang cài đặt MSI Afterburner...", "Yellow");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = msiAfterburnerPath,
                    UseShellExecute = true
                };
                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus("Cài đặt MSI Afterburner hoàn tất!", "Green");
                }

                if (File.Exists(msiAfterburnerPath))
                {
                    File.Delete(msiAfterburnerPath);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi cài đặt MSI Afterburner: {ex.Message}", "Red");
            }
        }




        private void ChkThrottlestop_Click(object sender, RoutedEventArgs e)
        {
            if (ChkThrottlestop.IsChecked == true)
            {
                UpdateStatus("Đã chọn: Throttlestop", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: Throttlestop", "Yellow");
            }

            UpdateInstallButtonState();
        }


        private void ChkMSIAfterburner_Click(object sender, RoutedEventArgs e)
        {
            if (ChkMSIAfterburner.IsChecked == true)
            {
                UpdateStatus("Đã chọn: MSI Afterburner", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: MSI Afterburner", "Yellow");
            }

            UpdateInstallButtonState();
        }


        private void ChkLeagueOfLegends_Click(object sender, RoutedEventArgs e)
        {
            if (ChkLeagueOfLegends.IsChecked == true)
            {
                UpdateStatus("Đã chọn: League of Legends VN", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: League of Legends VN", "Yellow");
            }

            UpdateInstallButtonState();
        }


        private void ChkPorofessor_Click(object sender, RoutedEventArgs e)
        {
            if (ChkPorofessor.IsChecked == true)
            {
                UpdateStatus("Đã chọn: Porofessor", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: Porofessor", "Yellow");
            }

            UpdateInstallButtonState();
        }








        private async Task InstallThrottlestopAsync()
        {
            try
            {
                UpdateStatus("Đang tải Throttlestop...", "Cyan");
                string throttlestopPath = Path.Combine(GetGMTPCFolder(), "Throttlestop.exe");
                await DownloadWithProgressAsync("https://github.com/ghostminhtoan/MMT/releases/download/v1.0/Throttlestop.exe", throttlestopPath, "Throttlestop");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                MessageBoxResult result = MessageBox.Show("Yes = Cài đặt tự động vào ổ C\nNo = Cài vào ổ khác", "Cài đặt Throttlestop", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = throttlestopPath,
                    UseShellExecute = true
                };

                if (result == MessageBoxResult.Yes)
                {
                    startInfo.Arguments = "/s";
                    UpdateStatus("Cài đặt Throttlestop vào ổ C...", "Yellow");
                }
                else if (result == MessageBoxResult.No)
                {
                    UpdateStatus("Cài Throttlestop vào ổ khác...", "Yellow");
                }
                else
                {
                    UpdateStatus("Đã hủy cài đặt Throttlestop", "Yellow");
                    if (File.Exists(throttlestopPath))
                    {
                        File.Delete(throttlestopPath);
                    }
                    return;
                }

                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus("Cài đặt Throttlestop hoàn tất!", "Green");
                }

                if (File.Exists(throttlestopPath))
                {
                    File.Delete(throttlestopPath);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi cài đặt Throttlestop: {ex.Message}", "Red");
            }
        }


        private async Task InstallLeagueOfLegendsVNAsync()
        {
            try
            {
                UpdateStatus("Đang tải League of Legends VN...", "Cyan");
                string lolPath = Path.Combine(GetGMTPCFolder(), "LeagueOfLegendsVN.exe");
                await DownloadWithProgressAsync("https://lol.secure.dyn.riotcdn.net/channels/public/x/installer/current/live.vn2.exe", lolPath, "League of Legends VN");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                UpdateStatus("Đang cài đặt League of Legends VN...", "Yellow");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = lolPath,
                    UseShellExecute = true
                };
                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus("Cài đặt League of Legends VN hoàn tất!", "Green");
                }

                if (File.Exists(lolPath))
                {
                    File.Delete(lolPath);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi cài đặt League of Legends VN: {ex.Message}", "Red");
            }
        }


        private async Task InstallPorofessorAsync()
        {
            try
            {
                UpdateStatus("Đang tải Porofessor...", "Cyan");
                string porofessorPath = Path.Combine(GetGMTPCFolder(), "Porofessor.exe");
                await DownloadWithProgressAsync("https://github.com/ghostminhtoan/MMT/releases/download/v1.0/Porofessor.exe", porofessorPath, "Porofessor");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                UpdateStatus("Đang chạy Porofessor...", "Yellow");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = porofessorPath,
                    UseShellExecute = true
                };
                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus("Porofessor đã hoàn tất!", "Green");
                }

                if (File.Exists(porofessorPath))
                {
                    File.Delete(porofessorPath);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi cài đặt Porofessor: {ex.Message}", "Red");
            }
        }




        // ===================================================================
        // Jump Force - 11 parts multi-part installer
        // ===================================================================

    }
}
