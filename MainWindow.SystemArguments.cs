// AI Summary: 2026-05-29 - Removed constants, click handlers, install methods, and UpdateInstallButtonState conditions for 25 target checkboxes.
﻿// AI Summary: 2026-04-28 - Added VentoySourceForgeFiles URL and Ventoy checkbox state wiring
// AI Summary: 2026-04-30 - Added Office Tool Plus Releases URLs for latest x64 runtime zip probing and hover/copy-link flows
// AI Summary: 2026-05-12 - Moved TeraCopy Defender exclusions before download, removed temp exclusion after install, and kept %ProgramFiles%\TeraCopy permanent
// AI Summary: 2026-05-02 - Updated Windows Setup constants for Ventoy and WintoHDD
// AI Summary: 2026-05-12 - Added ChkDeactivateWindows wiring for the built-in slmgr /dlv -> slmgr /upk x flow
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GMTPC.Tool
{
    /*
 * AI Summary:
 * Date: 2026-04-30
 * - Added Office Tool Plus Releases URLs for latest x64 runtime zip probing and hover/copy-link flows
 * Date: 2026-04-28
 * - Updated VENTOY_RELEASES_URL for the Windows Tools Ventoy GitHub Releases flow
 * Date: 2026-04-25 (2)
 * - Added Brave browser download URL, install arguments, and Browser-tab install wiring
 * Date: 2026-04-25
 * - Updated SUBTITLE_EDIT_DOWNLOAD_URL to the new Subtitle Edit GMTPC portable release link
 * Date: 2026-04-24
 * - Added SAMPLE_VIDEO_DOWNLOAD_URL and ChkDownloadSampleVideo to UpdateInstallButtonState()
 * Date: 2026-03-29 (6)
 * - Added ChkBoilsoftVideoSplitter, ChkVibe, ChkMKVToolNix to UpdateInstallButtonState()
 * Date: 2026-03-29 (4)
 * - Added BOILSOFT_VIDEO_SPLITTER_DOWNLOAD_URL, VIBE_DOWNLOAD_URL, MKVTOOLNIX_DOWNLOAD_URL and arguments
 * Date: 2026-03-29 (2)
 * - Added ChkVidCoder to UpdateInstallButtonState()
 * Date: 2026-03-28
 * - Added SUBTITLE_EDIT_DOWNLOAD_URL and SUBTITLE_EDIT_INSTALL_ARGUMENTS for Subtitle Edit
 * Date: 2026-03-26 (2)
 * - Added ChkWintoHDD and ChkJumpForce to UpdateInstallButtonState()
 * Date: 2026-03-26
 * - Added THEME_REGISTRY_PATH and THEME_REGISTRY_VALUE constants for Theme Toggle
 * - Added WINTOHDD_DOWNLOAD_URL, WINTOHDD_INSTALL_ARGUMENTS
 * - Added JUMP_FORCE_PART01_URL to PART11_URL constants for Jump Force (11 parts)
 * Date: 2026-03-11
 * - Added GHOST_OF_TSUSHIMA_PART01_URL to PART29_URL constants for Ghost of Tsushima
 * Date: 2026-03-09
 * - Added ChkAdvancedCodecPack, ChkTeraCopy, ChkVPN1111 constants and methods
 */
// =======================================================================
// MainWindow.SystemArguments.cs
// Chá»©a toàn bộ code phức tạp liên quan đến:
//   - InstallXxxAsync() cài MessageBox, nhiều nhánh argument, key dialog
//   - BtnXxx_Click dàmh riêng cho mỗi app có thể
//   - ShowXxxKeyDialog() và các helper dialog
//   - Logic activate / crack
// =======================================================================
    public partial class MainWindow
    {
        // ===================================================================
        // TabPopular â€” Links (B) and Arguments (C)
        // TabItem Header: "Popular"
        // Checkboxes: ChkInstallIDM, ChkInstallWinRAR, ChkInstallBID,
        //             ChkActivateWindows, ChkPauseWindowsUpdate, ChkVcredist,
        //             ChkDirectX, ChkJava, ChkOpenAL, ChkRevoUninstaller
        // ===================================================================
        // IDM

        // WinRAR
        private const string WINRAR_DOWNLOAD_URL = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/WinRAR.exe";
        private const string WINRAR_INSTALL_ARGUMENTS = "/silent /I /EN";

        // BID (Bulk Image Downloader)

        // Neat Download Manager
        private const string NEATDM_DOWNLOAD_URL = "https://neatdownloadmanager.com/file/NeatDM_setup.exe";
        private const string NEATDM_INSTALL_ARGUMENTS = "/silent";

        // Activate Windows
        // Vcredist
        private const string VCREDIST_DOWNLOAD_URL = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/vcredist.all.in.one.by.MMT.Windows.Tech.exe";
        private const string VCREDIST_INSTALL_ARGUMENTS = "/passive";

        // DirectX
        private const string DIRECTX_DOWNLOAD_URL = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/DirectX.exe";
        private const string DIRECTX_INSTALL_ARGUMENTS = "/passive";

        // Java
        private const string JAVA_DOWNLOAD_URL = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/java.exe";
        private const string JAVA_INSTALL_ARGUMENTS = "/s";

        // OpenAL
        private const string OPENAL_DOWNLOAD_URL = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/oalinst.exe";
        private const string OPENAL_INSTALL_ARGUMENTS = "/S";

        // Revo Uninstaller

        // Zalo
        private const string ZALO_DOWNLOAD_URL = "https://zalo.me/download/zalo-pc?utm=90000";
        private const string ZALO_INSTALL_ARGUMENTS = "/s";

        // Theme Toggle
        private const string THEME_REGISTRY_PATH = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string THEME_REGISTRY_VALUE = "AppsUseLightTheme";

        // ===================================================================
        // TabWindows â€” WintoHDD
        // TabItem Header: "Windows"
        // Checkbox: ChkWintoHDD, ChkVentoy
        // ===================================================================
        private const string VENTOY_RELEASES_URL = "https://github.com/ventoy/Ventoy/releases";

        // ===================================================================
        // TabGaming â€” Jump Force (11 parts)
        // TabItem Header: "Gaming"
        // Checkbox: ChkJumpForce
        // ===================================================================

        // ===================================================================
        // TabGaming â€” Ghost of Tsushima
        // TabItem Header: "Gaming"
        // Checkbox: ChkGhostOfTsushima
        // ===================================================================
        // Ghost of Tsushima (29 parts)

        // ===================================================================
        // TabOffice â€” Links (B) and Arguments (C)
        // TabItem Header: "Office"
        // Checkboxes: ChkActivateOffice, ChkOfficeToolPlus, ChkOfficeSoftmaker,
        //             ChkGMTPCFonts, ChkNotepadPlusPlus
        // ===================================================================
        // Office Tool Plus
        private const string OFFICE_TOOL_PLUS_RELEASES_URL = "https://github.com/YerongAI/Office-Tool/releases";
        private const string OFFICE_TOOL_PLUS_RELEASES_API_URL = "https://api.github.com/repos/YerongAI/Office-Tool/releases/latest";

        // GMTPC Fonts (Tab: Office)
        private const string GMTPC_FONTS_DOWNLOAD_URL = "https://github.com/ghostminhtoan/Subtitle-draft-GMTPC/releases/download/subtitle.materials/GMTPC-FONTS.exe";

        // Notepad++ (Tab: Office)
        private const string NOTEPAD_PLUS_PLUS_DOWNLOAD_URL = "https://github.com/notepad-plus-plus/notepad-plus-plus/releases/download/v8.9.2/npp.8.9.2.Installer.x64.msi";
        private const string NOTEPAD_PLUS_PLUS_INSTALL_ARGUMENTS = "/passive /norestart";

        // Subtitle Edit (Tab: Office)
        private const string SUBTITLE_EDIT_DOWNLOAD_URL = "https://github.com/ghostminhtoan/Subtitle-draft-GMTPC/releases/download/subtitle.materials/Subtitle.Edit.-.GMTPC.portable.exe";
        private const string SUBTITLE_EDIT_INSTALL_ARGUMENTS = "/passive";

        // ===================================================================
        // TabSubtitle â€” Links (B) and Arguments (C)
        // TabItem Header: "Subtitle"
        // Checkboxes: ChkBoilsoftVideoSplitter, ChkVibe, ChkMKVToolNix, ChkSubtitleDraftGMTPC, ChkDownloadSampleVideo
        // ===================================================================
        // Boilsoft Video Splitter (Tab: Subtitle)

        // Vibe (Tab: Subtitle)
        private const string VIBE_DOWNLOAD_URL = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/vibe.exe";
        private const string VIBE_INSTALL_ARGUMENTS = "/S";

        // MKVToolNix MKVCleaver (Tab: Subtitle)
        private const string MKVTOOLNIX_DOWNLOAD_URL = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/mkvtoolnix-mkvcleaver.exe";
        private const string MKVTOOLNIX_INSTALL_ARGUMENTS = "/S";

        // Subtitle Draft GMTPC (Tab: Subtitle)
        private const string SUBTITLE_DRAFT_GMTPC_DOWNLOAD_URL = "https://raw.githubusercontent.com/ghostminhtoan/Subtitle-draft-GMTPC/refs/heads/master/Subtitle%20draft%20GMTPC.exe";

        // Download sample video (Tab: Subtitle)

        // ===================================================================
        // TabDriver â€” Links (B) and Arguments (C)
        // TabItem Header: "Driver"
        // Checkboxes: Chk3DPChip, Chk3DPNet
        // ===================================================================
        // 3DP Chip (Tab: Driver)
        private const string DPCHIP_DOWNLOAD_URL = "https://www.3dpchip.com/3dp/chip.exe";
        private const string DPCHIP_INSTALL_ARGUMENTS = "/S";

        // 3DP Net (Tab: Driver)
        private const string DPNET_DOWNLOAD_URL = "https://www.3dpchip.com/3dp/net.exe";
        private const string DPNET_INSTALL_ARGUMENTS = "/S";

        // ===================================================================
        // TabBrowser â€” Links (B) and Arguments (C)
        // TabItem Header: "Browser"
        // Checkboxes: ChkChrome, ChkCocCoc, ChkEdge, ChkBrave
        // ===================================================================
        // Chrome (Tab: Browser)
        private const string CHROME_DOWNLOAD_URL = "https://dl.google.com/chrome/install/latest/chrome_installer.exe";
        private const string CHROME_INSTALL_ARGUMENTS = "/silent /install";

        // CocCoc (Tab: Browser)
        private const string COCCOC_DOWNLOAD_URL = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/CocCoc.exe";
        private const string COCCOC_INSTALL_ARGUMENTS = "/silent /install";

        // Edge (Tab: Browser)
        private const string EDGE_DOWNLOAD_URL = "https://go.microsoft.com/fwlink/?linkid=2108834&Channel=Stable&language=vi";
        private const string EDGE_INSTALL_ARGUMENTS = "/silent /install";

        // Brave (Tab: Browser)
        private const string BRAVE_DOWNLOAD_URL = "https://laptop-updates.brave.com/download/BRV010?bitness=64";
        private const string BRAVE_INSTALL_ARGUMENTS = "";

        // ===================================================================
        // TabMultimedia â€” Advanced Codec Pack
        // TabItem Header: "Multimedia"
        // Checkboxes: ChkAdvancedCodecPack
        // ===================================================================
        private const string ADVANCEDCODECPACK_DOWNLOAD_URL = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/ADVANCED_Codec_Pack.exe";
        private const string ADVANCEDCODECPACK_INSTALL_ARGUMENTS = "/S /V/qn";

        // ===================================================================
        // TabSystem â€” PowerISO
        // TabItem Header: "System"
        // Checkboxes: ChkPowerISO, ChkTeraCopy, ChkVPN1111
        // ===================================================================

        // MemReduct (GitHub Releases direct download)
        private const string MEMREDUCT_DOWNLOAD_URL = "https://github.com/henrypp/memreduct/releases/download/v.3.5.2/memreduct-3.5.2-setup.exe";

        // TeraCopy

        // VPN 1111 (Cloudflare)
        private const string VPN1111_DOWNLOAD_URL = "https://1111-releases.cloudflareclient.com/win/latest";
        private const string VPN1111_INSTALL_ARGUMENTS = "/passive";

        private async Task InstallZaloAsync()
        {
            try
            {
                UpdateStatus("Đang tải Zalo...", "Cyan");
                string zaloPath = Path.Combine(GetGMTPCFolder(), "ZaloSetup.exe");
                await DownloadWithProgressAsync(ZALO_DOWNLOAD_URL, zaloPath, "Zalo");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                UpdateStatus("Đang cài đặt Zalo (silent)...", "Yellow");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = zaloPath,
                    Arguments = ZALO_INSTALL_ARGUMENTS,
                    UseShellExecute = true
                };
                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus("cài đặt Zalo hoàn tất!", "Green");
                }

                if (File.Exists(zaloPath))
                {
                    File.Delete(zaloPath);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi cài đặt Zalo: {ex.Message}", "Red");
            }
        }

        private async Task InstallNeatDMAsync()
        {
            try
            {
                UpdateStatus("Đang tải Neat Download Manager...", "Cyan");
                string neatDMPath = Path.Combine(GetGMTPCFolder(), "NeatDM_setup.exe");
                await DownloadWithProgressAsync(NEATDM_DOWNLOAD_URL, neatDMPath, "Neat Download Manager");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                UpdateStatus("Đang cài đặt Neat Download Manager (silent)...", "Yellow");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = neatDMPath,
                    Arguments = NEATDM_INSTALL_ARGUMENTS,
                    UseShellExecute = true
                };
                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus("cài đặt Neat Download Manager hoàn tất!", "Green");
                }

                if (File.Exists(neatDMPath))
                {
                    File.Delete(neatDMPath);
                }

                UpdateStatus("Đang mở Neat Download Manager...", "Cyan");
                string neatDMExePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Neat Download Manager", "NeatDM.exe");
                if (File.Exists(neatDMExePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = neatDMExePath,
                        UseShellExecute = true
                    });
                }

                UpdateStatus("Đang mở trang extension Neat Download Manager...", "Cyan");
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c start https://chromewebstore.google.com/detail/neatdownloadmanager-exten/cpcifbdmkopohnnofedkjghjiclmhdah",
                    UseShellExecute = true,
                    CreateNoWindow = true
                });
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi cài đặt Neat Download Manager: {ex.Message}", "Red");
            }
        }


        // ===================================================================
        // TabSystem â€” Google Drive
        // TabItem Header: "System"
        // Checkbox: ChkGoogleDrive
        // ===================================================================
        private async Task InstallGoogleDriveAsync()
        {
            try
            {
                UpdateStatus("Đang tải Google Drive...", "Cyan");
                string googleDrivePath = Path.Combine(GetGMTPCFolder(), "GoogleDriveSetup.exe");
                await DownloadWithProgressAsync("https://dl.google.com/drive-file-stream/GoogleDriveSetup.exe", googleDrivePath, "Google Drive");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                UpdateStatus("Đang cài đặt Google Drive...", "Yellow");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = googleDrivePath,
                    Arguments = "--silent",
                    UseShellExecute = true
                };
                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus("cài đặt Google Drive hoàn tất!", "Green");
                }

                if (File.Exists(googleDrivePath))
                {
                    File.Delete(googleDrivePath);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi cài đặt Google Drive: {ex.Message}", "Red");
            }
        }

        private async Task InstallMemReductAsync()
        {
            try
            {
                UpdateStatus("Đang tải MemReduct...", "Cyan");
                string memReductPath = Path.Combine(GetGMTPCFolder(), "memreduct-3.5.2-setup.exe");
                await DownloadWithProgressAsync(MEMREDUCT_DOWNLOAD_URL, memReductPath, "MemReduct");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                UpdateStatus("Đang cài đặt MemReduct (silent)...", "Yellow");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = memReductPath,
                    Arguments = "/S",
                    UseShellExecute = true
                };
                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus("MemReduct đã hoàn tất.", "Green");
                }

                if (File.Exists(memReductPath)) File.Delete(memReductPath);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi cài MemReduct: {ex.Message}", "Red");
            }
        }

        // ===================================================================
        // TabSystem â€” FolderSize
        // TabItem Header: "System"
        // Checkbox: ChkFolderSize
        // ===================================================================
        private async Task InstallFolderSizeAsync()
        {
            try
            {
                UpdateStatus("Đang tải FolderSize...", "Cyan");
                string folderSizePath = Path.Combine(GetGMTPCFolder(), "FolderSize-2.6-x64.msi");
                await DownloadWithProgressAsync("https://github.com/ghostminhtoan/MMT/releases/download/v1.0/FolderSize-2.6-x64.msi", folderSizePath, "FolderSize");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                UpdateStatus("Đang cài đặt FolderSize (yêu cầu quyền)...", "Yellow");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "msiexec",
                    Arguments = $"/i \"{folderSizePath}\" /passive",
                    UseShellExecute = true
                };

                Process process = Process.Start(startInfo);
                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus("cài đặt FolderSize hoàn tất!", "Green");
                }

                if (File.Exists(folderSizePath))
                {
                    File.Delete(folderSizePath);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi cài đặt FolderSize: {ex.Message}", "Red");
            }
        }

        // ===================================================================
        // TabSystem â€” Checkbox Click Handlers
        // ===================================================================

        private void ChkDISMPP_Click(object sender, RoutedEventArgs e)
        {
            if (ChkDISMPP.IsChecked == true)
            {
                UpdateStatus("Đã chọn: DISM++", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: DISM++", "Yellow");
            }

            UpdateInstallButtonState();
        }


        private void ChkFolderSize_Click(object sender, RoutedEventArgs e)
        {
            if (ChkFolderSize.IsChecked == true)
            {
                UpdateStatus("Đã chọn: FolderSize", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: FolderSize", "Yellow");
            }

            UpdateInstallButtonState();
        }


        private void ChkGoogleDrive_Click(object sender, RoutedEventArgs e)
        {
            if (ChkGoogleDrive.IsChecked == true)
            {
                UpdateStatus("Đã chọn: Google Drive", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: Google Drive", "Yellow");
            }

            UpdateInstallButtonState();
        }


        // ===================================================================
        // Common â€” UpdateInstallButtonState
        // ===================================================================
        private void UpdateInstallButtonState()
        
        {
            // Kiểm tra xem có ít nhất một checkbox nào được chọn Không
            bool hasChecked = ChkInstallNeatDM.IsChecked == true ||
                             ChkDeactivateWindows.IsChecked == true ||
                             ChkOfficeToolPlus.IsChecked == true ||
                             ChkPauseWindowsUpdate.IsChecked == true ||
                             ChkInstallWinRAR.IsChecked == true ||
                             ChkVcredist.IsChecked == true ||
                             ChkDirectX.IsChecked == true ||
                             ChkJava.IsChecked == true ||
                             ChkOpenAL.IsChecked == true ||
                             Chk3DPChip.IsChecked == true ||
                             Chk3DPNet.IsChecked == true ||
                             ChkChrome.IsChecked == true ||
                             ChkCocCoc.IsChecked == true ||
                             ChkEdge.IsChecked == true ||
                             ChkBrave.IsChecked == true ||
                             ChkInstallZalo.IsChecked == true ||
                             ChkDISMPP.IsChecked == true ||
                             ChkGMTPCFonts.IsChecked == true ||
                             ChkNotepadPlusPlus.IsChecked == true ||
                             ChkSubtitleEdit.IsChecked == true ||
                             ChkVidCoder.IsChecked == true ||
                             ChkVibe.IsChecked == true ||
                             ChkMKVToolNix.IsChecked == true ||
                             ChkSubtitleDraftGMTPC.IsChecked == true ||
                             ChkGoogleDrive.IsChecked == true ||
                             ChkFolderSize.IsChecked == true ||
                             ChkThrottlestop.IsChecked == true ||
                             ChkMSIAfterburner.IsChecked == true ||
                             ChkLeagueOfLegends.IsChecked == true ||
                             ChkPorofessor.IsChecked == true ||
                             ChkUltraviewer.IsChecked == true ||
                             ChkPotPlayer.IsChecked == true ||
                             ChkFoxit.IsChecked == true ||
                             ChkTeamViewerQS.IsChecked == true ||
                             ChkTeamViewerFull.IsChecked == true ||
                             ChkAnyDesk.IsChecked == true ||
                             ChkWin11_26H1.IsChecked == true ||
                             ChkVentoy.IsChecked == true ||
                             ChkAdvancedCodecPack.IsChecked == true ||
                             ChkVPN1111.IsChecked == true;

            BtnInstall.IsEnabled = hasChecked;

            // Cập nhật trạng thái màu sắc dựa trên việc được chọn hay không
            if (hasChecked)
            {
                // Thay đổi màu border hoặc text của nút install để gây chú ý
            }
        }

        // ===================================================================
        // Common â€” Add/Remove Defender Exclusion Path
        // ===================================================================
        private void AddDefenderExclusionPath(string exclusionPath)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"Add-MpPreference -ExclusionPath '{exclusionPath}' -Force\"",
                    UseShellExecute = true,
                    Verb = "runas",
                    CreateNoWindow = true
                };
                Process process = Process.Start(startInfo);
                if (process != null)
                {
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi thêm exclusion: {ex.Message}", "Red");
            }
        }

        private void RemoveDefenderExclusionPath(string exclusionPath)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"Remove-MpPreference -ExclusionPath '{exclusionPath}' -Force\"",
                    UseShellExecute = true,
                    Verb = "runas",
                    CreateNoWindow = true
                };
                Process process = Process.Start(startInfo);
                if (process != null)
                {
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi xóa exclusion: {ex.Message}", "Red");
            }
        }

        // ===================================================================
        // TabSystem â€” DISM++
        // TabItem Header: "System"
        // Checkbox: ChkDISMPP
        // ===================================================================
        private async Task InstallDISMPPAsync()
        {
            try
            {
                UpdateStatus("Đang tải DISM++...", "Cyan");
                string dismppPath = Path.Combine(GetGMTPCFolder(), "DISM++.exe");
                await DownloadWithProgressAsync("https://github.com/ghostminhtoan/MMT/releases/download/WinPE/DISM++.exe", dismppPath, "DISM++ Installer");

                MessageBoxResult result = MessageBox.Show("Yes = cài đặt tự động vào ổ C\nNo = cài vào ổ khác", " cài đặt tự động DISM++", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = dismppPath,
                    UseShellExecute = true
                };

                if (result == MessageBoxResult.Yes)
                {
                    startInfo.Arguments = "/s";
                    UpdateStatus("cài đặt DISM++ vào ổ C...", "Yellow");
                }
                else if (result == MessageBoxResult.No)
                {
                    UpdateStatus("cài DISM++ vào ổ khác...", "Yellow");
                }
                else
                {
                    UpdateStatus("Đã hủy cài đặt DISM++", "Yellow");
                    if (File.Exists(dismppPath)) File.Delete(dismppPath);
                    return;
                }

                Process process = Process.Start(startInfo);
                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus(process.ExitCode == 0 ? "cài đặt DISM++ thành cÃ´ng!" : $"cài đặt DISM++ tháº¥t báº¡i. MÃ£ Lỗi: {process.ExitCode}", process.ExitCode == 0 ? "Green" : "Red");
                }

                if (File.Exists(dismppPath)) File.Delete(dismppPath);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi cài đặt DISM++: {ex.Message}", "Red");
            }
        }


        // ===================================================================
        // TabSystem â€” NetLimiter
        // TabItem Header: "System"
        // Checkbox: ChkNetLimiter
        // ===================================================================




        // ===================================================================
        // TabSystem â€” Comfort Clipboard Pro
        // TabItem Header: "System"
        // Checkbox: ChkComfortClipboardPro
        // ===================================================================


        // ===================================================================
        // TabSystem â€” MMT Apps
        // TabItem Header: "System"
        // Checkbox: ChkMMTApps
        // ===================================================================


        // ===================================================================
        // TabSystem â€” Defender Control
        // TabItem Header: "System"
        // Button: BtnDefenderControl
        // ===================================================================
        private async void BtnDefenderControl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateStatus("Bật tắt quá trình Defender Control...", "Cyan");

                AddDefenderExclusionPath(Path.GetTempPath());
                AddDefenderExclusionPath(Path.Combine(Environment.GetEnvironmentVariable("programfiles(x86)"), "DefenderControl"));

                string vbsUrl = "https://raw.githubusercontent.com/ghostminhtoan/MMT/refs/heads/main/windefend%20off.vbs";
                string vbsPath = Path.Combine(Path.GetTempPath(), "windefend_off.vbs");
                using (WebClient client = new WebClient())
                {
                    await Task.Run(() => client.DownloadFile(vbsUrl, vbsPath));
                }

                UpdateStatus("Đang cháº¡y windefend off.vbs...", "Cyan");
                Process vbsProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = "cscript.exe", Arguments = $"\"{vbsPath}\"",
                    UseShellExecute = true, CreateNoWindow = false, Verb = "runas"
                });
                if (vbsProcess != null) await Task.Run(() => vbsProcess.WaitForExit());

                if (File.Exists(vbsPath)) File.Delete(vbsPath);

                MessageBoxResult result = MessageBox.Show(
                    "Nếu đã tắt tamper protection thì bấm Yes để tải Defender Control vào",
                    "Defender Control", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    string exeUrl = "https://raw.githubusercontent.com/ghostminhtoan/MMT/main/Defender%20Control.exe";
                    string exePath = Path.Combine(Path.GetTempPath(), "Defender Control.exe");
                    using (WebClient client = new WebClient())
                    {
                        await Task.Run(() => client.DownloadFile(exeUrl, exePath));
                    }

                    Process exeProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = exePath, Arguments = "/s -p1111",
                        UseShellExecute = true, CreateNoWindow = false
                    });
                    if (exeProcess != null) await Task.Run(() => exeProcess.WaitForExit());

                    if (File.Exists(exePath)) File.Delete(exePath);
                    RemoveDefenderExclusionPath(Path.GetTempPath());
                }

                UpdateStatus("hoàn thành quá trình Defender Control!", "Green");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi: {ex.Message}", "Red");
            }
        }


        // ===================================================================
        // TabSystem â€” Backup Restore Mklink MMT
        // TabItem Header: "System"
        // Button: BtnBackupRestoreMklinkMMT
        // ===================================================================
        private async void BtnBackupRestoreMklinkMMT_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = Path.Combine(desktopPath, "backup.restore.mklink.MMT.xlsx");
                string downloadUrl = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/backup.restore.mklink.MMT.xlsx";

                UpdateStatus("Đang tải file backup.restore.mklink.MMT.xlsx...", "Cyan");
                using (WebClient client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(downloadUrl, filePath);
                }

                UpdateStatus("Đã tải vào desktop", "Green");
                MessageBox.Show("Đã tải vào desktop", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi tải file: {ex.Message}", "Red");
            }
        }


        // ===================================================================
        // TabPartition â€” Disk Genius
        // TabItem Header: "Partition"
        // Checkbox: ChkDiskGenius
        // ===================================================================


        // ===================================================================
        // TabPartition â€” AOMEI Partition Assistant
        // TabItem Header: "Partition"
        // Checkbox: ChkAomeiPartitionAssistant
        // ===================================================================

        // ===================================================================
        // TabMultimedia â€” Advanced Codec Pack
        // TabItem Header: "Multimedia"
        // Checkbox: ChkAdvancedCodecPack
        // ===================================================================
        private async Task InstallAdvancedCodecPackAsync()
        {
            try
            {
                UpdateStatus("Đang tải Advanced Codec Pack...", "Cyan");
                string codecPath = Path.Combine(GetGMTPCFolder(), "ADVANCED_Codec_Pack.exe");
                await DownloadWithProgressAsync(ADVANCEDCODECPACK_DOWNLOAD_URL, codecPath, "Advanced Codec Pack");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                UpdateStatus("Đang cài đặt Advanced Codec Pack (silent)...", "Yellow");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = codecPath,
                    Arguments = ADVANCEDCODECPACK_INSTALL_ARGUMENTS,
                    UseShellExecute = true
                };
                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus("Advanced Codec Pack đã hoàn tất.", "Green");
                }

                if (File.Exists(codecPath)) File.Delete(codecPath);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi cài Advanced Codec Pack: {ex.Message}", "Red");
            }
        }

        // ===================================================================
        // TabSystem â€” TeraCopy
        // TabItem Header: "System"
        // Checkbox: ChkTeraCopy
        // ===================================================================

        // ===================================================================
        // TabSystem â€” VPN 1111 (Cloudflare)
        // TabItem Header: "System"
        // Checkbox: ChkVPN1111
        // ===================================================================
        private async Task InstallVPN1111Async()
        {
            try
            {
                UpdateStatus("Đang tải VPN 1111...", "Cyan");
                string vpnPath = Path.Combine(GetGMTPCFolder(), "VPN1111.msi");
                await DownloadWithProgressAsync(VPN1111_DOWNLOAD_URL, vpnPath, "VPN 1111");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                UpdateStatus("Đang cài đặt VPN 1111 (passive)...", "Yellow");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "msiexec.exe",
                    Arguments = $"/i \"{vpnPath}\" /passive",
                    UseShellExecute = true
                };
                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus("VPN 1111 đã hoàn tất.", "Green");
                }

                if (File.Exists(vpnPath)) File.Delete(vpnPath);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi cài VPN 1111: {ex.Message}", "Red");
            }
        }

        // ===================================================================
        // Windows Defender Exclusion Methods
        // ===================================================================
        /// <summary>
        /// Add a path to Windows Defender exclusion list using PowerShell
        /// </summary>
        private void AddDefenderExclusion(string path)
        {
            try
            {
                // First, ensure the folder exists
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                // Use PowerShell to add exclusion with administrator privileges
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"Add-MpPreference -ExclusionPath '{path}' -Force\"",
                    UseShellExecute = true,
                    Verb = "runas", // Run as administrator
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (Process process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                        if (process.ExitCode == 0)
                        {
                            UpdateStatus($"Đã thêm exclusion: {path}", "Green");
                        }
                        else
                        {
                            UpdateStatus($"Không thể thêm exclusion (cần quyền Admin). Tiếp tục cài đặt...", "Yellow");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi thêm Defender exclusion: {ex.Message}. Tiếp tục...", "Yellow");
            }
        }

        /// <summary>
        /// Remove a path from Windows Defender exclusion list using PowerShell
        /// </summary>
        private void RemoveDefenderExclusion(string path)
        {
            try
            {
                // Use PowerShell to remove exclusion with administrator privileges
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"Remove-MpPreference -ExclusionPath '{path}'\"",
                    UseShellExecute = true,
                    Verb = "runas", // Run as administrator
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (Process process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                        if (process.ExitCode == 0)
                        {
                            UpdateStatus($"Đã xóa exclusion: {path}", "Green");
                        }
                        else
                        {
                            UpdateStatus($"Không thể xóa exclusion (cần quyền Admin).", "Yellow");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi xóa Defender exclusion: {ex.Message}", "Yellow");
            }
        }

    }
}

