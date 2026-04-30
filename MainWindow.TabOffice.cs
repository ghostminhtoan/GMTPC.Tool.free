using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;

namespace GMTPC.Tool
{
    public partial class MainWindow
    {
        /*
         * AI Summary:
         * Date: 2026-04-17
         * - Added 3 download buttons for Subtitle Edit: Vietnamese Profile, Multiple Replace Template, and Shortcut MMT XML
         * Date: 2026-03-28
         * - Added ChkSubtitleEdit_Click and InstallSubtitleEditAsync
         * Date: 2026-04-22
         * - Replaced Gouenji Fansub Fonts with GMTPC Fonts and delete-on-exit cleanup
         * Date: 2026-03-08
         * - Added ChkNotepadPlusPlus_Click and InstallNotepadPlusPlusAsync
         */

        private async void BtnActivateOffice_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Đã chọn: Tự động kích hoạt Office", "Green");
            await Task.Run(() => ActivateOffice());
        }


        // ===================== Chức năng kích hoạt Office =====================
        private void ActivateOffice()
        {
            try
            {
                UpdateStatus("Đang kích hoạt Office...", "Cyan");
                string activateOfficeCmdPath = Path.Combine(GetGMTPCFolder(), "ACTIVATE.OFFICE.cmd");

                // Tải file ACTIVATE.OFFICE.cmd
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile("https://github.com/ghostminhtoan/MMT/releases/download/activate/ACTIVATE.OFFICE.cmd", activateOfficeCmdPath);
                }
                UpdateStatus("Đã tải file ACTIVATE.OFFICE.cmd", "Cyan");

                // Chạy script với quyền admin
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = activateOfficeCmdPath,
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process.Start(startInfo);
                UpdateStatus("Đã mở cửa sổ kích hoạt Office", "Green");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi kích hoạt Office: {ex.Message}", "Red");
            }
        }


        private void ChkActivateOffice_Click(object sender, RoutedEventArgs e)
        {
            if (ChkActivateOffice.IsChecked == true)
            {
                UpdateStatus("Đã chọn: Tự động kích hoạt Office", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: Tự động kích hoạt Office", "Yellow");
            }

            UpdateInstallButtonState();
        }


        private void ChkOfficeToolPlus_Click(object sender, RoutedEventArgs e)
        {
            if (ChkOfficeToolPlus.IsChecked == true)
            {
                UpdateStatus("Đã chọn: Office Tool Plus", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: Office Tool Plus", "Yellow");
            }

            UpdateInstallButtonState();
        }


        // Helper methods for installation tasks to be called from BtnInstall_Click
        private async Task InstallOfficeToolPlusAsync()
        {
            try
            {
                string officeToolPlusRootFolder = @"C:\Office Tool Plus";
                string officeToolPlusDownloadFolder = Path.Combine(officeToolPlusRootFolder, "Download");

                if (Directory.Exists(officeToolPlusRootFolder))
                {
                    try
                    {
                        Directory.Delete(officeToolPlusRootFolder, true);
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus($"Không xóa được thư mục Office Tool Plus cũ: {ex.Message}", "Orange");
                    }
                }

                Directory.CreateDirectory(officeToolPlusDownloadFolder);
                Directory.CreateDirectory(officeToolPlusRootFolder);

                UpdateStatus("Đang lấy link Office Tool Plus mới nhất từ GitHub Releases...", "Cyan");
                string officeToolPlusZipUrl = await GetLatestOfficeToolPlusRuntimeZipUrlAsync();
                string officeToolPlusZipName = Path.GetFileName(new Uri(officeToolPlusZipUrl).LocalPath);
                string officeToolPlusZipPath = Path.Combine(officeToolPlusDownloadFolder, officeToolPlusZipName);

                UpdateStatus($"Đang tải {officeToolPlusZipName}...", "Cyan");
                await DownloadWithProgressAsync(officeToolPlusZipUrl, officeToolPlusZipPath, "Office Tool Plus");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                UpdateStatus("Đang giải nén Office Tool Plus vào ổ C...", "Cyan");
                ZipFile.ExtractToDirectory(officeToolPlusZipPath, officeToolPlusRootFolder);

                if (File.Exists(officeToolPlusZipPath))
                {
                    File.Delete(officeToolPlusZipPath);
                    UpdateStatus("Đã xóa file zip Office Tool Plus sau khi giải nén", "Cyan");
                }

                string officeToolPlusExePath = FindOfficeToolPlusExePath(officeToolPlusRootFolder);
                if (string.IsNullOrEmpty(officeToolPlusExePath))
                {
                    throw new FileNotFoundException("Không tìm thấy Office Tool Plus.exe trong thư mục đã giải nén.");
                }

                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string shortcutPath = Path.Combine(desktopPath, "Office Tool Plus.lnk");
                if (File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                }

                try
                {
                    Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                    if (shellType != null)
                    {
                        object shell = Activator.CreateInstance(shellType);
                        object shortcut = shellType.InvokeMember("CreateShortcut", System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { shortcutPath });
                        shellType.InvokeMember("TargetPath", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { officeToolPlusExePath });
                        shellType.InvokeMember("WorkingDirectory", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { Path.GetDirectoryName(officeToolPlusExePath) });
                        shellType.InvokeMember("Description", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { "Office Tool Plus" });
                        shellType.InvokeMember("Save", System.Reflection.BindingFlags.InvokeMethod, null, shortcut, null);
                        UpdateStatus("Đã tạo shortcut Office Tool Plus trên Desktop", "Green");
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Không thể tạo shortcut Office Tool Plus: {ex.Message}", "Orange");
                }

                UpdateStatus("Đang mở Office Tool Plus...", "Green");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = officeToolPlusExePath,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(officeToolPlusExePath)
                };

                Process process = Process.Start(startInfo);
                if (process != null)
                {
                    UpdateStatus("Office Tool Plus đã được mở!", "Green");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi tải hoặc cài đặt Office Tool Plus: {ex.Message}", "Red");
            }
        }

        private async Task<string> GetLatestOfficeToolPlusRuntimeZipUrlAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("GMTPC-Tool");
                    client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");

                    string json = await client.GetStringAsync(OFFICE_TOOL_PLUS_RELEASES_API_URL);
                    Match match = Regex.Match(
                        json,
                        "\"name\"\\s*:\\s*\"(?<name>Office_Tool_with_runtime_v(?<version>[^\"]+?)_x64\\.zip)\".*?\"browser_download_url\"\\s*:\\s*\"(?<url>[^\"]+)\"",
                        RegexOptions.Singleline);

                    if (match.Success)
                    {
                        string assetName = match.Groups["name"].Value;
                        string version = match.Groups["version"].Value;
                        string downloadUrl = match.Groups["url"].Value.Replace("\\/", "/");
                        UpdateStatus($"Đã tìm thấy Office Tool Plus {version}: {assetName}", "Green");
                        return downloadUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Không lấy được link Office Tool Plus mới nhất: {ex.Message}", "Yellow");
            }

            throw new InvalidOperationException("Không tìm thấy gói Office Tool Plus x64 mới nhất trên GitHub Releases.");
        }

        private string FindOfficeToolPlusExePath(string searchRoot)
        {
            if (!Directory.Exists(searchRoot))
            {
                return string.Empty;
            }

            string[] matches = Directory.GetFiles(searchRoot, "Office Tool Plus.exe", SearchOption.AllDirectories);
            if (matches != null && matches.Length > 0)
            {
                return matches[0];
            }

            return string.Empty;
        }


        // Thêm phương thức xử lý sự kiện Click cho Office Softmaker
        private void ChkOfficeSoftmaker_Click(object sender, RoutedEventArgs e)
        {
            if (ChkOfficeSoftmaker.IsChecked == true)
            {
                UpdateStatus("Đã chọn: Office Softmaker", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: Office Softmaker", "Yellow");
            }

            UpdateInstallButtonState();
        }


        // Thêm phương thức cài đặt Office Softmaker
        private async Task InstallOfficeSoftmakerAsync()
        {
            try
            {
                UpdateStatus("Đang tải Office Softmaker...", "Cyan");
                string officeSoftmakerPath = Path.Combine(GetGMTPCFolder(), "Office.Softmaker.exe");
                await DownloadWithProgressAsync("https://github.com/ghostminhtoan/MMT/releases/download/v1.0/Office.Softmaker.exe", officeSoftmakerPath, "Office Softmaker Installer");

                // Đảm bảo progress bar reset sau khi tải
                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                // Hiển thị popup để hỏi người dùng chọn cài đặt
                MessageBoxResult result = MessageBox.Show("Yes = Cài đặt tự động vào ổ C\nNo = Cài vào ổ khác", "Cài đặt tự động Office Softmaker", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = officeSoftmakerPath,
                    UseShellExecute = true
                };

                if (result == MessageBoxResult.Yes) // Cài đặt tự động vào ổ C
                {
                    startInfo.Arguments = "/passive"; // Sử dụng /passive như yêu cầu
                    UpdateStatus("1 = Cài đặt tự động vào ổ C", "Yellow");
                }
                else if (result == MessageBoxResult.No) // Cài vào ổ khác
                {
                    UpdateStatus("2 = Cài vào ổ khác", "Yellow");
                }
                else // Hủy
                {
                    UpdateStatus("Đã hủy cài đặt Office Softmaker", "Yellow");
                    if (File.Exists(officeSoftmakerPath))
                    {
                        File.Delete(officeSoftmakerPath);
                    }
                    return;
                }

                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());

                    if (process.ExitCode == 0)
                    {
                        UpdateStatus("Cài đặt Office Softmaker thành công!", "Green");
                    }
                    else
                    {
                        UpdateStatus($"Cài đặt Office Softmaker thất bại. Mã lỗi: {process.ExitCode}", "Red");
                    }
                }

                // Xóa file sau khi cài đặt xong
                if (File.Exists(officeSoftmakerPath))
                {
                    File.Delete(officeSoftmakerPath);
                    UpdateStatus("Đã xóa file Office.Softmaker.exe", "Cyan");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi cài đặt Office Softmaker: {ex.Message}", "Red");
            }
        }

        // ===================================================================
        // TabOffice — GMTPC Fonts
        // ===================================================================
        private void ChkGMTPCFonts_Click(object sender, RoutedEventArgs e)
        {
            if (ChkGMTPCFonts.IsChecked == true)
            {
                UpdateStatus("Đã chọn: GMTPC Fonts", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: GMTPC Fonts", "Yellow");
            }

            UpdateInstallButtonState();
        }

        private async Task InstallGMTPCFontsAsync()
        {
            try
            {
                UpdateStatus("Đang tải GMTPC Fonts...", "Cyan");
                string fontsPath = Path.Combine(GetGMTPCFolder(), "GMTPC-FONTS.exe");
                RegisterDownloadedFileForDeleteOnExit(fontsPath);
                await DownloadWithProgressAsync(GMTPC_FONTS_DOWNLOAD_URL, fontsPath, "GMTPC Fonts");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                UpdateStatus("Đang chạy GMTPC Fonts...", "Yellow");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = fontsPath,
                    UseShellExecute = true
                };
                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    UpdateStatus("Đã chạy GMTPC Fonts. File tải về sẽ được xóa khi tắt app.", "Green");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi chạy GMTPC Fonts: {ex.Message}", "Red");
            }
        }

        // ===================================================================
        // TabOffice — Notepad++
        // ===================================================================
        private void ChkNotepadPlusPlus_Click(object sender, RoutedEventArgs e)
        {
            if (ChkNotepadPlusPlus.IsChecked == true)
            {
                UpdateStatus("Đã chọn: Notepad++", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: Notepad++", "Yellow");
            }

            UpdateInstallButtonState();
        }

        private async Task InstallNotepadPlusPlusAsync()
        {
            try
            {
                UpdateStatus("Đang tải Notepad++...", "Cyan");
                string notepadPlusPlusPath = Path.Combine(GetGMTPCFolder(), "npp.8.9.2.Installer.x64.msi");
                await DownloadWithProgressAsync(NOTEPAD_PLUS_PLUS_DOWNLOAD_URL, notepadPlusPlusPath, "Notepad++");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                UpdateStatus("Đang cài đặt Notepad++ (passive)...", "Yellow");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "msiexec",
                    Arguments = $"/i \"{notepadPlusPlusPath}\" /passive",
                    UseShellExecute = true
                };
                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus("Cài đặt Notepad++ hoàn tất!", "Green");
                }

                if (File.Exists(notepadPlusPlusPath))
                {
                    File.Delete(notepadPlusPlusPath);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi cài đặt Notepad++: {ex.Message}", "Red");
            }
        }

        // ===================================================================
        // TabOffice — Subtitle Edit
        // ===================================================================
        private void ChkSubtitleEdit_Click(object sender, RoutedEventArgs e)
        {
            if (ChkSubtitleEdit.IsChecked == true)
            {
                UpdateStatus("Đã chọn: Subtitle Edit", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: Subtitle Edit", "Yellow");
            }

            UpdateInstallButtonState();
        }

        private async Task InstallSubtitleEditAsync()
        {
            try
            {
                UpdateStatus("Đang tải Subtitle Edit...", "Cyan");
                string subtitleEditPath = Path.Combine(GetGMTPCFolder(), "Subtitle.Edit.exe");
                await DownloadWithProgressAsync(SUBTITLE_EDIT_DOWNLOAD_URL, subtitleEditPath, "Subtitle Edit");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                // Hiển thị popup để hỏi người dùng chọn cài đặt
                MessageBoxResult result = MessageBox.Show("Yes = Cài đặt tự động (silent)\nNo = Cài đặt thủ công (GUI)", "Cài đặt Subtitle Edit", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                {
                    UpdateStatus("Đã hủy cài đặt Subtitle Edit", "Yellow");
                    if (File.Exists(subtitleEditPath))
                    {
                        File.Delete(subtitleEditPath);
                    }
                    return;
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = subtitleEditPath,
                    UseShellExecute = true
                };

                if (result == MessageBoxResult.Yes)
                {
                    // Cài đặt tự động
                    startInfo.Arguments = SUBTITLE_EDIT_INSTALL_ARGUMENTS;
                    UpdateStatus("Đang cài đặt Subtitle Edit (silent)...", "Yellow");
                }
                else
                {
                    // Cài đặt thủ công
                    UpdateStatus("Đang mở Subtitle Edit installer (thủ công)...", "Yellow");
                }

                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    // Đợi người dùng tắt installer
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus("Cài đặt Subtitle Edit hoàn tất!", "Green");
                }

                // Xóa file installer sau khi cài đặt xong
                if (File.Exists(subtitleEditPath))
                {
                    File.Delete(subtitleEditPath);
                    UpdateStatus("Đã xóa file Subtitle.Edit.exe", "Cyan");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi cài đặt Subtitle Edit: {ex.Message}", "Red");
            }
        }

        // ===================================================================
        // Subtitle Edit Download Buttons
        // ===================================================================
        private async void BtnDownloadVietnameseProfile_Click(object sender, RoutedEventArgs e)
        {
            await DownloadSubtitleFileAsync("https://github.com/ghostminhtoan/Subtitle-draft-GMTPC/releases/download/subtitle.materials/vietnamese.profile.profile", "vietnamese.profile.profile");
        }

        private async void BtnDownloadMultipleReplace_Click(object sender, RoutedEventArgs e)
        {
            await DownloadSubtitleFileAsync("https://github.com/ghostminhtoan/Subtitle-draft-GMTPC/releases/download/subtitle.materials/multiple_replace.template", "multiple_replace.template");
        }

        private async void BtnDownloadShortcutMMT_Click(object sender, RoutedEventArgs e)
        {
            await DownloadSubtitleFileAsync("https://github.com/ghostminhtoan/Subtitle-draft-GMTPC/releases/download/subtitle.materials/shortcut.MMT.xml", "shortcut.MMT.xml");
        }

        private async Task DownloadSubtitleFileAsync(string url, string fileName)
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string filePath = Path.Combine(desktopPath, fileName);

                UpdateStatus($"Đang tải {fileName} về Desktop...", "Cyan");

                using (WebClient client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(url, filePath);
                }

                UpdateStatus($"{fileName} đã tải về Desktop!", "Green");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi tải {fileName}: {ex.Message}", "Red");
            }
        }

    }
}
