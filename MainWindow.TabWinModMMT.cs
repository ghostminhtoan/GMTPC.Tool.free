// AI Summary: 2026-04-28 - Added Ventoy SourceForge probe/install flow and checkbox wiring
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
// MainWindow.TabWinModMMT.cs
// Updated: 2026-04-28 - Added Ventoy SourceForge probe/install flow and checkbox wiring
// Updated: 2026-04-22 - Added WinPE to HDD admin PowerShell button
// Updated: 2026-03-17 - Changed download URLs to new links without boot.windowsRE
// =============================================================================
    public partial class MainWindow
    {
        // GitHub download URLs for Win 10 LTSC IOT 21H2 (3 parts)
        private const string WIN10_LTSC_IOT_PART1_URL = "https://github.com/ghostminhtoan/MMT/releases/download/windows/LTSC.IOT.21H2.-.2021.10.-.gaming.-.Office.365.-.win.10.MMTPC.3.0.iso.001";
        private const string WIN10_LTSC_IOT_PART2_URL = "https://github.com/ghostminhtoan/MMT/releases/download/windows/LTSC.IOT.21H2.-.2021.10.-.gaming.-.Office.365.-.win.10.MMTPC.3.0.iso.002";
        private const string WIN10_LTSC_IOT_PART3_URL = "https://github.com/ghostminhtoan/MMT/releases/download/windows/LTSC.IOT.21H2.-.2021.10.-.gaming.-.Office.365.-.win.10.MMTPC.3.0.iso.003";
        private const string WIN10_LTSC_IOT_FINAL_NAME = "LTSC.IOT.21H2.-.2021.10.-.gaming.-.Office.365.-.win.10.MMTPC.3.0.iso";

        // GitHub download URLs for Win 10 22H2 2024 DECEMBER (5 parts)
        private const string WIN10_22H2_2024_DEC_PART1_URL = "https://github.com/ghostminhtoan/MMT/releases/download/windows/win.10.22h2.2024.DECEMBER.-.Office.365.-.win.10.MMTPC.4.0.iso.001";
        private const string WIN10_22H2_2024_DEC_PART2_URL = "https://github.com/ghostminhtoan/MMT/releases/download/windows/win.10.22h2.2024.DECEMBER.-.Office.365.-.win.10.MMTPC.4.0.iso.002";
        private const string WIN10_22H2_2024_DEC_PART3_URL = "https://github.com/ghostminhtoan/MMT/releases/download/windows/win.10.22h2.2024.DECEMBER.-.Office.365.-.win.10.MMTPC.4.0.iso.003";
        private const string WIN10_22H2_2024_DEC_PART4_URL = "https://github.com/ghostminhtoan/MMT/releases/download/windows/win.10.22h2.2024.DECEMBER.-.Office.365.-.win.10.MMTPC.4.0.iso.004";
        private const string WIN10_22H2_2024_DEC_PART5_URL = "https://github.com/ghostminhtoan/MMT/releases/download/windows/win.10.22h2.2024.DECEMBER.-.Office.365.-.win.10.MMTPC.4.0.iso.005";
        private const string WIN10_22H2_2024_DEC_FINAL_NAME = "win.10.22h2.2024.DECEMBER.-.Office.365.-.win.10.MMTPC.4.0.iso";
        private const string VENTOY_EXTRACT_ROOT = @"C:\Ventoy";

        // WintoHDD - Use InstallWithPromptAsync for Yes/No dialog with NTFS Compression check
        private async Task InstallWintoHDDAsync()
        {
            // Check and enable NTFS Compression before installation (only if running as admin)
            await CheckAndEnableNTFSCompression();

            string gmtPCFolder = GetGMTPCFolder();
            string wintoHddPath = Path.Combine(gmtPCFolder, "wintohdd.exe");
            await InstallWithPromptAsync(WINTOHDD_DOWNLOAD_URL, wintoHddPath, WINTOHDD_INSTALL_ARGUMENTS, "WintoHDD");
        }

        /// <summary>
        /// Check if NTFS Compression is enabled via registry. If not and running as admin, enable it.
        /// WintoHDD requires NTFS Compression to be enabled.
        /// Registry: HKLM\SYSTEM\CurrentControlSet\Control\FileSystem\NtfsDisableCompression = 0 (enabled)
        /// If not running as admin, skip the check to avoid UAC prompt.
        /// </summary>
        private async Task CheckAndEnableNTFSCompression()
        {
            try
            {
                UpdateStatus("Đang kiểm tra NTFS Compression...", "Cyan");
                await Task.Delay(500);

                // Check if running as administrator
                bool isAdmin = IsRunningAsAdministrator();

                if (!isAdmin)
                {
                    // Not running as admin - skip NTFS compression check to avoid UAC prompt
                    // WintoHDD will handle this itself with its own UAC prompt
                    UpdateStatus("Không có quyền admin, bỏ qua kiểm tra NTFS Compression.", "Gray");
                    await Task.Delay(500);
                    return;
                }

                // Check NTFS Compression status via registry
                // NtfsDisableCompression = 0 means compression is ENABLED
                // NtfsDisableCompression = 1 means compression is DISABLED
                const string registryPath = @"SYSTEM\CurrentControlSet\Control\FileSystem";
                const string valueName = "NtfsDisableCompression";

                int currentValue = 1; // Default to disabled (safe assumption)
                try
                {
                    using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryPath))
                    {
                        if (key != null)
                        {
                            object value = key.GetValue(valueName);
                            if (value != null)
                            {
                                currentValue = Convert.ToInt32(value);
                            }
                        }
                    }
                }
                catch
                {
                    // Cannot read registry - assume compression is disabled
                    currentValue = 1;
                }

                bool isCompressionEnabled = (currentValue == 0);

                if (!isCompressionEnabled)
                {
                    UpdateStatus("NTFS Compression đang tắt. Đang bật...", "Yellow");
                    await Task.Delay(500);

                    // Enable NTFS Compression by setting NtfsDisableCompression = 0
                    ProcessStartInfo enableStartInfo = new ProcessStartInfo
                    {
                        FileName = "reg.exe",
                        Arguments = @"ADD HKLM\SYSTEM\CurrentControlSet\Control\FileSystem /v NtfsDisableCompression /t REG_DWORD /d 0 /f",
                        UseShellExecute = true,
                        Verb = "runas" // Run as administrator - required for writing to HKLM
                    };

                    Process enableProcess = Process.Start(enableStartInfo);
                    if (enableProcess != null)
                    {
                        await Task.Run(() => enableProcess.WaitForExit());
                        UpdateStatus("Đã bật NTFS Compression (set registry về 0)!", "Green");
                        await Task.Delay(1000);
                    }
                }
                else
                {
                    UpdateStatus("NTFS Compression đã được bật (registry = 0).", "Green");
                }
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                // User canceled UAC prompt - skip NTFS compression check
                UpdateStatus("Đã bỏ qua kiểm tra NTFS Compression (người dùng hủy UAC).", "Yellow");
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi kiểm tra/bật NTFS Compression: {ex.Message}", "Red");
                await Task.Delay(500);
            }
        }

        private void ChkWintoHDD_Click(object sender, RoutedEventArgs e)
        {
            if (ChkWintoHDD.IsChecked == true)
            {
                UpdateStatus("Đã chọn: WintoHDD", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: WintoHDD", "Yellow");
            }

            UpdateInstallButtonState();
        }

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

        private void ChkWin10LtscIot21H2_Click(object sender, RoutedEventArgs e)
        {
            UpdateInstallButtonState();
        }

        private void ChkWin10_22H2_2024_December_Click(object sender, RoutedEventArgs e)
        {
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

        private async Task InstallVentoyAsync()
        {
            try
            {
                Directory.CreateDirectory(VENTOY_EXTRACT_ROOT);

                UpdateStatus("Đang probe version Ventoy mới nhất...", "Cyan");
                string latestVersionFolderUrl = await GetLatestVentoyVersionFolderUrlAsync();
                if (string.IsNullOrEmpty(latestVersionFolderUrl))
                {
                    throw new InvalidOperationException("Không tìm thấy version Ventoy mới nhất.");
                }

                string latestVersionName = GetVentoyVersionNameFromUrl(latestVersionFolderUrl);
                UpdateStatus($"Đã chọn Ventoy {latestVersionName}", "Green");

                UpdateStatus("Đang probe file Ventoy windows.zip...", "Cyan");
                Tuple<string, string> ventoyZipDownloadInfo = await GetVentoyWindowsZipDownloadInfoAsync(latestVersionFolderUrl);
                if (ventoyZipDownloadInfo == null || string.IsNullOrEmpty(ventoyZipDownloadInfo.Item1))
                {
                    throw new InvalidOperationException("Không tìm thấy file Ventoy windows.zip.");
                }

                string ventoyZipDownloadUrl = ventoyZipDownloadInfo.Item1;
                string zipFileName = ventoyZipDownloadInfo.Item2;
                if (string.IsNullOrEmpty(zipFileName))
                {
                    zipFileName = $"ventoy-{latestVersionName.TrimStart('v')}-windows.zip";
                }

                string versionFolderPath = Path.Combine(VENTOY_EXTRACT_ROOT, latestVersionName);
                string zipPath = Path.Combine(VENTOY_EXTRACT_ROOT, zipFileName);

                if (Directory.Exists(versionFolderPath))
                {
                    try
                    {
                        Directory.Delete(versionFolderPath, true);
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus($"Không xóa được folder Ventoy cũ: {ex.Message}", "Orange");
                    }
                }

                UpdateStatus($"Đã tìm thấy file: {zipFileName}", "Cyan");
                UpdateStatus("Đang tải Ventoy windows.zip...", "Cyan");
                await DownloadWithProgressAsync(ventoyZipDownloadUrl, zipPath, "Ventoy");

                UpdateStatus("Đang giải nén Ventoy...", "Cyan");
                Directory.CreateDirectory(versionFolderPath);
                ZipFile.ExtractToDirectory(zipPath, versionFolderPath);

                string ventoyExePath = FindVentoy2DiskExe(versionFolderPath);
                if (string.IsNullOrEmpty(ventoyExePath))
                {
                    throw new InvalidOperationException("Không tìm thấy ventoy2disk.exe sau khi giải nén.");
                }

                UpdateStatus("Đang mở Ventoy2Disk với quyền administrator...", "Cyan");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = ventoyExePath,
                    WorkingDirectory = Path.GetDirectoryName(ventoyExePath),
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process process = Process.Start(startInfo);
                if (process != null)
                {
                    UpdateStatus("Ventoy2Disk đã được mở!", "Green");
                }
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                UpdateStatus("Đã hủy mở Ventoy2Disk.", "Yellow");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi cài đặt Ventoy: {ex.Message}", "Red");
            }
        }

        private async Task<string> GetLatestVentoyVersionFolderUrlAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("GMTPC-Tool");
                string html = await client.GetStringAsync(VENTOY_SOURCEFORGE_FILES_URL);

                Version bestVersion = null;
                string bestUrl = null;

                MatchCollection matches = Regex.Matches(html ?? string.Empty, @"href=""(?<href>[^""]*/files/v(?<ver>\d+\.\d+\.\d+)/?)""", RegexOptions.IgnoreCase);
                if (matches == null || matches.Count == 0)
                {
                    return null;
                }

                foreach (Match match in matches)
                {
                    string versionText = "v" + match.Groups["ver"].Value;
                    Version version = ParseVentoyVersion(versionText);
                    if (version == null)
                    {
                        continue;
                    }

                    if (bestVersion == null || version.CompareTo(bestVersion) > 0)
                    {
                        bestVersion = version;
                        bestUrl = NormalizeSourceForgeUrl(match.Groups["href"].Value);
                    }
                }

                return bestUrl;
            }
        }

        private async Task<Tuple<string, string>> GetVentoyWindowsZipDownloadInfoAsync(string versionFolderUrl)
        {
            if (string.IsNullOrEmpty(versionFolderUrl))
            {
                return null;
            }

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("GMTPC-Tool");
                string html = await client.GetStringAsync(versionFolderUrl);

                MatchCollection matches = Regex.Matches(html ?? string.Empty, @"href=""(?<href>[^""]*ventoy-(?<ver>\d+\.\d+\.\d+)-windows\.zip[^""]*)""", RegexOptions.IgnoreCase);
                if (matches == null || matches.Count == 0)
                {
                    return null;
                }

                foreach (Match match in matches)
                {
                    string fileName = $"ventoy-{match.Groups["ver"].Value}-windows.zip";
                    return Tuple.Create(NormalizeSourceForgeDownloadUrl(match.Groups["href"].Value), fileName);
                }
            }

            return null;
        }

        private static Version ParseVentoyVersion(string versionName)
        {
            Match match = Regex.Match(versionName ?? string.Empty, @"^v(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)$");
            if (!match.Success)
            {
                return null;
            }

            return new Version(
                int.Parse(match.Groups["major"].Value),
                int.Parse(match.Groups["minor"].Value),
                int.Parse(match.Groups["patch"].Value));
        }

        private static string GetVentoyVersionNameFromUrl(string versionFolderUrl)
        {
            if (string.IsNullOrEmpty(versionFolderUrl))
            {
                return string.Empty;
            }

            string trimmed = versionFolderUrl.TrimEnd('/');
            int lastSlashIndex = trimmed.LastIndexOf('/');
            return lastSlashIndex >= 0 ? trimmed.Substring(lastSlashIndex + 1) : trimmed;
        }

        private static string NormalizeSourceForgeUrl(string href)
        {
            if (string.IsNullOrEmpty(href))
            {
                return href;
            }

            string url = href;
            if (url.StartsWith("//", StringComparison.Ordinal))
            {
                url = "https:" + url;
            }
            else if (url.StartsWith("/", StringComparison.Ordinal))
            {
                url = "https://sourceforge.net" + url;
            }

            return url;
        }

        private static string NormalizeSourceForgeDownloadUrl(string href)
        {
            string url = NormalizeSourceForgeUrl(href);
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }

            if (!url.EndsWith("/download", StringComparison.OrdinalIgnoreCase))
            {
                url = url.TrimEnd('/') + "/download";
            }

            return url;
        }

        private static string FindVentoy2DiskExe(string rootFolder)
        {
            if (string.IsNullOrEmpty(rootFolder) || !Directory.Exists(rootFolder))
            {
                return null;
            }

            string[] candidates = Directory.GetFiles(rootFolder, "ventoy2disk.exe", SearchOption.AllDirectories);
            if (candidates != null && candidates.Length > 0)
            {
                return candidates[0];
            }

            candidates = Directory.GetFiles(rootFolder, "Ventoy2Disk.exe", SearchOption.AllDirectories);
            if (candidates != null && candidates.Length > 0)
            {
                return candidates[0];
            }

            return null;
        }

        private async Task InstallWin10LtscIot21H2Async()
        {
            string gmtPCFolder = GetGMTPCFolder();
            string part1Path = Path.Combine(gmtPCFolder, "LTSC.IOT.21H2.-.2021.10.-.gaming.-.Office.365.-.win.10.MMTPC.3.0.iso.001");
            string part2Path = Path.Combine(gmtPCFolder, "LTSC.IOT.21H2.-.2021.10.-.gaming.-.Office.365.-.win.10.MMTPC.3.0.iso.002");
            string part3Path = Path.Combine(gmtPCFolder, "LTSC.IOT.21H2.-.2021.10.-.gaming.-.Office.365.-.win.10.MMTPC.3.0.iso.003");
            string finalIsoPath = Path.Combine(gmtPCFolder, WIN10_LTSC_IOT_FINAL_NAME);

            UpdateStatus("Đang tải về 3 file từ GitHub (ổ C)...", "Cyan");

            try
            {
                // Download all 3 parts sequentially with progress
                UpdateStatus("Đang tải phần 1/3...", "Cyan");
                await DownloadWithProgressAsync(WIN10_LTSC_IOT_PART1_URL, part1Path, "Phần 1/3 - GitHub");

                UpdateStatus("Đang tải phần 2/3...", "Cyan");
                await DownloadWithProgressAsync(WIN10_LTSC_IOT_PART2_URL, part2Path, "Phần 2/3 - GitHub");

                UpdateStatus("Đang tải phần 3/3...", "Cyan");
                await DownloadWithProgressAsync(WIN10_LTSC_IOT_PART3_URL, part3Path, "Phần 3/3 - GitHub");

                UpdateStatus("Tải xong 3 phần! Đang gộp file...", "Cyan");

                // Merge the 3 parts into final ISO and delete parts after merging
                await MergeIsoPartsAsync(part1Path, part2Path, part3Path, finalIsoPath, deleteParts: true);

                UpdateStatus("Gộp file thành công! Đang mở thư mục và file ISO...", "Green");
                
                // Open folder with the final ISO
                Process.Start("explorer.exe", $"/select,{finalIsoPath}");

                // Mount/open the ISO
                Process.Start(new ProcessStartInfo { FileName = finalIsoPath, UseShellExecute = true });
            }
            catch (OperationCanceledException)
            {
                UpdateStatus("Đã hủy tải Win 10 LTSC IoT 21H2.", "Yellow");
                throw;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi tải Win 10 LTSC IoT 21H2: {ex.Message}", "Red");
                throw;
            }
        }

        /// <summary>
        /// Merge 3 ISO parts into a single ISO file.
        /// </summary>
        private async Task MergeIsoPartsAsync(string part1Path, string part2Path, string part3Path, string outputPath, bool deleteParts = true)
        {
            string[] parts = { part1Path, part2Path, part3Path };

            using (var outputFs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                foreach (string partPath in parts)
                {
                    using (var inputFs = new FileStream(partPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        await inputFs.CopyToAsync(outputFs);
                    }
                }
            }

            // Delete the 3 part files after successful merge
            if (deleteParts)
            {
                foreach (string partPath in parts)
                {
                    if (File.Exists(partPath))
                    {
                        try
                        {
                            File.Delete(partPath);
                            UpdateStatus($"Đã xóa file {Path.GetFileName(partPath)}", "Gray");
                        }
                        catch { /* Ignore delete errors */ }
                    }
                }
                UpdateStatus("Đã xóa 3 file split sau khi gộp!", "Green");
            }
        }

        // ===================================================================
        // Win 10 22H2 2024 DECEMBER - 5 parts ISO download
        // ===================================================================
        private async Task InstallWin10_22H2_2024_DecemberAsync()
        {
            string gmtPCFolder = GetGMTPCFolder();
            string part1Path = Path.Combine(gmtPCFolder, "win.10.22h2.2024.DECEMBER.-.Office.365.-.win.10.MMTPC.4.0.iso.001");
            string part2Path = Path.Combine(gmtPCFolder, "win.10.22h2.2024.DECEMBER.-.Office.365.-.win.10.MMTPC.4.0.iso.002");
            string part3Path = Path.Combine(gmtPCFolder, "win.10.22h2.2024.DECEMBER.-.Office.365.-.win.10.MMTPC.4.0.iso.003");
            string part4Path = Path.Combine(gmtPCFolder, "win.10.22h2.2024.DECEMBER.-.Office.365.-.win.10.MMTPC.4.0.iso.004");
            string part5Path = Path.Combine(gmtPCFolder, "win.10.22h2.2024.DECEMBER.-.Office.365.-.win.10.MMTPC.4.0.iso.005");
            string finalIsoPath = Path.Combine(gmtPCFolder, WIN10_22H2_2024_DEC_FINAL_NAME);

            UpdateStatus("Đang tải về 5 file từ GitHub (ổ C)...", "Cyan");

            try
            {
                // Download all 5 parts sequentially with progress
                UpdateStatus("Đang tải phần 1/5...", "Cyan");
                await DownloadWithProgressAsync(WIN10_22H2_2024_DEC_PART1_URL, part1Path, "Phần 1/5 - GitHub");

                UpdateStatus("Đang tải phần 2/5...", "Cyan");
                await DownloadWithProgressAsync(WIN10_22H2_2024_DEC_PART2_URL, part2Path, "Phần 2/5 - GitHub");

                UpdateStatus("Đang tải phần 3/5...", "Cyan");
                await DownloadWithProgressAsync(WIN10_22H2_2024_DEC_PART3_URL, part3Path, "Phần 3/5 - GitHub");

                UpdateStatus("Đang tải phần 4/5...", "Cyan");
                await DownloadWithProgressAsync(WIN10_22H2_2024_DEC_PART4_URL, part4Path, "Phần 4/5 - GitHub");

                UpdateStatus("Đang tải phần 5/5...", "Cyan");
                await DownloadWithProgressAsync(WIN10_22H2_2024_DEC_PART5_URL, part5Path, "Phần 5/5 - GitHub");

                UpdateStatus("Tải xong 5 phần! Đang gộp file...", "Cyan");

                // Merge the 5 parts into final ISO and delete parts after merging
                await MergeIsoPartsAsync(part1Path, part2Path, part3Path, part4Path, part5Path, finalIsoPath, deleteParts: true);

                UpdateStatus("Gộp file thành công! Đang mở thư mục và file ISO...", "Green");

                // Open folder with the final ISO
                Process.Start("explorer.exe", $"/select,{finalIsoPath}");

                // Mount/open the ISO
                Process.Start(new ProcessStartInfo { FileName = finalIsoPath, UseShellExecute = true });
            }
            catch (OperationCanceledException)
            {
                UpdateStatus("Đã hủy tải Win 10 22H2 2024 DECEMBER.", "Yellow");
                throw;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi tải Win 10 22H2 2024 DECEMBER: {ex.Message}", "Red");
                throw;
            }
        }

        /// <summary>
        /// Merge 5 ISO parts into a single ISO file.
        /// </summary>
        private async Task MergeIsoPartsAsync(string part1Path, string part2Path, string part3Path, string part4Path, string part5Path, string outputPath, bool deleteParts = true)
        {
            string[] parts = { part1Path, part2Path, part3Path, part4Path, part5Path };

            using (var outputFs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                foreach (string partPath in parts)
                {
                    using (var inputFs = new FileStream(partPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        await inputFs.CopyToAsync(outputFs);
                    }
                }
            }

            // Delete the 5 part files after successful merge
            if (deleteParts)
            {
                foreach (string partPath in parts)
                {
                    if (File.Exists(partPath))
                    {
                        try
                        {
                            File.Delete(partPath);
                            UpdateStatus($"Đã xóa file {Path.GetFileName(partPath)}", "Gray");
                        }
                        catch { /* Ignore delete errors */ }
                    }
                }
                UpdateStatus("Đã xóa 5 file split sau khi gộp!", "Green");
            }
        }
    }
}
