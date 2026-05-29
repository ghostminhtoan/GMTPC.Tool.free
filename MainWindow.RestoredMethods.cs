// AI Summary: 2026-05-29 - Removed WinRAR installer methods and button click handler.
// AI Summary: 2026-05-29 - Removed install and activation methods, click event handlers, and cleaned up RunAutomatedProcessAsync for deleted checkboxes.
// =======================================================================
// MainWindow.RestoredMethods.cs
// Chức năng: Hàm bổ trợ, phương thức dùng chung cho toàn ứng dụng
//            (GetBrush, GetColor, GetGMTPCFolder, Defender exclusion...)
// Cập nhật gần đây:
//   - 2026-03-07: Cập nhật các Install methods sử dụng constants từ
//                 SystemArguments.cs theo AI_WORKFLOW.md
//   - 2026-03-07: Fix Java installer exit code -1 handling, use JAVA_DOWNLOAD_URL
//   - 2026-03-07: Removed Zalo installation support
//   - 2026-03-10: Fix IDM download - use DownloadWithProgressAsync (multi-segment)
//                 instead of DownloadSingleConnectionAsync
//   - 2026-03-17: Updated GetGMTPCFolder() to use _selectedTempDrivePath
// =======================================================================
// AI Summary: 2026-05-15 - Switched IDM final open to the explicit %programfiles(x86)% path with retry wait before launching
// AI Summary: 2026-05-15 - Updated IDM flow to open IDMan.exe one more time after the 5 open/close cycles
// AI Summary: 2026-05-12 - Added built-in Windows deactivation flow to capture DLV output, extract Activation ID, and run slmgr /upk x
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace GMTPC.Tool
{
    public partial class MainWindow
    {
        // ===================== Color / Brush Helpers =====================
        private SolidColorBrush GetBrush(string colorName)
        {
            Color color = GetColor(colorName);
            return new SolidColorBrush(color);
        }

        private Color GetColor(string colorName)
        {
            switch (colorName.ToLower())
            {
                case "red":     return Colors.Red;
                case "green":   return Colors.LimeGreen;
                case "yellow":  return Colors.Yellow;
                case "cyan":    return Colors.Cyan;
                case "orange":  return Colors.Orange;
                case "gray":    return Colors.Gray;
                default:        return Colors.Yellow;
            }
        }

        // ===================== GMTPC Folder Helper =====================
        /// <summary>
        /// Get the GMTPC folder path - returns the selected temp folder path
        /// This ensures all downloads use the user-selected temp folder
        /// </summary>
        private string GetGMTPCFolder()
        {
            // Use the selected temp folder path if available
            if (!string.IsNullOrEmpty(_selectedTempDrivePath))
            {
                if (!Directory.Exists(_selectedTempDrivePath))
                    Directory.CreateDirectory(_selectedTempDrivePath);

                return _selectedTempDrivePath;
            }

            // Default to LocalAppData if nothing selected
            string tempPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GMTPC", "GMTPC Tools");

            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);

            return tempPath;
        }

        // ===================== Windows Defender Exclusion =====================
        private void AddDefenderExclusion()
        {
            try
            {
                string exclusionPath = GetGMTPCFolder();
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"Add-MpPreference -ExclusionPath '{exclusionPath}' -Force\"",
                    UseShellExecute = true,
                    Verb = "runas",
                    CreateNoWindow = true
                };
                Process process = Process.Start(startInfo);
                if (process != null) process.WaitForExit();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi thêm exclusion cho Windows Defender: {ex.Message}", "Red");
            }
        }

        private void RemoveDefenderExclusion()
        {
            try
            {
                string exclusionPath = GetGMTPCFolder();
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"Remove-MpPreference -ExclusionPath '{exclusionPath}' -Force\"",
                    UseShellExecute = true,
                    Verb = "runas",
                    CreateNoWindow = true
                };
                Process process = Process.Start(startInfo);
                if (process != null) process.WaitForExit();

                if (Directory.Exists(exclusionPath))
                    Directory.Delete(exclusionPath, true);
            }
            catch { }
        }

        // ===================== Automated Process =====================
        private async Task StartAutomatedProcessAsync()
        {
            await Task.Delay(500, _cancellationTokenSource?.Token ?? CancellationToken.None);
            await RunAutomatedProcessAsync();
        }

        private void ScrollToBottom() { /* Không dùng */ }

        private string GetSlmgrVbsPath()
        {
            string windowsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

            if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess)
            {
                string sysnativePath = Path.Combine(windowsFolder, "Sysnative", "slmgr.vbs");
                if (File.Exists(sysnativePath))
                {
                    return sysnativePath;
                }
            }

            return Path.Combine(windowsFolder, "System32", "slmgr.vbs");
        }

        private string ExtractActivationIdFromSlmgrOutput(string output)
        {
            if (string.IsNullOrWhiteSpace(output))
            {
                return null;
            }

            string[] labels =
            {
                "Activation ID",
                "ID kích hoạt",
                "ActivationId"
            };

            foreach (string label in labels)
            {
                int index = output.IndexOf(label, StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                {
                    continue;
                }

                string snippet = output.Substring(index);
                Match match = Regex.Match(snippet, @"(?i)" + Regex.Escape(label) + @"\s*[:=]\s*([0-9a-fA-F]{8}(?:-[0-9a-fA-F]{4}){3}-[0-9a-fA-F]{12})");
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }

                match = Regex.Match(snippet, @"([0-9a-fA-F]{8}(?:-[0-9a-fA-F]{4}){3}-[0-9a-fA-F]{12})");
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }

            return null;
        }

        private void DeactivateWindows()
        {
            UpdateStatus("Đang gỡ kích hoạt Windows...", "Cyan");

            string slmgrPath = GetSlmgrVbsPath();
            string cscriptPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "cscript.exe");
            string outputPath = Path.Combine(GetGMTPCFolder(), "SLMGR_DLV_OUTPUT.txt");
            string upkOutputPath = Path.Combine(GetGMTPCFolder(), "SLMGR_UPK_OUTPUT.txt");

            try
            {
                string dlvCommand = $"/c \"\"{cscriptPath}\" //nologo \"{slmgrPath}\" /dlv > \"{outputPath}\" 2>&1\"";
                ProcessStartInfo dlvStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = dlvCommand,
                    UseShellExecute = true,
                    Verb = "runas",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (Process process = Process.Start(dlvStartInfo))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                    }
                }

                string dlvOutput = File.Exists(outputPath) ? File.ReadAllText(outputPath) : string.Empty;
                if (string.IsNullOrWhiteSpace(dlvOutput))
                {
                    UpdateStatus("Không đọc được kết quả slmgr /DLV", "Red");
                    return;
                }

                string activationId = ExtractActivationIdFromSlmgrOutput(dlvOutput);
                if (string.IsNullOrWhiteSpace(activationId))
                {
                    Dispatcher.Invoke(() => Clipboard.SetText(dlvOutput));
                    UpdateStatus("Đã copy kết quả slmgr /DLV nhưng không tìm thấy Activation ID", "Yellow");
                    return;
                }

                Dispatcher.Invoke(() =>
                {
                    Clipboard.SetText(dlvOutput + Environment.NewLine + Environment.NewLine + "Activation ID: " + activationId);
                });

                UpdateStatus($"Đã copy kết quả slmgr /DLV và Activation ID: {activationId}", "Green");

                string upkCommand = $"/c \"\"{cscriptPath}\" //nologo \"{slmgrPath}\" /upk {activationId} > \"{upkOutputPath}\" 2>&1\"";
                ProcessStartInfo upkStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = upkCommand,
                    UseShellExecute = true,
                    Verb = "runas",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (Process process = Process.Start(upkStartInfo))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                    }
                }

                UpdateStatus("Đã chạy slmgr /upk x", "Green");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi gỡ kích hoạt Windows: {ex.Message}", "Red");
            }
            finally
            {
                try
                {
                    if (File.Exists(outputPath))
                    {
                        File.Delete(outputPath);
                    }
                }
                catch
                {
                }

                try
                {
                    if (File.Exists(upkOutputPath))
                    {
                        File.Delete(upkOutputPath);
                    }
                }
                catch
                {
                }
            }
        }

        private void PauseWindowsUpdate()
        {
            UpdateStatus("Đang truy cập tính năng Pause Windows Update...", "Cyan");
            string pauseUpdateScriptPath = Path.Combine(GetGMTPCFolder(), "pause.update.win.11.ps1");
            try
            {
                using (System.Net.WebClient client = new System.Net.WebClient())
                {
                     client.DownloadFile("https://github.com/ghostminhtoan/MMT/releases/download/test/pause.update.win.11.ps1", pauseUpdateScriptPath);
                }
                ProcessStartInfo startInfo = new ProcessStartInfo 
                { 
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -File \"{pauseUpdateScriptPath}\"",
                    UseShellExecute = true, 
                    Verb = "runas" 
                };
                Process process = Process.Start(startInfo);
                if (process != null)
                {
                    process.WaitForExit();
                }
                UpdateStatus("Đã mở công cụ Pause Windows Update", "Green");
            }
            catch (Exception ex) { UpdateStatus($"Lỗi: {ex.Message}", "Red"); }
        }




        /// <summary>
        /// Lấy link tải BID mới nhất từ bulkimagedownloader.com
        /// Tìm link có dạng: bid_*_*_setup_x64.exe
        /// </summary>


        /// <summary>
        /// Kills all processes with the specified name
        /// </summary>
        private void KillProcessByName(string processName)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(processName);
                foreach (Process process in processes)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                            process.WaitForExit();
                        }
                    }
                    catch
                    {
                        // Ignore if process cannot be killed
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            catch
            {
                // Ignore if cannot get processes by name
            }
        }

        private async Task<bool> WaitForFileAsync(string filePath, int timeoutMilliseconds, int pollIntervalMilliseconds)
        {
            int elapsed = 0;
            while (elapsed < timeoutMilliseconds)
            {
                if (File.Exists(filePath))
                {
                    return true;
                }

                await Task.Delay(pollIntervalMilliseconds);
                elapsed += pollIntervalMilliseconds;
            }

            return File.Exists(filePath);
        }

        private void StartProcessWithShell(string filePath)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                WorkingDirectory = Path.GetDirectoryName(filePath),
                UseShellExecute = true
            });
        }

        private async Task InstallVcredistAsync()
        {
            UpdateStatus("Đang tải Vcredist...", "Cyan");
            string vcredistPath = Path.Combine(GetGMTPCFolder(), "vcredist.all.in.one.by.MMT.Windows.Tech.exe");
            try
            {
                await DownloadWithProgressAsync(VCREDIST_DOWNLOAD_URL, vcredistPath, "Vcredist");
                Dispatcher.Invoke(() => { DownloadProgressBar.Value = 0; ProgressTextBlock.Text = ""; SpeedTextBlock.Text = ""; });
                UpdateStatus("Đang cài đặt Vcredist ( " + VCREDIST_INSTALL_ARGUMENTS + " )...", "Yellow");
                ProcessStartInfo startInfo = new ProcessStartInfo { FileName = vcredistPath, Arguments = VCREDIST_INSTALL_ARGUMENTS, UseShellExecute = true };
                Process process = Process.Start(startInfo);
                if (process != null) { await Task.Run(() => process.WaitForExit()); UpdateStatus("Cài đặt Vcredist hoàn tất.", "Green"); }
                if (File.Exists(vcredistPath)) File.Delete(vcredistPath);
            } catch (Exception ex) { UpdateStatus($"Lỗi: {ex.Message}", "Red"); }
        }

        private async Task InstallDirectXAsync()
        {
            UpdateStatus("Đang tải DirectX...", "Cyan");
            string directxPath = Path.Combine(GetGMTPCFolder(), "DirectX.exe");
            try
            {
                await DownloadWithProgressAsync(DIRECTX_DOWNLOAD_URL, directxPath, "DirectX");
                Dispatcher.Invoke(() => { DownloadProgressBar.Value = 0; ProgressTextBlock.Text = ""; SpeedTextBlock.Text = ""; });
                UpdateStatus("Đang cài đặt DirectX ( " + DIRECTX_INSTALL_ARGUMENTS + " )...", "Yellow");
                ProcessStartInfo startInfo = new ProcessStartInfo { FileName = directxPath, Arguments = DIRECTX_INSTALL_ARGUMENTS, UseShellExecute = true };
                Process process = Process.Start(startInfo);
                if (process != null) { await Task.Run(() => process.WaitForExit()); UpdateStatus("Cài đặt DirectX hoàn tất.", "Green"); }
                if (File.Exists(directxPath)) File.Delete(directxPath);
            } catch (Exception ex) { UpdateStatus($"Lỗi: {ex.Message}", "Red"); }
        }

        private Task InstallJavaAsync()
        {
            BtnJava_Click(null, null);
            return Task.CompletedTask;
        }

        private Task InstallOpenALAsync()
        {
            BtnOpenAL_Click(null, null);
            return Task.CompletedTask;
        }

        private async Task WaitForIDM2TmpFileToDisappear()
        {
            string tempFolder = Path.Combine(Path.GetTempPath(), "IDM");
            for (int i = 0; i < 50; i++) // 5 seconds timeout
            {
                try
                {
                    if (Directory.Exists(tempFolder) && Directory.GetFiles(tempFolder, "IDM*.tmp").Length > 0)
                    {
                        await Task.Delay(100);
                    }
                    else break;
                }
                catch { break; }
            }
        }

        private void BtnVcredist_Click(object sender, RoutedEventArgs e)
        {
            ChkVcredist.IsChecked = true;
            _ = InstallVcredistAsync();
        }


        private async Task RunAutomatedProcessAsync()
        
        {
            await Task.CompletedTask;
        }


        private void BtnPauseWindowsUpdate_Click(object sender, RoutedEventArgs e)
        {
            ChkPauseWindowsUpdate.IsChecked = true;
            _ = Task.Run(() => PauseWindowsUpdate());
        }







        // ===================== TabPopular — Button Click Handlers =====================
        private async void BtnDirectX_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Đang tải DirectX...", "Cyan");
            string directXPath = Path.Combine(GetGMTPCFolder(), "directx_installer.exe");
            try
            {
                await DownloadWithProgressAsync("https://download.microsoft.com/download/1/7/1/1718CCC4-6315-4D8E-9543-8E28A4E99C9C/dxwebsetup.exe", directXPath, "DirectX Installer");
                Dispatcher.Invoke(() => { DownloadProgressBar.Value = 0; ProgressTextBlock.Text = ""; SpeedTextBlock.Text = ""; });
                UpdateStatus("Đang chạy DirectX installer với lệnh /q...", "Yellow");
                ProcessStartInfo startInfo = new ProcessStartInfo { FileName = directXPath, Arguments = "/q", UseShellExecute = true };
                Process process = Process.Start(startInfo);
                if (process != null) { await Task.Run(() => process.WaitForExit()); UpdateStatus(process.ExitCode == 0 ? "Cài đặt DirectX thành công!" : $"Mã lỗi: {process.ExitCode}", process.ExitCode == 0 ? "Green" : "Red"); }
                if (File.Exists(directXPath)) File.Delete(directXPath);
            }
            catch (Exception ex) { UpdateStatus($"Lỗi: {ex.Message}", "Red"); }
        }

        private async void BtnJava_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Đang tải Java...", "Cyan");
            string javaInstallerPath = Path.Combine(GetGMTPCFolder(), "java_installer.exe");
            try
            {
                await DownloadWithProgressAsync(JAVA_DOWNLOAD_URL, javaInstallerPath, "Java Installer");
                UpdateStatus("Đang chạy Java installer...", "Yellow");
                ProcessStartInfo startInfo = new ProcessStartInfo { FileName = javaInstallerPath, Arguments = JAVA_INSTALL_ARGUMENTS, UseShellExecute = true };
                Process process = Process.Start(startInfo);
                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    // Java web installer thường trả -1 hoặc 16389 khi thành công
                    bool isSuccess = process.ExitCode == 0 || process.ExitCode == -1 || process.ExitCode == 16389;
                    UpdateStatus(isSuccess ? "Cài đặt Java thành công!" : $"Mã lỗi: {process.ExitCode}", isSuccess ? "Green" : "Red");
                }
                if (File.Exists(javaInstallerPath)) File.Delete(javaInstallerPath);
            }
            catch (Exception ex) { UpdateStatus($"Lỗi: {ex.Message}", "Red"); }
        }

        private async void BtnOpenAL_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Đang tải OpenAL...", "Cyan");
            string openALInstallerPath = Path.Combine(GetGMTPCFolder(), "OpenAL.exe");
            try
            {
                await DownloadWithProgressAsync("https://github.com/ghostminhtoan/MMT/releases/download/v1.0/OpenAL.exe", openALInstallerPath, "OpenAL Installer");
                UpdateStatus("Đang chạy OpenAL installer với lệnh /s...", "Yellow");
                ProcessStartInfo startInfo = new ProcessStartInfo { FileName = openALInstallerPath, Arguments = "/s", UseShellExecute = true };
                Process process = Process.Start(startInfo);
                if (process != null) { await Task.Run(() => process.WaitForExit()); UpdateStatus(process.ExitCode == 0 ? "Cài đặt OpenAL thành công!" : $"Mã lỗi: {process.ExitCode}", process.ExitCode == 0 ? "Green" : "Red"); }
                if (File.Exists(openALInstallerPath)) File.Delete(openALInstallerPath);
            }
            catch (Exception ex) { UpdateStatus($"Lỗi: {ex.Message}", "Red"); }
        }
    }
}
