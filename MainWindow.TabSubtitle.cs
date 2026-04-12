using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        /*
         * AI Summary:
         * Date: 2026-04-13
         * - Added ChkSubtitleDraftGMTPC and InstallSubtitleDraftGMTPCAsync with download to C:\, desktop shortcut, and open file
         * Date: 2026-03-29 (3)
         * - Added 3 new checkboxes: ChkBoilsoftVideoSplitter, ChkVibe, ChkMKVToolNix
         * - Using InstallWithPromptAsync mechanism (Yes/No dialog)
         * Date: 2026-03-29 (2)
         * - Added Desktop shortcut creation for VidCoder after download
         * Date: 2026-03-29
         * - Created MainWindow.TabSubtitle.cs for Subtitle tab
         * - Added ChkVidCoder_Click, InstallVidCoderAsync with GitHub latest version probe
         * Note: ChkSubtitleEdit_Click and InstallSubtitleEditAsync remain in MainWindow.TabOffice.cs
         */

        // ===================================================================
        // TabSubtitle — VidCoder
        // ===================================================================
        private void ChkVidCoder_Click(object sender, RoutedEventArgs e)
        {
            if (ChkVidCoder.IsChecked == true)
            {
                UpdateStatus("Đã chọn: VidCoder", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: VidCoder", "Yellow");
            }

            UpdateInstallButtonState();
        }

        private async Task InstallVidCoderAsync()
        {
            try
            {
                // Bước 1: Tạo folder C:\Vidcoder nếu chưa tồn tại
                string vidCoderFolder = @"C:\Vidcoder";
                if (!Directory.Exists(vidCoderFolder))
                {
                    Directory.CreateDirectory(vidCoderFolder);
                    UpdateStatus($"Đã tạo folder {vidCoderFolder}", "Cyan");
                }

                // Bước 2: Tìm phiên bản VidCoder mới nhất từ GitHub
                UpdateStatus("Đang tìm phiên bản VidCoder mới nhất...", "Cyan");
                string latestVersion = await GetLatestVidCoderVersionAsync();
                
                if (string.IsNullOrEmpty(latestVersion))
                {
                    UpdateStatus("Không thể tìm thấy phiên bản VidCoder mới nhất!", "Red");
                    return;
                }

                UpdateStatus($"Phiên bản mới nhất: {latestVersion}", "Green");

                // Bước 3: Tải VidCoder.exe
                string vidCoderExeUrl = $"https://github.com/RandomEngy/VidCoder/releases/download/{latestVersion}/VidCoder-{latestVersion.TrimStart('v')}-Portable.exe";
                string vidCoderExePath = Path.Combine(vidCoderFolder, "VidCoder.exe");
                
                UpdateStatus($"Đang tải VidCoder {latestVersion}...", "Cyan");
                await DownloadWithProgressAsync(vidCoderExeUrl, vidCoderExePath, "VidCoder");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                // Bước 4: Tải file VidCoder.sqlite từ MMT repo
                string vidCoderSqliteUrl = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/VidCoder.sqlite";
                string vidCoderSqlitePath = Path.Combine(vidCoderFolder, "VidCoder.sqlite");

                UpdateStatus("Đang tải VidCoder.sqlite...", "Cyan");
                using (WebClient client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(vidCoderSqliteUrl, vidCoderSqlitePath);
                }

                UpdateStatus("Đã tải xong VidCoder.sqlite", "Green");

                // Bước 5: Tạo shortcut trên Desktop
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string shortcutPath = Path.Combine(desktopPath, "VidCoder.lnk");
                
                // Xóa shortcut cũ nếu tồn tại
                if (File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                }
                
                // Tạo shortcut mới sử dụng WshShell
                try
                {
                    Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                    if (shellType != null)
                    {
                        object shell = Activator.CreateInstance(shellType);
                        object shortcut = shellType.InvokeMember("CreateShortcut", System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { shortcutPath });
                        
                        // Set các thuộc tính shortcut
                        shellType.InvokeMember("TargetPath", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { vidCoderExePath });
                        shellType.InvokeMember("WorkingDirectory", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { vidCoderFolder });
                        shellType.InvokeMember("Description", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { "VidCoder - Video transcoder" });
                        shellType.InvokeMember("Save", System.Reflection.BindingFlags.InvokeMethod, null, shortcut, null);
                        
                        UpdateStatus("Đã tạo shortcut VidCoder trên Desktop", "Green");
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Không thể tạo shortcut: {ex.Message}", "Orange");
                }

                // Bước 6: Chỉ chạy file .exe sau khi tải xong SQLite
                UpdateStatus("Đang mở VidCoder...", "Cyan");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = vidCoderExePath,
                    UseShellExecute = true,
                    WorkingDirectory = vidCoderFolder
                };
                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    UpdateStatus("VidCoder đã được mở!", "Green");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi cài đặt VidCoder: {ex.Message}", "Red");
            }
        }

        /// <summary>
        /// Tìm phiên bản VidCoder mới nhất từ GitHub Releases
        /// </summary>
        private async Task<string> GetLatestVidCoderVersionAsync()
        {
            try
            {
                // Sử dụng GitHub API để lấy danh sách releases
                string apiUrl = "https://api.github.com/repos/RandomEngy/VidCoder/releases";
                
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
                request.UserAgent = "GMTPC-Tool";
                request.Accept = "application/json";

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string json = await reader.ReadToEndAsync();
                    
                    // Parse JSON đơn giản để tìm tất cả versions
                    var versions = new List<(string Version, int BuildNumber)>();
                    
                    // Tìm tất cả các tag_name có dạng v*
                    int startIndex = 0;
                    while ((startIndex = json.IndexOf("\"tag_name\":", startIndex)) != -1)
                    {
                        startIndex += "\"tag_name\":".Length;
                        int quoteStart = json.IndexOf('"', startIndex);
                        if (quoteStart == -1) break;
                        
                        quoteStart++;
                        int quoteEnd = json.IndexOf('"', quoteStart);
                        if (quoteEnd == -1) break;
                        
                        string tagName = json.Substring(quoteStart, quoteEnd - quoteStart);
                        
                        // Chỉ lấy các tag có dạng vX.Y.Z
                        if (tagName.StartsWith("v") && tagName.Length > 1)
                        {
                            // Parse version number để so sánh
                            string versionNum = tagName.TrimStart('v');
                            int buildNumber = ParseVersionToNumber(versionNum);
                            versions.Add((tagName, buildNumber));
                        }
                        
                        startIndex = quoteEnd + 1;
                    }

                    // Tìm version có số build lớn nhất
                    if (versions.Count > 0)
                    {
                        var latest = versions.OrderByDescending(v => v.BuildNumber).First();
                        return latest.Version;
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi tìm phiên bản VidCoder: {ex.Message}", "Orange");
            }

            return null;
        }

        /// <summary>
        /// Chuyển version string (X.Y.Z) thành số để so sánh
        /// </summary>
        private int ParseVersionToNumber(string version)
        {
            try
            {
                var parts = version.Split('.');
                if (parts.Length >= 3)
                {
                    int major = int.TryParse(parts[0], out var m) ? m : 0;
                    int minor = int.TryParse(parts[1], out var n) ? n : 0;
                    int build = int.TryParse(parts[2], out var b) ? b : 0;
                    
                    // Công thức: major * 1000000 + minor * 1000 + build
                    return major * 1000000 + minor * 1000 + build;
                }
            }
            catch { }

            return 0;
        }

        // ===================================================================
        // TabSubtitle — Boilsoft Video Splitter
        // ===================================================================
        private void ChkBoilsoftVideoSplitter_Click(object sender, RoutedEventArgs e)
        {
            if (ChkBoilsoftVideoSplitter.IsChecked == true)
            {
                UpdateStatus("Đã chọn: Boilsoft Video Splitter", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: Boilsoft Video Splitter", "Yellow");
            }

            UpdateInstallButtonState();
        }

        private async Task InstallBoilsoftVideoSplitterAsync()
        {
            try
            {
                UpdateStatus("Đang tải Boilsoft Video Splitter...", "Cyan");
                string boilsoftPath = Path.Combine(GetGMTPCFolder(), "Boilsoft.VideoSplitter.exe");
                await DownloadWithProgressAsync(BOILSOFT_VIDEO_SPLITTER_DOWNLOAD_URL, boilsoftPath, "Boilsoft Video Splitter");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                // Hiển thị popup để hỏi người dùng chọn cài đặt
                MessageBoxResult result = MessageBox.Show("Yes = Cài đặt tự động (silent)\nNo = Cài đặt thủ công (GUI)", "Cài đặt Boilsoft Video Splitter", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                {
                    UpdateStatus("Đã hủy cài đặt Boilsoft Video Splitter", "Yellow");
                    if (File.Exists(boilsoftPath))
                    {
                        File.Delete(boilsoftPath);
                    }
                    return;
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = boilsoftPath,
                    UseShellExecute = true
                };

                if (result == MessageBoxResult.Yes)
                {
                    // Cài đặt tự động
                    startInfo.Arguments = BOILSOFT_VIDEO_SPLITTER_INSTALL_ARGUMENTS;
                    UpdateStatus("Đang cài đặt Boilsoft Video Splitter (silent)...", "Yellow");
                }
                else
                {
                    // Cài đặt thủ công
                    UpdateStatus("Đang mở Boilsoft Video Splitter installer (thủ công)...", "Yellow");
                }

                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus("Cài đặt Boilsoft Video Splitter hoàn tất!", "Green");
                }

                if (File.Exists(boilsoftPath))
                {
                    File.Delete(boilsoftPath);
                    UpdateStatus("Đã xóa file Boilsoft.VideoSplitter.exe", "Cyan");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi cài đặt Boilsoft Video Splitter: {ex.Message}", "Red");
            }
        }

        // ===================================================================
        // TabSubtitle — Vibe
        // ===================================================================
        private void ChkVibe_Click(object sender, RoutedEventArgs e)
        {
            if (ChkVibe.IsChecked == true)
            {
                UpdateStatus("Đã chọn: Vibe", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: Vibe", "Yellow");
            }

            UpdateInstallButtonState();
        }

        private async Task InstallVibeAsync()
        {
            try
            {
                UpdateStatus("Đang tải Vibe...", "Cyan");
                string vibePath = Path.Combine(GetGMTPCFolder(), "Vibe.exe");
                await DownloadWithProgressAsync(VIBE_DOWNLOAD_URL, vibePath, "Vibe");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                // Hiển thị popup để hỏi người dùng chọn cài đặt
                MessageBoxResult result = MessageBox.Show("Yes = Cài đặt tự động (silent)\nNo = Cài đặt thủ công (GUI)", "Cài đặt Vibe", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                {
                    UpdateStatus("Đã hủy cài đặt Vibe", "Yellow");
                    if (File.Exists(vibePath))
                    {
                        File.Delete(vibePath);
                    }
                    return;
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = vibePath,
                    UseShellExecute = true
                };

                if (result == MessageBoxResult.Yes)
                {
                    // Cài đặt tự động
                    startInfo.Arguments = VIBE_INSTALL_ARGUMENTS;
                    UpdateStatus("Đang cài đặt Vibe (silent)...", "Yellow");
                }
                else
                {
                    // Cài đặt thủ công
                    UpdateStatus("Đang mở Vibe installer (thủ công)...", "Yellow");
                }

                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus("Cài đặt Vibe hoàn tất!", "Green");
                }

                if (File.Exists(vibePath))
                {
                    File.Delete(vibePath);
                    UpdateStatus("Đã xóa file Vibe.exe", "Cyan");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi cài đặt Vibe: {ex.Message}", "Red");
            }
        }

        // ===================================================================
        // TabSubtitle — MKVToolNix MKVCleaver
        // ===================================================================
        private void ChkMKVToolNix_Click(object sender, RoutedEventArgs e)
        {
            if (ChkMKVToolNix.IsChecked == true)
            {
                UpdateStatus("Đã chọn: MKVToolNix MKVCleaver", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: MKVToolNix MKVCleaver", "Yellow");
            }

            UpdateInstallButtonState();
        }

        private async Task InstallMKVToolNixAsync()
        {
            try
            {
                UpdateStatus("Đang tải MKVToolNix MKVCleaver...", "Cyan");
                string mkvtoolnixPath = Path.Combine(GetGMTPCFolder(), "MKVToolNix.MKVCleaver.exe");
                await DownloadWithProgressAsync(MKVTOOLNIX_DOWNLOAD_URL, mkvtoolnixPath, "MKVToolNix MKVCleaver");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                // Hiển thị popup để hỏi người dùng chọn cài đặt
                MessageBoxResult result = MessageBox.Show("Yes = Cài đặt tự động (silent)\nNo = Cài đặt thủ công (GUI)", "Cài đặt MKVToolNix MKVCleaver", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                {
                    UpdateStatus("Đã hủy cài đặt MKVToolNix MKVCleaver", "Yellow");
                    if (File.Exists(mkvtoolnixPath))
                    {
                        File.Delete(mkvtoolnixPath);
                    }
                    return;
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = mkvtoolnixPath,
                    UseShellExecute = true
                };

                if (result == MessageBoxResult.Yes)
                {
                    // Cài đặt tự động
                    startInfo.Arguments = MKVTOOLNIX_INSTALL_ARGUMENTS;
                    UpdateStatus("Đang cài đặt MKVToolNix MKVCleaver (silent)...", "Yellow");
                }
                else
                {
                    // Cài đặt thủ công
                    UpdateStatus("Đang mở MKVToolNix MKVCleaver installer (thủ công)...", "Yellow");
                }

                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus("Cài đặt MKVToolNix MKVCleaver hoàn tất!", "Green");
                }

                if (File.Exists(mkvtoolnixPath))
                {
                    File.Delete(mkvtoolnixPath);
                    UpdateStatus("Đã xóa file MKVToolNix.MKVCleaver.exe", "Cyan");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi cài đặt MKVToolNix MKVCleaver: {ex.Message}", "Red");
            }
        }

        // ===================================================================
        // TabSubtitle — Subtitle Draft GMTPC
        // ===================================================================
        private void ChkSubtitleDraftGMTPC_Click(object sender, RoutedEventArgs e)
        {
            if (ChkSubtitleDraftGMTPC.IsChecked == true)
            {
                UpdateStatus("Đã chọn: Subtitle Draft GMTPC", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: Subtitle Draft GMTPC", "Yellow");
            }

            UpdateInstallButtonState();
        }

        private async Task InstallSubtitleDraftGMTPCAsync()
        {
            try
            {
                // Bước 1: Tải file về ổ C:\
                string subtitleDraftFolder = @"C:\";
                string subtitleDraftExe = Path.Combine(subtitleDraftFolder, "Subtitle draft GMTPC.exe");

                UpdateStatus("Đang tải Subtitle Draft GMTPC...", "Cyan");
                await DownloadWithProgressAsync(SUBTITLE_DRAFT_GMTPC_DOWNLOAD_URL, subtitleDraftExe, "Subtitle Draft GMTPC");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                UpdateStatus("Đã tải xong Subtitle Draft GMTPC", "Green");

                // Bước 2: Tạo shortcut trên Desktop
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string shortcutPath = Path.Combine(desktopPath, "Subtitle Draft GMTPC.lnk");

                // Xóa shortcut cũ nếu tồn tại
                if (File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                }

                // Tạo shortcut mới sử dụng WshShell
                try
                {
                    Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                    if (shellType != null)
                    {
                        object shell = Activator.CreateInstance(shellType);
                        object shortcut = shellType.InvokeMember("CreateShortcut", System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { shortcutPath });

                        // Set các thuộc tính shortcut
                        shellType.InvokeMember("TargetPath", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { subtitleDraftExe });
                        shellType.InvokeMember("WorkingDirectory", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { subtitleDraftFolder });
                        shellType.InvokeMember("Description", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { "Subtitle Draft GMTPC" });
                        shellType.InvokeMember("Save", System.Reflection.BindingFlags.InvokeMethod, null, shortcut, null);

                        UpdateStatus("Đã tạo shortcut Subtitle Draft GMTPC trên Desktop", "Green");
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Không thể tạo shortcut: {ex.Message}", "Orange");
                }

                // Bước 3: Mở file
                UpdateStatus("Đang mở Subtitle Draft GMTPC...", "Cyan");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = subtitleDraftExe,
                    UseShellExecute = true,
                    WorkingDirectory = subtitleDraftFolder
                };
                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    UpdateStatus("Subtitle Draft GMTPC đã được mở!", "Green");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi cài đặt Subtitle Draft GMTPC: {ex.Message}", "Red");
            }
        }
    }
}
