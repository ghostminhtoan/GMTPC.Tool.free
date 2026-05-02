// AI Summary: 2026-04-28 - Added VentoySourceForgeFiles URL and Ventoy checkbox state wiring
// AI Summary: 2026-04-30 - Added Office Tool Plus Releases URLs for latest x64 runtime zip probing and hover/copy-link flows
// AI Summary: 2026-05-02 - Added MemReduct latest-release constants for the Windows Setup workflow
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
// Chá»©a toÃ n bá»™ code phá»©c táº¡p liÃªn quan Ä‘áº¿n:
//   - InstallXxxAsync() cÃ³ MessageBox, nhiá»u nhÃ¡nh argument, key dialog
//   - BtnXxx_Click dÃ nh riÃªng cho má»™t app cá»¥ thá»ƒ
//   - ShowXxxKeyDialog() vÃ  cÃ¡c helper dialog
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
        private const string IDM_DOWNLOAD_URL = "https://tinyurl.com/idmhcmvn";
        private const string IDM_ACTIVATE_URL = "https://github.com/ghostminhtoan/MMT/releases/download/activate/IDM_6.4x_rabbit.exe";
        private const string IDM_INSTALL_ARGUMENTS = "/s /a /u /o /quiet /skipdlgst";

        // WinRAR
        private const string WINRAR_DOWNLOAD_URL = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/WinRAR.exe";
        private const string WINRAR_INSTALL_ARGUMENTS = "/silent /I /EN";

        // BID (Bulk Image Downloader)
        private const string BID_DOWNLOAD_URL = "https://bulkimagedownloader.com/files/bid_6_62_setup_x64.exe";
        private const string BID_INSTALL_ARGUMENTS = "";

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
        private const string REVO_DOWNLOAD_URL = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/RevoUninstallerPro.exe";
        private const string REVO_INSTALL_ARGUMENTS = "/S";

        // Zalo
        private const string ZALO_DOWNLOAD_URL = "https://zalo.me/download/zalo-pc?utm=90000";
        private const string ZALO_INSTALL_ARGUMENTS = "/s";

        // Theme Toggle
        private const string THEME_REGISTRY_PATH = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string THEME_REGISTRY_VALUE = "AppsUseLightTheme";

        // ===================================================================
        // TabWindows â€” WintoHDD
        // TabItem Header: "Windows"
        // Checkbox: ChkMemReduct, ChkWintoHDD, ChkVentoy
        // ===================================================================
        private const string MEMREDUCT_RELEASES_URL = "https://github.com/henrypp/memreduct/releases";
        private const string MEMREDUCT_RELEASES_API_URL = "https://api.github.com/repos/henrypp/memreduct/releases/latest";
        private const string MEMREDUCT_DOWNLOAD_BASE_URL = "https://github.com/henrypp/memreduct/releases/download";
        private const string MEMREDUCT_INSTALL_ARGUMENTS = "/S";
        private const string WINTOHDD_DOWNLOAD_URL = "https://github.com/ghostminhtoan/MMT/releases/download/WinPE/wintohdd.exe";
        private const string WINTOHDD_INSTALL_ARGUMENTS = "/S";
        private const string VENTOY_RELEASES_URL = "https://github.com/ventoy/Ventoy/releases";

        // ===================================================================
        // TabGaming â€” Jump Force (11 parts)
        // TabItem Header: "Gaming"
        // Checkbox: ChkJumpForce
        // ===================================================================
        private const string JUMP_FORCE_PART01_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/JUMP.FORCE_LinkNeverDie.Com.part01.exe";
        private const string JUMP_FORCE_PART02_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/JUMP.FORCE_LinkNeverDie.Com.part02.rar";
        private const string JUMP_FORCE_PART03_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/JUMP.FORCE_LinkNeverDie.Com.part03.rar";
        private const string JUMP_FORCE_PART04_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/JUMP.FORCE_LinkNeverDie.Com.part04.rar";
        private const string JUMP_FORCE_PART05_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/JUMP.FORCE_LinkNeverDie.Com.part05.rar";
        private const string JUMP_FORCE_PART06_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/JUMP.FORCE_LinkNeverDie.Com.part06.rar";
        private const string JUMP_FORCE_PART07_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/JUMP.FORCE_LinkNeverDie.Com.part07.rar";
        private const string JUMP_FORCE_PART08_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/JUMP.FORCE_LinkNeverDie.Com.part08.rar";
        private const string JUMP_FORCE_PART09_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/JUMP.FORCE_LinkNeverDie.Com.part09.rar";
        private const string JUMP_FORCE_PART10_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/JUMP.FORCE_LinkNeverDie.Com.part10.rar";
        private const string JUMP_FORCE_PART11_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/JUMP.FORCE_LinkNeverDie.Com.part11.rar";
        private const string JUMP_FORCE_FINAL_NAME = "JUMP.FORCE_LinkNeverDie.Com.iso";

        // ===================================================================
        // TabGaming â€” Ghost of Tsushima
        // TabItem Header: "Gaming"
        // Checkbox: ChkGhostOfTsushima
        // ===================================================================
        // Ghost of Tsushima (29 parts)
        private const string GHOST_OF_TSUSHIMA_PART01_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part01.exe";
        private const string GHOST_OF_TSUSHIMA_PART02_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part02.rar";
        private const string GHOST_OF_TSUSHIMA_PART03_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part03.rar";
        private const string GHOST_OF_TSUSHIMA_PART04_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part04.rar";
        private const string GHOST_OF_TSUSHIMA_PART05_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part05.rar";
        private const string GHOST_OF_TSUSHIMA_PART06_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part06.rar";
        private const string GHOST_OF_TSUSHIMA_PART07_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part07.rar";
        private const string GHOST_OF_TSUSHIMA_PART08_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part08.rar";
        private const string GHOST_OF_TSUSHIMA_PART09_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part09.rar";
        private const string GHOST_OF_TSUSHIMA_PART10_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part10.rar";
        private const string GHOST_OF_TSUSHIMA_PART11_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part11.rar";
        private const string GHOST_OF_TSUSHIMA_PART12_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part12.rar";
        private const string GHOST_OF_TSUSHIMA_PART13_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part13.rar";
        private const string GHOST_OF_TSUSHIMA_PART14_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part14.rar";
        private const string GHOST_OF_TSUSHIMA_PART15_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part15.rar";
        private const string GHOST_OF_TSUSHIMA_PART16_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part16.rar";
        private const string GHOST_OF_TSUSHIMA_PART17_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part17.rar";
        private const string GHOST_OF_TSUSHIMA_PART18_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part18.rar";
        private const string GHOST_OF_TSUSHIMA_PART19_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part19.rar";
        private const string GHOST_OF_TSUSHIMA_PART20_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part20.rar";
        private const string GHOST_OF_TSUSHIMA_PART21_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part21.rar";
        private const string GHOST_OF_TSUSHIMA_PART22_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part22.rar";
        private const string GHOST_OF_TSUSHIMA_PART23_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part23.rar";
        private const string GHOST_OF_TSUSHIMA_PART24_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part24.rar";
        private const string GHOST_OF_TSUSHIMA_PART25_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part25.rar";
        private const string GHOST_OF_TSUSHIMA_PART26_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part26.rar";
        private const string GHOST_OF_TSUSHIMA_PART27_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part27.rar";
        private const string GHOST_OF_TSUSHIMA_PART28_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part28.rar";
        private const string GHOST_OF_TSUSHIMA_PART29_URL = "https://github.com/ghostminhtoan/MMT/releases/download/game/Ghost.of.Tsushima_LinkNeverDie.Com.part29.rar";

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
        private const string BOILSOFT_VIDEO_SPLITTER_DOWNLOAD_URL = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/BoilsoftVideoSplitter.8.3.3.by.elchupacabra.exe";
        private const string BOILSOFT_VIDEO_SPLITTER_INSTALL_ARGUMENTS = "/S";

        // Vibe (Tab: Subtitle)
        private const string VIBE_DOWNLOAD_URL = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/vibe.exe";
        private const string VIBE_INSTALL_ARGUMENTS = "/S";

        // MKVToolNix MKVCleaver (Tab: Subtitle)
        private const string MKVTOOLNIX_DOWNLOAD_URL = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/mkvtoolnix-mkvcleaver.exe";
        private const string MKVTOOLNIX_INSTALL_ARGUMENTS = "/S";

        // Subtitle Draft GMTPC (Tab: Subtitle)
        private const string SUBTITLE_DRAFT_GMTPC_DOWNLOAD_URL = "https://raw.githubusercontent.com/ghostminhtoan/Subtitle-draft-GMTPC/refs/heads/master/Subtitle%20draft%20GMTPC.exe";

        // Download sample video (Tab: Subtitle)
        private const string SAMPLE_VIDEO_DOWNLOAD_URL = "https://github.com/ghostminhtoan/Subtitle-draft-GMTPC/releases/download/subtitle.materials/ReZero.Starting.Life.in.Another.World.S04E01.WEB-DL.1080p.AVC.AAC.Dual.Audio-FBI.mkv";

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
        private const string POWERISO_DOWNLOAD_URL = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/PowerISO.exe";
        private const string POWERISO_INSTALL_ARGUMENTS = "/S";

        // TeraCopy
        private const string TERACOPY_DOWNLOAD_URL = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/TeraCopy.Pro.v3.17.0.0.exe";
        private const string TERACOPY_INSTALL_ARGUMENTS = "/S";

        // VPN 1111 (Cloudflare)
        private const string VPN1111_DOWNLOAD_URL = "https://1111-releases.cloudflareclient.com/win/latest";
        private const string VPN1111_INSTALL_ARGUMENTS = "/passive";

        private async Task InstallZaloAsync()
        {
            try
            {
                UpdateStatus("Äang táº£i Zalo...", "Cyan");
                string zaloPath = Path.Combine(GetGMTPCFolder(), "ZaloSetup.exe");
                await DownloadWithProgressAsync(ZALO_DOWNLOAD_URL, zaloPath, "Zalo");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                UpdateStatus("Äang cÃ i Ä‘áº·t Zalo (silent)...", "Yellow");
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
                    UpdateStatus("CÃ i Ä‘áº·t Zalo hoÃ n táº¥t!", "Green");
                }

                if (File.Exists(zaloPath))
                {
                    File.Delete(zaloPath);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lá»—i khi cÃ i Ä‘áº·t Zalo: {ex.Message}", "Red");
            }
        }

        private async Task InstallNeatDMAsync()
        {
            try
            {
                UpdateStatus("Äang táº£i Neat Download Manager...", "Cyan");
                string neatDMPath = Path.Combine(GetGMTPCFolder(), "NeatDM_setup.exe");
                await DownloadWithProgressAsync(NEATDM_DOWNLOAD_URL, neatDMPath, "Neat Download Manager");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                UpdateStatus("Äang cÃ i Ä‘áº·t Neat Download Manager (silent)...", "Yellow");
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
                    UpdateStatus("CÃ i Ä‘áº·t Neat Download Manager hoÃ n táº¥t!", "Green");
                }

                if (File.Exists(neatDMPath))
                {
                    File.Delete(neatDMPath);
                }

                UpdateStatus("Äang má»Ÿ Neat Download Manager...", "Cyan");
                string neatDMExePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Neat Download Manager", "NeatDM.exe");
                if (File.Exists(neatDMExePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = neatDMExePath,
                        UseShellExecute = true
                    });
                }

                UpdateStatus("Äang má»Ÿ trang extension Neat Download Manager...", "Cyan");
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
                UpdateStatus($"Lá»—i khi cÃ i Ä‘áº·t Neat Download Manager: {ex.Message}", "Red");
            }
        }

        private async Task InstallPowerISOAsync()
        {
            try
            {
                UpdateStatus("Äang táº£i PowerISO...", "Cyan");
                string powerISOPath = Path.Combine(GetGMTPCFolder(), "PowerISO.exe");
                await DownloadWithProgressAsync(POWERISO_DOWNLOAD_URL, powerISOPath, "PowerISO");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                UpdateStatus("Äang cÃ i Ä‘áº·t PowerISO (silent)...", "Yellow");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = powerISOPath,
                    Arguments = POWERISO_INSTALL_ARGUMENTS,
                    UseShellExecute = true
                };
                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus("CÃ i Ä‘áº·t PowerISO hoÃ n táº¥t!", "Green");
                }

                if (File.Exists(powerISOPath))
                {
                    File.Delete(powerISOPath);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lá»—i khi cÃ i Ä‘áº·t PowerISO: {ex.Message}", "Red");
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
                UpdateStatus("Äang táº£i Google Drive...", "Cyan");
                string googleDrivePath = Path.Combine(GetGMTPCFolder(), "GoogleDriveSetup.exe");
                await DownloadWithProgressAsync("https://dl.google.com/drive-file-stream/GoogleDriveSetup.exe", googleDrivePath, "Google Drive");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                UpdateStatus("Äang cÃ i Ä‘áº·t Google Drive...", "Yellow");
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
                    UpdateStatus("CÃ i Ä‘áº·t Google Drive hoÃ n táº¥t!", "Green");
                }

                if (File.Exists(googleDrivePath))
                {
                    File.Delete(googleDrivePath);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lá»—i khi cÃ i Ä‘áº·t Google Drive: {ex.Message}", "Red");
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
                UpdateStatus("Äang táº£i FolderSize...", "Cyan");
                string folderSizePath = Path.Combine(GetGMTPCFolder(), "FolderSize-2.6-x64.msi");
                await DownloadWithProgressAsync("https://github.com/ghostminhtoan/MMT/releases/download/v1.0/FolderSize-2.6-x64.msi", folderSizePath, "FolderSize");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                UpdateStatus("Äang cÃ i Ä‘áº·t FolderSize (yÃªu cáº§u quyá»n)...", "Yellow");
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
                    UpdateStatus("CÃ i Ä‘áº·t FolderSize hoÃ n táº¥t!", "Green");
                }

                if (File.Exists(folderSizePath))
                {
                    File.Delete(folderSizePath);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lá»—i khi cÃ i Ä‘áº·t FolderSize: {ex.Message}", "Red");
            }
        }

        // ===================================================================
        // TabSystem â€” Checkbox Click Handlers
        // ===================================================================
        private void ChkMMTApps_Click(object sender, RoutedEventArgs e)
        {
            if (ChkMMTApps.IsChecked == true)
            {
                UpdateStatus("ÄÃ£ chá»n: MMT Apps", "Green");
            }
            else
            {
                UpdateStatus("ÄÃ£ há»§y chá»n: MMT Apps", "Yellow");
            }

            UpdateInstallButtonState();
        }

        private void ChkDISMPP_Click(object sender, RoutedEventArgs e)
        {
            if (ChkDISMPP.IsChecked == true)
            {
                UpdateStatus("ÄÃ£ chá»n: DISM++", "Green");
            }
            else
            {
                UpdateStatus("ÄÃ£ há»§y chá»n: DISM++", "Yellow");
            }

            UpdateInstallButtonState();
        }

        private void ChkComfortClipboardPro_Click(object sender, RoutedEventArgs e)
        {
            if (ChkComfortClipboardPro.IsChecked == true)
            {
                UpdateStatus("ÄÃ£ chá»n: Comfort Clipboard Pro", "Green");
            }
            else
            {
                UpdateStatus("ÄÃ£ há»§y chá»n: Comfort Clipboard Pro", "Yellow");
            }

            UpdateInstallButtonState();
        }

        private void ChkFolderSize_Click(object sender, RoutedEventArgs e)
        {
            if (ChkFolderSize.IsChecked == true)
            {
                UpdateStatus("ÄÃ£ chá»n: FolderSize", "Green");
            }
            else
            {
                UpdateStatus("ÄÃ£ há»§y chá»n: FolderSize", "Yellow");
            }

            UpdateInstallButtonState();
        }

        private void ChkPowerISO_Click(object sender, RoutedEventArgs e)
        {
            if (ChkPowerISO.IsChecked == true)
            {
                UpdateStatus("ÄÃ£ chá»n: PowerISO", "Green");
            }
            else
            {
                UpdateStatus("ÄÃ£ há»§y chá»n: PowerISO", "Yellow");
            }

            UpdateInstallButtonState();
        }

        private void ChkGoogleDrive_Click(object sender, RoutedEventArgs e)
        {
            if (ChkGoogleDrive.IsChecked == true)
            {
                UpdateStatus("ÄÃ£ chá»n: Google Drive", "Green");
            }
            else
            {
                UpdateStatus("ÄÃ£ há»§y chá»n: Google Drive", "Yellow");
            }

            UpdateInstallButtonState();
        }

        private void ChkNetLimiter_Click(object sender, RoutedEventArgs e)
        {
            if (ChkNetLimiter.IsChecked == true)
            {
                UpdateStatus("ÄÃ£ chá»n: NetLimiter", "Green");
            }
            else
            {
                UpdateStatus("ÄÃ£ há»§y chá»n: NetLimiter", "Yellow");
            }

            UpdateInstallButtonState();
        }

        // ===================================================================
        // Common â€” UpdateInstallButtonState
        // ===================================================================
        private void UpdateInstallButtonState()
        {
            // Kiá»ƒm tra xem cÃ³ Ã­t nháº¥t má»™t checkbox nÃ o Ä‘Æ°á»£c chá»n khÃ´ng
            bool hasChecked = ChkInstallIDM.IsChecked == true ||
                             ChkInstallNeatDM.IsChecked == true ||
                             ChkActivateWindows.IsChecked == true ||
                             ChkActivateOffice.IsChecked == true ||
                             ChkOfficeToolPlus.IsChecked == true ||
                             ChkPauseWindowsUpdate.IsChecked == true ||
                             ChkInstallWinRAR.IsChecked == true ||
                             ChkInstallBID.IsChecked == true ||
                             // ThÃªm cÃ¡c checkbox má»›i cho tab Runtime
                             ChkVcredist.IsChecked == true ||
                             ChkDirectX.IsChecked == true ||
                             // ThÃªm checkbox cho Java vÃ  OpenAL
                             ChkJava.IsChecked == true ||
                             ChkOpenAL.IsChecked == true ||
                             // ThÃªm checkbox cho Driver
                             Chk3DPChip.IsChecked == true ||
                             Chk3DPNet.IsChecked == true ||
                             // Google Chrome
                             ChkChrome.IsChecked == true ||
                             // Cá»‘c Cá»‘c
                             ChkCocCoc.IsChecked == true ||
                             // Microsoft Edge
                             ChkEdge.IsChecked == true ||
                             // Brave
                             ChkBrave.IsChecked == true ||
                             ChkRevoUninstaller.IsChecked == true ||
                             // ThÃªm checkbox cho Zalo
                             ChkInstallZalo.IsChecked == true ||
                             // ThÃªm checkbox cho MMT Apps
                             ChkMMTApps.IsChecked == true ||
                             // ThÃªm checkbox cho DISM++
                             ChkDISMPP.IsChecked == true ||
                             // ThÃªm checkbox cho Comfort Clipboard Pro
                             ChkComfortClipboardPro.IsChecked == true ||
                             // ThÃªm checkbox cho Office Softmaker
                             ChkOfficeSoftmaker.IsChecked == true ||
                             // ThÃªm checkbox cho GMTPC Fonts
                             ChkGMTPCFonts.IsChecked == true ||
                             // ThÃªm checkbox cho Notepad++
                             ChkNotepadPlusPlus.IsChecked == true ||
                             // ThÃªm checkbox cho Subtitle Edit
                             ChkSubtitleEdit.IsChecked == true ||
                             // ThÃªm checkbox cho VidCoder (Tab Subtitle)
                             ChkVidCoder.IsChecked == true ||
                             // ThÃªm checkbox cho Boilsoft Video Splitter (Tab Subtitle)
                             ChkBoilsoftVideoSplitter.IsChecked == true ||
                             // ThÃªm checkbox cho Vibe (Tab Subtitle)
                             ChkVibe.IsChecked == true ||
                             // ThÃªm checkbox cho MKVToolNix MKVCleaver (Tab Subtitle)
                             ChkMKVToolNix.IsChecked == true ||
                             // ThÃªm checkbox cho Subtitle Draft GMTPC (Tab Subtitle)
                             ChkSubtitleDraftGMTPC.IsChecked == true ||
                             ChkDownloadSampleVideo.IsChecked == true ||
                             // ThÃªm checkbox cho AOMEI Partition Assistant
                             ChkAomeiPartitionAssistant.IsChecked == true ||
                             // ThÃªm checkbox cho PowerISO
                             ChkPowerISO.IsChecked == true ||
                             // ThÃªm checkbox cho Google Drive
                             ChkGoogleDrive.IsChecked == true ||
                             // ThÃªm checkbox cho NetLimiter
                             ChkNetLimiter.IsChecked == true ||
                             // FolderSize
                             ChkFolderSize.IsChecked == true ||
                             // ThÃªm checkbox cho Gaming tab
                             ChkDiskGenius.IsChecked == true ||
                             ChkProcessLasso.IsChecked == true ||
                             ChkThrottlestop.IsChecked == true ||
                             ChkMSIAfterburner.IsChecked == true ||
                             ChkLeagueOfLegends.IsChecked == true ||
                             ChkPorofessor.IsChecked == true ||
                             ChkSamuraiMaiden.IsChecked == true ||
                             ChkGhostOfTsushima.IsChecked == true ||
                             ChkUltraviewer.IsChecked == true ||
                             // Multimedia
                             ChkPotPlayer.IsChecked == true ||
                             ChkFastStone.IsChecked == true ||
                             ChkFoxit.IsChecked == true ||
                             ChkBandiview.IsChecked == true ||
                             ChkTeamViewerQS.IsChecked == true ||
                             ChkTeamViewerFull.IsChecked == true ||
                             ChkAnyDesk.IsChecked == true ||
                             ChkVMWare162Lite.IsChecked == true ||
                             // Windows - Microsoft
                             ChkMemReduct.IsChecked == true ||
                             ChkWin11_26H1.IsChecked == true ||
                             ChkWin10LtscIot21H2.IsChecked == true ||
                             ChkWin10_22H2_2024_December.IsChecked == true ||
                             // Windows - Ventoy/WintoHDD
                             ChkVentoy.IsChecked == true ||
                             ChkWintoHDD.IsChecked == true ||
                             // Multimedia - Advanced Codec Pack
                             ChkAdvancedCodecPack.IsChecked == true ||
                             // System - TeraCopy and VPN 1111
                             ChkTeraCopy.IsChecked == true ||
                             ChkVPN1111.IsChecked == true ||
                             // Gaming - Jump Force
                             ChkJumpForce.IsChecked == true;

            // Bao gá»“m checkbox cho Táº¡o WinRE vÃ  WinPE

            // Cáº­p nháº­t tráº¡ng thÃ¡i cá»§a nÃºt Install
            BtnInstall.IsEnabled = hasChecked;
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
                UpdateStatus($"Lá»—i khi thÃªm exclusion: {ex.Message}", "Red");
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
                UpdateStatus($"Lá»—i khi xÃ³a exclusion: {ex.Message}", "Red");
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
                UpdateStatus("Äang táº£i DISM++...", "Cyan");
                string dismppPath = Path.Combine(GetGMTPCFolder(), "DISM++.exe");
                await DownloadWithProgressAsync("https://github.com/ghostminhtoan/MMT/releases/download/WinPE/DISM++.exe", dismppPath, "DISM++ Installer");

                MessageBoxResult result = MessageBox.Show("Yes = CÃ i Ä‘áº·t tá»± Ä‘á»™ng vÃ o á»• C\nNo = CÃ i vÃ o á»• khÃ¡c", " CÃ i Ä‘áº·t tá»± Ä‘á»™ng DISM++", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = dismppPath,
                    UseShellExecute = true
                };

                if (result == MessageBoxResult.Yes)
                {
                    startInfo.Arguments = "/s";
                    UpdateStatus("CÃ i Ä‘áº·t DISM++ vÃ o á»• C...", "Yellow");
                }
                else if (result == MessageBoxResult.No)
                {
                    UpdateStatus("CÃ i DISM++ vÃ o á»• khÃ¡c...", "Yellow");
                }
                else
                {
                    UpdateStatus("ÄÃ£ há»§y cÃ i Ä‘áº·t DISM++", "Yellow");
                    if (File.Exists(dismppPath)) File.Delete(dismppPath);
                    return;
                }

                Process process = Process.Start(startInfo);
                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus(process.ExitCode == 0 ? "CÃ i Ä‘áº·t DISM++ thÃ nh cÃ´ng!" : $"CÃ i Ä‘áº·t DISM++ tháº¥t báº¡i. MÃ£ lá»—i: {process.ExitCode}", process.ExitCode == 0 ? "Green" : "Red");
                }

                if (File.Exists(dismppPath)) File.Delete(dismppPath);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lá»—i khi cÃ i Ä‘áº·t DISM++: {ex.Message}", "Red");
            }
        }


        // ===================================================================
        // TabSystem â€” NetLimiter
        // TabItem Header: "System"
        // Checkbox: ChkNetLimiter
        // ===================================================================
        private async Task InstallNetLimiterAsync()
        {
            try
            {
                UpdateStatus("Äang táº£i NetLimiter...", "Cyan");
                string netLimiterPath = Path.Combine(GetGMTPCFolder(), "NetLimiter.exe");
                await DownloadWithProgressAsync("https://github.com/ghostminhtoan/MMT/releases/download/v1.0/netlimiter-4.1.12.0.exe", netLimiterPath, "NetLimiter");

                UpdateStatus("Äang cÃ i Ä‘áº·t NetLimiter...", "Yellow");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = netLimiterPath,
                    Arguments = "/passive",
                    UseShellExecute = true
                };
                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus("CÃ i Ä‘áº·t NetLimiter hoÃ n táº¥t!", "Green");

                    await Dispatcher.InvokeAsync(() => ShowNetLimiterKeyDialog());
                }

                if (File.Exists(netLimiterPath)) File.Delete(netLimiterPath);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lá»—i khi cÃ i Ä‘áº·t NetLimiter: {ex.Message}", "Red");
            }
        }

        private void ShowNetLimiterKeyDialog()
        {
            try
            {
                Window keyDialog = new Window
                {
                    Title = "NetLimiter Key",
                    Width = 500,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    Background = new SolidColorBrush(Color.FromRgb(31, 31, 31)),
                    Foreground = new SolidColorBrush(Color.FromRgb(230, 230, 230))
                };

                StackPanel mainPanel = new StackPanel { Margin = new Thickness(10), Orientation = Orientation.Vertical };

                // Name row
                StackPanel line1 = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
                TextBox nameBox = new TextBox
                {
                    Text = "Vladimir Putin #2", Width = 300, Height = 28, IsReadOnly = true,
                    Background = new SolidColorBrush(Color.FromRgb(43, 43, 43)),
                    Foreground = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(122, 122, 122)),
                    Margin = new Thickness(0, 0, 5, 0)
                };
                Button copyNameBtn = new Button
                {
                    Content = "Copy", Width = 70, Height = 28,
                    Background = new SolidColorBrush(Color.FromRgb(43, 43, 43)),
                    Foreground = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(122, 122, 122))
                };
                copyNameBtn.Click += (s, e) => { Clipboard.SetText(nameBox.Text); UpdateStatus("ÄÃ£ copy: Vladimir Putin #2", "Green"); };
                line1.Children.Add(nameBox);
                line1.Children.Add(copyNameBtn);
                mainPanel.Children.Add(line1);

                // Key row
                StackPanel line2 = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
                TextBox keyBox = new TextBox
                {
                    Text = "XLEVD-PNASB-6A3BD-Z72GJ-SPAH7", Width = 300, Height = 28, IsReadOnly = true,
                    Background = new SolidColorBrush(Color.FromRgb(43, 43, 43)),
                    Foreground = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(122, 122, 122)),
                    Margin = new Thickness(0, 0, 5, 0)
                };
                Button copyKeyBtn = new Button
                {
                    Content = "Copy", Width = 70, Height = 28,
                    Background = new SolidColorBrush(Color.FromRgb(43, 43, 43)),
                    Foreground = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(122, 122, 122))
                };
                copyKeyBtn.Click += (s, e) => { Clipboard.SetText(keyBox.Text); UpdateStatus("ÄÃ£ copy: XLEVD-PNASB-6A3BD-Z72GJ-SPAH7", "Green"); };
                line2.Children.Add(keyBox);
                line2.Children.Add(copyKeyBtn);
                mainPanel.Children.Add(line2);

                keyDialog.Content = mainPanel;
                keyDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lá»—i hiá»ƒn thá»‹ dialog key: {ex.Message}", "Red");
            }
        }

        private void BtnActivateNetLimiter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateStatus("Má»Ÿ cá»­a sá»• kÃ­ch hoáº¡t NetLimiter...", "Cyan");
                ShowNetLimiterKeyDialog();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lá»—i khi má»Ÿ NetLimiter Key Dialog: {ex.Message}", "Red");
            }
        }


        // ===================================================================
        // TabSystem â€” Comfort Clipboard Pro
        // TabItem Header: "System"
        // Checkbox: ChkComfortClipboardPro
        // ===================================================================
        private async Task InstallComfortClipboardProAsync()
        {
            try
            {
                UpdateStatus("Äang táº£i Comfort Clipboard Pro...", "Cyan");
                string comfortClipboardPath = Path.Combine(GetGMTPCFolder(), "Comfort.Clipboard.Pro.7.0.2.exe");
                await DownloadWithProgressAsync("https://github.com/ghostminhtoan/MMT/releases/download/v1.0/Comfort.Clipboard.Pro.7.0.2.exe", comfortClipboardPath, "Comfort Clipboard Pro Installer");

                MessageBoxResult result = MessageBox.Show("Yes = CÃ i Ä‘áº·t tá»± Ä‘á»™ng vÃ o á»• C\nNo = CÃ i vÃ o á»• khÃ¡c", " CÃ i Ä‘áº·t tá»± Ä‘á»™ng Comfort Clipboard Pro", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                ProcessStartInfo startInfo = new ProcessStartInfo { FileName = comfortClipboardPath, UseShellExecute = true };

                if (result == MessageBoxResult.Yes) { startInfo.Arguments = "/passive"; UpdateStatus("CÃ i Ä‘áº·t tá»± Ä‘á»™ng vÃ o á»• C", "Yellow"); }
                else if (result == MessageBoxResult.No) { UpdateStatus("CÃ i vÃ o á»• khÃ¡c...", "Yellow"); }
                else
                {
                    UpdateStatus("ÄÃ£ há»§y cÃ i Ä‘áº·t Comfort Clipboard Pro", "Yellow");
                    if (File.Exists(comfortClipboardPath)) File.Delete(comfortClipboardPath);
                    return;
                }

                Process process = Process.Start(startInfo);
                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus(process.ExitCode == 0 ? "CÃ i Ä‘áº·t Comfort Clipboard Pro thÃ nh cÃ´ng!" : $"Tháº¥t báº¡i. MÃ£ lá»—i: {process.ExitCode}", process.ExitCode == 0 ? "Green" : "Red");
                }

                if (File.Exists(comfortClipboardPath)) File.Delete(comfortClipboardPath);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lá»—i khi cÃ i Ä‘áº·t Comfort Clipboard Pro: {ex.Message}", "Red");
            }
        }


        // ===================================================================
        // TabSystem â€” MMT Apps
        // TabItem Header: "System"
        // Checkbox: ChkMMTApps
        // ===================================================================
        private async Task InstallMMTAppsAsync()
        {
            try
            {
                UpdateStatus("Äang táº£i MMT.Apps.exe...", "Cyan");
                string mmtAppsPath = Path.Combine(GetGMTPCFolder(), "MMT.Apps.exe");
                await DownloadWithProgressAsync("https://github.com/ghostminhtoan/MMT/releases/download/v1.0/MMT.Apps.exe", mmtAppsPath, "MMT Apps Installer");

                MessageBoxResult result = MessageBox.Show("Yes = CÃ i Ä‘áº·t tá»± Ä‘á»™ng vÃ o á»• C\nNo = CÃ i vÃ o á»• khÃ¡c", "CÃ i Ä‘áº·t tá»± Ä‘á»™ng MMT Apps", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                ProcessStartInfo startInfo = new ProcessStartInfo { FileName = mmtAppsPath, UseShellExecute = true };

                if (result == MessageBoxResult.Yes) { startInfo.Arguments = "/passive"; UpdateStatus("CÃ i Ä‘áº·t tá»± Ä‘á»™ng vÃ o á»• C", "Yellow"); }
                else if (result == MessageBoxResult.No) { UpdateStatus("CÃ i vÃ o á»• khÃ¡c...", "Yellow"); }
                else
                {
                    UpdateStatus("ÄÃ£ há»§y cÃ i Ä‘áº·t MMT Apps", "Yellow");
                    if (File.Exists(mmtAppsPath)) File.Delete(mmtAppsPath);
                    return;
                }

                Process process = Process.Start(startInfo);
                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus(process.ExitCode == 0 ? "CÃ i Ä‘áº·t MMT Apps thÃ nh cÃ´ng!" : $"Tháº¥t báº¡i. MÃ£ lá»—i: {process.ExitCode}", process.ExitCode == 0 ? "Green" : "Red");
                }

                if (File.Exists(mmtAppsPath)) File.Delete(mmtAppsPath);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lá»—i khi cÃ i Ä‘áº·t MMT Apps: {ex.Message}", "Red");
            }
        }


        // ===================================================================
        // TabSystem â€” Defender Control
        // TabItem Header: "System"
        // Button: BtnDefenderControl
        // ===================================================================
        private async void BtnDefenderControl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateStatus("Báº¯t Ä‘áº§u quÃ¡ trÃ¬nh Defender Control...", "Cyan");

                AddDefenderExclusionPath(Path.GetTempPath());
                AddDefenderExclusionPath(Path.Combine(Environment.GetEnvironmentVariable("programfiles(x86)"), "DefenderControl"));

                string vbsUrl = "https://raw.githubusercontent.com/ghostminhtoan/MMT/refs/heads/main/windefend%20off.vbs";
                string vbsPath = Path.Combine(Path.GetTempPath(), "windefend_off.vbs");
                using (WebClient client = new WebClient())
                {
                    await Task.Run(() => client.DownloadFile(vbsUrl, vbsPath));
                }

                UpdateStatus("Äang cháº¡y windefend off.vbs...", "Cyan");
                Process vbsProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = "cscript.exe", Arguments = $"\"{vbsPath}\"",
                    UseShellExecute = true, CreateNoWindow = false, Verb = "runas"
                });
                if (vbsProcess != null) await Task.Run(() => vbsProcess.WaitForExit());

                if (File.Exists(vbsPath)) File.Delete(vbsPath);

                MessageBoxResult result = MessageBox.Show(
                    "Náº¿u Ä‘Ã£ táº¯t tamper protection thÃ¬ báº¥m Yes Ä‘á»ƒ táº£i Defender Control vá»",
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

                UpdateStatus("HoÃ n thÃ nh quÃ¡ trÃ¬nh Defender Control!", "Green");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lá»—i: {ex.Message}", "Red");
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

                UpdateStatus("Äang táº£i file backup.restore.mklink.MMT.xlsx...", "Cyan");
                using (WebClient client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(downloadUrl, filePath);
                }

                UpdateStatus("ÄÃ£ táº£i vá» desktop", "Green");
                MessageBox.Show("ÄÃ£ táº£i vá» desktop", "ThÃ´ng bÃ¡o", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lá»—i khi táº£i file: {ex.Message}", "Red");
            }
        }


        // ===================================================================
        // TabPartition â€” Disk Genius
        // TabItem Header: "Partition"
        // Checkbox: ChkDiskGenius
        // ===================================================================
        private async Task InstallDiskGeniusAsync()
        {
            try
            {
                UpdateStatus("Äang táº£i Disk Genius...", "Cyan");
                string diskGeniusPath = Path.Combine(GetGMTPCFolder(), "DiskGenius.exe");
                await DownloadWithProgressAsync("https://github.com/ghostminhtoan/MMT/releases/download/WinPE/Disk.Genius.exe", diskGeniusPath, "Disk Genius");

                MessageBoxResult result = MessageBox.Show("Yes = CÃ i Ä‘áº·t tá»± Ä‘á»™ng vÃ o á»• C\nNo = CÃ i vÃ o á»• khÃ¡c", "CÃ i Ä‘áº·t Disk Genius", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                ProcessStartInfo startInfo = new ProcessStartInfo { FileName = diskGeniusPath, UseShellExecute = true };

                if (result == MessageBoxResult.Yes) { startInfo.Arguments = "/s"; UpdateStatus("CÃ i Ä‘áº·t Disk Genius vÃ o á»• C...", "Yellow"); }
                else if (result == MessageBoxResult.No) { UpdateStatus("CÃ i Disk Genius vÃ o á»• khÃ¡c...", "Yellow"); }
                else
                {
                    UpdateStatus("ÄÃ£ há»§y cÃ i Ä‘áº·t Disk Genius", "Yellow");
                    if (File.Exists(diskGeniusPath)) File.Delete(diskGeniusPath);
                    return;
                }

                Process process = Process.Start(startInfo);
                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus("CÃ i Ä‘áº·t Disk Genius hoÃ n táº¥t!", "Green");
                }

                if (File.Exists(diskGeniusPath)) File.Delete(diskGeniusPath);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lá»—i khi cÃ i Ä‘áº·t Disk Genius: {ex.Message}", "Red");
            }
        }


        // ===================================================================
        // TabPartition â€” AOMEI Partition Assistant
        // TabItem Header: "Partition"
        // Checkbox: ChkAomeiPartitionAssistant
        // ===================================================================
        private async Task InstallAomeiPartitionAssistantAsync()
        {
            try
            {
                UpdateStatus("Äang táº£i AOMEI Partition Assistant...", "Cyan");
                string filePath = Path.Combine(GetGMTPCFolder(), "AOMEI.Partition.Assistant.exe");
                await DownloadWithProgressAsync("https://github.com/ghostminhtoan/MMT/releases/download/WinPE/AOMEI.Partition.Assistant.exe", filePath, "AOMEI Partition Assistant");

                MessageBoxResult result = MessageBox.Show("Yes = CÃ i Ä‘áº·t tá»± Ä‘á»™ng vÃ o á»• C\nNo = CÃ i vÃ o á»• khÃ¡c", "CÃ i Ä‘áº·t AOMEI Partition Assistant", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                {
                    UpdateStatus("ÄÃ£ há»§y cÃ i Ä‘áº·t AOMEI Partition Assistant", "Yellow");
                    if (File.Exists(filePath)) File.Delete(filePath);
                    return;
                }

                ProcessStartInfo startInfo = new ProcessStartInfo { FileName = filePath, UseShellExecute = true };
                if (result == MessageBoxResult.Yes) { startInfo.Arguments = "/passive"; UpdateStatus("CÃ i Ä‘áº·t AOMEI vÃ o á»• C...", "Yellow"); }
                else { UpdateStatus("CÃ i AOMEI vÃ o á»• khÃ¡c...", "Yellow"); }

                Process process = Process.Start(startInfo);
                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus("CÃ i Ä‘áº·t AOMEI Partition Assistant hoÃ n táº¥t!", "Green");
                }

                if (File.Exists(filePath)) File.Delete(filePath);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lá»—i khi cÃ i Ä‘áº·t AOMEI Partition Assistant: {ex.Message}", "Red");
            }
        }

        // ===================================================================
        // TabMultimedia â€” Advanced Codec Pack
        // TabItem Header: "Multimedia"
        // Checkbox: ChkAdvancedCodecPack
        // ===================================================================
        private async Task InstallAdvancedCodecPackAsync()
        {
            try
            {
                UpdateStatus("Äang táº£i Advanced Codec Pack...", "Cyan");
                string codecPath = Path.Combine(GetGMTPCFolder(), "ADVANCED_Codec_Pack.exe");
                await DownloadWithProgressAsync(ADVANCEDCODECPACK_DOWNLOAD_URL, codecPath, "Advanced Codec Pack");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                UpdateStatus("Äang cÃ i Ä‘áº·t Advanced Codec Pack (silent)...", "Yellow");
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
                    UpdateStatus("Advanced Codec Pack Ä‘Ã£ hoÃ n táº¥t.", "Green");
                }

                if (File.Exists(codecPath)) File.Delete(codecPath);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lá»—i khi cÃ i Advanced Codec Pack: {ex.Message}", "Red");
            }
        }

        // ===================================================================
        // TabSystem â€” TeraCopy
        // TabItem Header: "System"
        // Checkbox: ChkTeraCopy
        // ===================================================================
        private async Task InstallTeraCopyAsync()
        {
            try
            {
                // Add Windows Defender exclusion for %TEMP%\TeraCopy before download
                string tempTeraCopyFolder = Path.Combine(Path.GetTempPath(), "TeraCopy");
                UpdateStatus($"Äang thÃªm Windows Defender exclusion: {tempTeraCopyFolder}...", "Yellow");
                AddDefenderExclusion(tempTeraCopyFolder);

                UpdateStatus("Äang táº£i TeraCopy...", "Cyan");
                string teraPath = Path.Combine(GetGMTPCFolder(), "TeraCopy.Pro.v3.17.0.0.exe");
                await DownloadWithProgressAsync(TERACOPY_DOWNLOAD_URL, teraPath, "TeraCopy");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                UpdateStatus("Äang cÃ i Ä‘áº·t TeraCopy (silent)...", "Yellow");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = teraPath,
                    Arguments = TERACOPY_INSTALL_ARGUMENTS,
                    UseShellExecute = true
                };
                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                    UpdateStatus("TeraCopy Ä‘Ã£ hoÃ n táº¥t.", "Green");
                }

                if (File.Exists(teraPath)) File.Delete(teraPath);

                // Remove Windows Defender exclusion after installation
                UpdateStatus("Äang xÃ³a Windows Defender exclusion...", "Yellow");
                RemoveDefenderExclusion(tempTeraCopyFolder);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lá»—i khi cÃ i TeraCopy: {ex.Message}", "Red");
            }
        }

        // ===================================================================
        // TabSystem â€” VPN 1111 (Cloudflare)
        // TabItem Header: "System"
        // Checkbox: ChkVPN1111
        // ===================================================================
        private async Task InstallVPN1111Async()
        {
            try
            {
                UpdateStatus("Äang táº£i VPN 1111...", "Cyan");
                string vpnPath = Path.Combine(GetGMTPCFolder(), "VPN1111.msi");
                await DownloadWithProgressAsync(VPN1111_DOWNLOAD_URL, vpnPath, "VPN 1111");

                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });

                UpdateStatus("Äang cÃ i Ä‘áº·t VPN 1111 (passive)...", "Yellow");
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
                    UpdateStatus("VPN 1111 Ä‘Ã£ hoÃ n táº¥t.", "Green");
                }

                if (File.Exists(vpnPath)) File.Delete(vpnPath);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lá»—i khi cÃ i VPN 1111: {ex.Message}", "Red");
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
                            UpdateStatus($"ÄÃ£ thÃªm exclusion: {path}", "Green");
                        }
                        else
                        {
                            UpdateStatus($"KhÃ´ng thá»ƒ thÃªm exclusion (cáº§n quyá»n Admin). Tiáº¿p tá»¥c cÃ i Ä‘áº·t...", "Yellow");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lá»—i khi thÃªm Defender exclusion: {ex.Message}. Tiáº¿p tá»¥c...", "Yellow");
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
                            UpdateStatus($"ÄÃ£ xÃ³a exclusion: {path}", "Green");
                        }
                        else
                        {
                            UpdateStatus($"KhÃ´ng thá»ƒ xÃ³a exclusion (cáº§n quyá»n Admin).", "Yellow");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lá»—i khi xÃ³a Defender exclusion: {ex.Message}", "Yellow");
            }
        }

    }
}

