// AI Summary: 2026-05-29 - Removed 25 target checkboxes from Select All, Select None, MouseEnter, and Install tasks lists.
﻿// =======================================================================
// MainWindow.SystemButtons.cs
// Chức năng: Logic các nút điều khiển chung: Select All, Select None,
//            SelectNoneAllTabs, Install, Pause, Resume, Refresh Color,
//            BtnDownloadPage, DPI controls
// Cập nhật gần đây:
//   - 2026-05-12: Added ChkDeactivateWindows to the Windows activation flow and built-in DLV -> UPK hover/status wiring
//   - 2026-04-30: Added Office Tool Plus Releases URL to hover and copy-link flows
//   - 2026-04-28: Added Ventoy to Windows Tools selection, hover, and copy-link flows
//   - 2026-04-25: Locked manual DPI changes to each tab's auto-fit ceiling until the next tab selection resets and recomputes it
//   - 2026-04-25: Kept window max height independent from DPI so sparse tabs clamp manual zoom before becoming oversized
//   - 2026-04-25: Added Brave to Browser tab selection, install, hover, and download-link cache flows
//   - 2026-04-25: Suppressed repetitive primary DPI status messages while auto-fitting scale for the selected tab
//   - 2026-04-25: Updated Subtitle Edit cached download link to the new GMTPC portable release URL
//   - 2026-04-24: Added to Subtitle select/install flows, download-link cache, and hover tooltip
//   - 2026-04-23: Fixed Ghost of Tsushima copy-link cache and added WintoHDD to Select None flows
//   - 2026-04-19: Added ChkInstallNeatDM to BtnSelectNoneAllTabs for proper interaction with Select None All Tabs button
//   - 2026-04-22: Replaced ChkGouenjiFonts with ChkGMTPCFonts
//   - 2026-04-19: Added missing checkbox cases in Checkbox_MouseEnter for office/subtitle checkboxes; Added ChkSubtitleDraftGMTPC to _cachedDownloadLinks
//   - 2026-04-19: Added missing checkboxes to Checkbox_MouseEnter: ChkInstallZalo, ; Added missing checkboxes to _cachedDownloadLinks for hover and copy functionality
//   - 2026-03-29 (5): Added ChkVibe, ChkMKVToolNix
//   - 2026-03-29: Added ChkVidCoder and new Subtitle tab support
//   - 2026-03-28: Added ChkSubtitleEdit to BtnSelectAll, BtnSelectNone,
//                 BtnSelectNoneAllTabs, UpdateInstallButtonState, BtnInstall_Click
//   - 2026-03-26: Added and to BtnSelectAll,
//                 BtnSelectNone, BtnSelectNoneAllTabs, BtnInstall_Click
//   - 2026-03-11: Added to BtnSelectAll, BtnSelectNone,
//                 BtnSelectNoneAllTabs, UpdateInstallButtonState, BtnInstall_Click
//   - 2026-03-05: Thêm currentDPIScale, DPI_STEPS, ApplyDPIScale từ xaml.cs
//                 theo AI_WORKFLOW.md
//   - 2026-03-08: Thêm MouseRightButtonUp cho BtnDownloadPage để copy link
//   - 2026-03-09: Removed ChkAdvancedCodec, ChkTeracopy, ChkVPN1111 references
// AI Summary: 2026-04-28 - Added Ventoy to Windows Tools selection, hover, and copy-link flows
// AI Summary: 2026-05-02 - Wired Windows Setup selection, install, and hover flows
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
using System.Windows.Media.Animation;

namespace GMTPC.Tool
{
    public partial class MainWindow
    {
        // ===================== DPI Scale Fields =====================
        private double currentDPIScale = 1.0;
        private bool _isUpdatingDpiSelection;
        private bool _suppressPrimaryDpiStatus;
        private readonly Dictionary<string, int> _tabMaxDpiIndexByHeader = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly int[] DPI_STEPS = new int[] { 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 200 };
        private const string VIDCODER_DOWNLOAD_URL = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/vidcoder.exe";

        private void ApplyDPIScale()
        {
            ScaleTransform scaleTransform = new ScaleTransform(currentDPIScale, currentDPIScale);
            MainGrid.LayoutTransform = scaleTransform;

            Rect workArea = GetCurrentMonitorWorkAreaDip();
            bool isPortrait = workArea.Height > workArea.Width;
            double designMaxWidth  = isPortrait ? 580  : 1000;
            double designMaxHeight = isPortrait ? 950  : 750;

            this.MaxHeight = Math.Min(designMaxHeight, workArea.Height);
            this.MaxWidth  = Math.Min(designMaxWidth  * currentDPIScale, workArea.Width);
            ApplyResponsiveLayout();

            MainGrid.InvalidateMeasure();
            this.InvalidateMeasure();

            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                this.SizeToContent = SizeToContent.Manual;
                this.Width  = double.NaN;
                this.Height = double.NaN;
                this.SizeToContent = SizeToContent.WidthAndHeight;
            }));

            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                KeepWindowInsideCurrentMonitor();

                try { this.SizeToContent = SizeToContent.Manual; } catch { }
            }));

            int dpiPercent = (int)(currentDPIScale * 100);
            string dpiText = $"{dpiPercent}%";

            ComboBoxItem selectedItem = CboDPIValue.SelectedItem as ComboBoxItem;
            string currentSelection  = selectedItem?.Content.ToString() ?? "";

            if (currentSelection != dpiText)
            {
                foreach (ComboBoxItem item in CboDPIValue.Items)
                {
                    if (item.Content.ToString() == dpiText)
                    {
                        try
                        {
                            _isUpdatingDpiSelection = true;
                            CboDPIValue.SelectedItem = item;
                        }
                        finally
                        {
                            _isUpdatingDpiSelection = false;
                        }
                        break;
                    }
                }
            }

            if (!_suppressPrimaryDpiStatus)
            {
                UpdateStatus($"Đã đặt tỷ lệ DPI: {dpiText}", "Green");
            }
        }

        private void ResetDPIButtonStates() { /* Kept for compatibility */ }
        private int GetCurrentDpiStepIndex()
        {
            int currentPercent = (int)Math.Round(currentDPIScale * 100.0);
            int idx = Array.IndexOf(DPI_STEPS, currentPercent);
            if (idx >= 0) return idx;

            int closest = 0;
            int minDiff = int.MaxValue;
            for (int i = 0; i < DPI_STEPS.Length; i++)
            {
                int diff = Math.Abs(DPI_STEPS[i] - currentPercent);
                if (diff < minDiff) { minDiff = diff; closest = i; }
            }

            return closest;
        }

        private string GetSelectedTabHeader()
        {
            try
            {
                if (MainTabControl?.SelectedItem is TabItem selectedTab)
                {
                    return selectedTab.Header?.ToString() ?? string.Empty;
                }
            }
            catch
            {
            }

            return string.Empty;
        }

        private int GetBase100DpiIndex()
        {
            int baseIndex = Array.IndexOf(DPI_STEPS, 100);
            if (baseIndex >= 0) return baseIndex;
            return GetClosestDpiStepIndex(100);
        }

        private void ResetSelectedTabDpiLimitTo100Percent()
        {
            string header = GetSelectedTabHeader();
            if (string.IsNullOrWhiteSpace(header)) return;

            _tabMaxDpiIndexByHeader[header] = GetBase100DpiIndex();
        }

        private void SetSelectedTabDpiLimitIndex(int maxIndex)
        {
            string header = GetSelectedTabHeader();
            if (string.IsNullOrWhiteSpace(header)) return;

            maxIndex = Math.Max(0, Math.Min(DPI_STEPS.Length - 1, maxIndex));
            _tabMaxDpiIndexByHeader[header] = maxIndex;
        }

        private int GetSelectedTabDpiLimitIndex()
        {
            if (IsSystemInformationTabSelected())
            {
                return GetBase100DpiIndex();
            }

            string header = GetSelectedTabHeader();
            if (!string.IsNullOrWhiteSpace(header) && _tabMaxDpiIndexByHeader.TryGetValue(header, out int maxIndex))
            {
                return Math.Max(0, Math.Min(DPI_STEPS.Length - 1, maxIndex));
            }

            return DPI_STEPS.Length - 1;
        }

        private bool TrySetDpiIndexSafelyForCurrentTab(int targetIndex)
        {
            if (_isAutoFittingScale) return false;
            if (ForceSystemInformationDpiTo100Percent()) return false;

            int maxAllowedIndex = GetSelectedTabDpiLimitIndex();
            targetIndex = Math.Max(0, Math.Min(DPI_STEPS.Length - 1, targetIndex));
            targetIndex = Math.Min(targetIndex, maxAllowedIndex);
            int currentIndex = GetCurrentDpiStepIndex();
            if (targetIndex == currentIndex) return false;

            if (targetIndex < currentIndex)
            {
                SetCurrentDpiStepSilently(targetIndex);
                ApplyDPIScale();
                return true;
            }

            Rect workArea = GetCurrentMonitorWorkAreaDip();
            int safeIndex = currentIndex;
            bool changed = false;

            for (int idx = currentIndex + 1; idx <= targetIndex; idx++)
            {
                SetCurrentDpiStepSilently(idx);
                ApplyDPIScale();
                MainGrid.UpdateLayout();
                UpdateLayout();
                changed = true;

                if (IsCurrentScaleOverflowingForTabFit(workArea))
                {
                    SetCurrentDpiStepSilently(safeIndex);
                    ApplyDPIScale();
                    MainGrid.UpdateLayout();
                    UpdateLayout();
                    return safeIndex != currentIndex;
                }

                safeIndex = idx;
            }

            return changed;
        }

        private bool ForceSystemInformationDpiTo100Percent()
        {
            if (!IsSystemInformationTabSelected()) return false;

            int baseIndex = Array.IndexOf(DPI_STEPS, 100);
            if (baseIndex < 0) baseIndex = GetClosestDpiStepIndex(100);

            try
            {
                _suppressPrimaryDpiStatus = true;
                currentDPIScale = DPI_STEPS[baseIndex] / 100.0;
                SetDPIComboIndexSilently(baseIndex);
                ApplyDPIScale();
                return true;
            }
            finally
            {
                _suppressPrimaryDpiStatus = false;
            }
        }

        private void BtnDPIMinus_Click(object sender, RoutedEventArgs e)
        {
            if (ForceSystemInformationDpiTo100Percent()) return;

            int idx = GetCurrentDpiStepIndex();
            if (idx <= 0) idx = 0; else idx--;
            if (TrySetDpiIndexSafelyForCurrentTab(idx))
            {
                UpdateSecondaryStatus($"Đã đổi DPI: {DPI_STEPS[GetCurrentDpiStepIndex()]}%", "Cyan");
            }
        }

        private void BtnDPIPlus_Click(object sender, RoutedEventArgs e)
        {
            if (ForceSystemInformationDpiTo100Percent()) return;

            int idx = GetCurrentDpiStepIndex();
            if (idx < DPI_STEPS.Length - 1) idx++; else idx = DPI_STEPS.Length - 1;
            if (TrySetDpiIndexSafelyForCurrentTab(idx))
            {
                UpdateSecondaryStatus($"Đã đổi DPI: {DPI_STEPS[GetCurrentDpiStepIndex()]}%", "Cyan");
            }
        }

        private void CboDPIValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingDpiSelection) return;
            if (CboDPIValue.SelectedItem == null) return;
            if (ForceSystemInformationDpiTo100Percent()) return;

            // Get the ComboBoxItem and extract its Content
            ComboBoxItem selectedItem = CboDPIValue.SelectedItem as ComboBoxItem;
            if (selectedItem == null) return;

            string selectedValue = selectedItem.Content.ToString();

            // Parse the percentage and convert to decimal
            if (selectedValue.EndsWith("%"))
            {
                selectedValue = selectedValue.Substring(0, selectedValue.Length - 1);
            }

            if (double.TryParse(selectedValue, out double percent))
            {
                // Ensure the selected percent is among our allowed steps. If not, snap to the nearest.
                int sel = (int)Math.Round(percent);
                int closest = 0; int minDiff = int.MaxValue;
                for (int i = 0; i < DPI_STEPS.Length; i++)
                {
                    int diff = Math.Abs(DPI_STEPS[i] - sel);
                    if (diff < minDiff) { minDiff = diff; closest = i; }
                }
                int currentIndex = Array.IndexOf(DPI_STEPS, (int)Math.Round(currentDPIScale * 100.0));
                if (currentIndex < 0)
                {
                    currentIndex = 0;
                    int currentDiff = int.MaxValue;
                    int currentPercent = (int)Math.Round(currentDPIScale * 100.0);
                    for (int i = 0; i < DPI_STEPS.Length; i++)
                    {
                        int diff = Math.Abs(DPI_STEPS[i] - currentPercent);
                        if (diff < currentDiff) { currentDiff = diff; currentIndex = i; }
                    }
                }

                if (closest > currentIndex)
                {
                    if (TrySetDpiIndexSafelyForCurrentTab(closest))
                    {
                        UpdateSecondaryStatus($"Đã đổi DPI: {DPI_STEPS[GetCurrentDpiStepIndex()]}%", "Cyan");
                    }
                    else
                    {
                        SetDPIComboIndexSilently(currentIndex);
                    }
                    return;
                }

                if (TrySetDpiIndexSafelyForCurrentTab(closest))
                {
                    UpdateSecondaryStatus($"Đã đổi DPI: {DPI_STEPS[GetCurrentDpiStepIndex()]}%", "Cyan");
                }
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("https://tinyurl.com/gmtpcdonate");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi mở liên kết ủng hộ: {ex.Message}", "Red");
            }
        }

        // ===================== Event Handlers for New Buttons =====================
        private void BtnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Đã chọn tất cả trong tab hiện tại", "Green");

            // Lấy tab hiện tại đang được chọn
            if (MainTabControl.SelectedItem is TabItem selectedTab)
            {
                string tabHeader = selectedTab.Header.ToString();

                // Nếu tab là "Popular"
                if (tabHeader == "Popular")
                {
                    // Chọn tất cả các checkbox trong tab Popular
                    ChkInstallNeatDM.IsChecked = true;
                    ChkDeactivateWindows.IsChecked = true;
                    ChkPauseWindowsUpdate.IsChecked = true;
                    ChkInstallWinRAR.IsChecked = true;
                    ChkVcredist.IsChecked = true;
                    ChkDirectX.IsChecked = true;
                    ChkJava.IsChecked = true;
                    ChkOpenAL.IsChecked = true;
                    Chk3DPChip.IsChecked = true;
                    Chk3DPNet.IsChecked = true;
                    ChkInstallZalo.IsChecked = true;
                }
                // Nếu tab là "System"
                else if (tabHeader == "System")
                {
                    // Chọn checkbox trong tab System
                    ChkDISMPP.IsChecked = true;
                    ChkFolderSize.IsChecked = true;
                    ChkMemReduct.IsChecked = true;
                    ChkVPN1111.IsChecked = true;
                    ChkGoogleDrive.IsChecked = true;
                }
                else if (tabHeader == "Office")
                {
                    ChkOfficeToolPlus.IsChecked = true;
                    ChkGMTPCFonts.IsChecked = true;
                    ChkNotepadPlusPlus.IsChecked = true;
                }
                else if (tabHeader == "Subtitle")
                {
                    ChkSubtitleEdit.IsChecked = true;
                    ChkVidCoder.IsChecked = true;
                    ChkVibe.IsChecked = true;
                    ChkMKVToolNix.IsChecked = true;
                    ChkSubtitleDraftGMTPC.IsChecked = true;
                }
                // Nếu tab là "Partition"
                else if (tabHeader == "Partition")
                {
                    // Chọn checkbox trong tab Partition
                }
                // Nếu tab là "Gaming"
                else if (tabHeader == "Gaming")
                {
                    // Chọn checkbox trong tab Gaming
                    ChkThrottlestop.IsChecked = true;
                    ChkMSIAfterburner.IsChecked = true;
                    ChkLeagueOfLegends.IsChecked = true;
                    ChkPorofessor.IsChecked = true;
                }
                // Nếu tab là "Browser"
                else if (tabHeader == "Browser")
                {
                    ChkChrome.IsChecked = true;
                    ChkCocCoc.IsChecked = true;
                    ChkEdge.IsChecked = true;
                    ChkBrave.IsChecked = true;
                }
                else if (tabHeader == "Multimedia")
                {
                    // Chọn checkbox trong tab Multimedia
                    ChkAdvancedCodecPack.IsChecked = true;
                    ChkPotPlayer.IsChecked = true;
                    ChkFoxit.IsChecked = true;
                }
                // Nếu tab là "Remote Desktop"
                else if (tabHeader == "Remote Desktop")
                {
                    // Chọn checkbox trong tab Remote Desktop
                    ChkUltraviewer.IsChecked = true;
                    ChkTeamViewerQS.IsChecked = true;
                    ChkTeamViewerFull.IsChecked = true;
                    ChkAnyDesk.IsChecked = true;
                }
                else if (tabHeader == "Driver")
                {
                    Chk3DPChip.IsChecked = true;
                    Chk3DPNet.IsChecked = true;
                }
                else if (tabHeader == "Windows")
                {
                    ChkWin11_26H1.IsChecked = true;
                    ChkVentoy.IsChecked = true;
                }
            }

            UpdateInstallButtonState();
        }

        private void BtnSelectNone_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Đã hủy chọn tất cả trong tab hiện tại", "Yellow");

            // Lấy tab hiện tại đang được chọn
            if (MainTabControl.SelectedItem is TabItem selectedTab)
            {
                string tabHeader = selectedTab.Header.ToString();

                // Nếu tab là "Popular"
                if (tabHeader == "Popular")
                {
                    // Bỏ chọn tất cả các checkbox trong tab Popular
                    ChkInstallNeatDM.IsChecked = false;
                    ChkDeactivateWindows.IsChecked = false;
                    ChkPauseWindowsUpdate.IsChecked = false;
                    ChkInstallWinRAR.IsChecked = false;
                    ChkVcredist.IsChecked = false;
                    ChkDirectX.IsChecked = false;
                    ChkJava.IsChecked = false;
                    ChkOpenAL.IsChecked = false;
                    Chk3DPChip.IsChecked = false;
                    Chk3DPNet.IsChecked = false;
                    ChkInstallZalo.IsChecked = false;
                }
                // Nếu tab là "System"
                else if (tabHeader == "System")
                {
                    // Bỏ chọn checkbox trong tab System
                    ChkDISMPP.IsChecked = false;
                    ChkFolderSize.IsChecked = false;
                    ChkMemReduct.IsChecked = false;
                    ChkVPN1111.IsChecked = false;
                    ChkGoogleDrive.IsChecked = false;
                }
                else if (tabHeader == "Office")
                {
                    ChkOfficeToolPlus.IsChecked = false;
                    ChkGMTPCFonts.IsChecked = false;
                    ChkNotepadPlusPlus.IsChecked = false;
                }
                else if (tabHeader == "Subtitle")
                {
                    ChkSubtitleEdit.IsChecked = false;
                    ChkVidCoder.IsChecked = false;
                    ChkVibe.IsChecked = false;
                    ChkMKVToolNix.IsChecked = false;
                    ChkSubtitleDraftGMTPC.IsChecked = false;
                }
                // Nếu tab là "Partition"
                else if (tabHeader == "Partition")
                {
                    // Bỏ chọn checkbox trong tab Partition
                }
                // Nếu tab là "Gaming"
                else if (tabHeader == "Gaming")
                {
                    // Bỏ chọn checkbox trong tab Gaming
                    ChkThrottlestop.IsChecked = false;
                    ChkMSIAfterburner.IsChecked = false;
                    ChkLeagueOfLegends.IsChecked = false;
                    ChkPorofessor.IsChecked = false;
                }
                // Nếu tab là "Browser"
                else if (tabHeader == "Browser")
                {
                    ChkChrome.IsChecked = false;
                    ChkCocCoc.IsChecked = false;
                    ChkEdge.IsChecked = false;
                    ChkBrave.IsChecked = false;
                }
                else if (tabHeader == "Multimedia")
                {
                    // Bỏ chọn checkbox trong tab Multimedia
                    ChkAdvancedCodecPack.IsChecked = false;
                    ChkPotPlayer.IsChecked = false;
                    ChkFoxit.IsChecked = false;
                }
                // Nếu tab là "Remote Desktop"
                else if (tabHeader == "Remote Desktop")
                {
                    // Bỏ chọn checkbox trong tab Remote Desktop
                    ChkUltraviewer.IsChecked = false;
                    ChkTeamViewerQS.IsChecked = false;
                    ChkTeamViewerFull.IsChecked = false;
                    ChkAnyDesk.IsChecked = false;
                }
                else if (tabHeader == "Driver")
                {
                    Chk3DPChip.IsChecked = false;
                    Chk3DPNet.IsChecked = false;
                }
                else if (tabHeader == "Windows")
                {
                    ChkWin11_26H1.IsChecked = false;
                    ChkVentoy.IsChecked = false;
                }
            }

            UpdateInstallButtonState();
        }

        private void BtnSelectNoneAllTabs_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Đã hủy chọn tất cả trong tất cả các tab", "Yellow");

            // Bỏ chọn tất cả các checkbox trong tab Popular
            ChkInstallNeatDM.IsChecked = false;
            ChkDeactivateWindows.IsChecked = false;
            ChkPauseWindowsUpdate.IsChecked = false;
            ChkInstallWinRAR.IsChecked = false;
            ChkVcredist.IsChecked = false;
            ChkDirectX.IsChecked = false;
            ChkJava.IsChecked = false;
            ChkOpenAL.IsChecked = false;
            Chk3DPChip.IsChecked = false;
            Chk3DPNet.IsChecked = false;
            ChkChrome.IsChecked = false;
            ChkCocCoc.IsChecked = false;
            ChkEdge.IsChecked = false;
            ChkBrave.IsChecked = false;
            ChkPotPlayer.IsChecked = false;
            ChkFoxit.IsChecked = false;
            ChkAdvancedCodecPack.IsChecked = false;
            ChkInstallZalo.IsChecked = false;

            // Bỏ chọn checkbox trong tab System
            ChkDISMPP.IsChecked = false;
            ChkFolderSize.IsChecked = false;
            ChkMemReduct.IsChecked = false;
            ChkVPN1111.IsChecked = false;
            ChkGoogleDrive.IsChecked = false;

            // Bỏ chọn checkbox trong tab Partition

            // Bỏ chọn checkbox trong tab Gaming
            ChkThrottlestop.IsChecked = false;
            ChkMSIAfterburner.IsChecked = false;
            ChkLeagueOfLegends.IsChecked = false;
            ChkPorofessor.IsChecked = false;

            // Bỏ chọn checkbox trong tab Remote Desktop
            ChkUltraviewer.IsChecked = false;
            ChkTeamViewerQS.IsChecked = false;
            ChkTeamViewerFull.IsChecked = false;
            ChkAnyDesk.IsChecked = false;

            // Bỏ chọn checkbox trong tab Driver
            Chk3DPChip.IsChecked = false;
            Chk3DPNet.IsChecked = false;

            // Bỏ chọn checkbox trong tab Office
            ChkOfficeToolPlus.IsChecked = false;
            ChkGMTPCFonts.IsChecked = false;
            ChkNotepadPlusPlus.IsChecked = false;

            // Bỏ chọn checkbox trong tab Subtitle
            ChkSubtitleEdit.IsChecked = false;
            ChkVidCoder.IsChecked = false;
            ChkVibe.IsChecked = false;
            ChkMKVToolNix.IsChecked = false;
            ChkSubtitleDraftGMTPC.IsChecked = false;

            // Bỏ chọn checkbox trong tab Windows - Microsoft
            ChkWin11_26H1.IsChecked = false;

            // Bỏ chọn checkbox trong tab Windows Tools
            ChkVentoy.IsChecked = false;

            UpdateInstallButtonState();
        }

        private async void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            // Đặt trạng thái đang cài đặt
            SetInstallingState(true);

            UpdateStatus("Đang chờ...", "Yellow"); // Thêm phản hồi tức thì
            await Task.Delay(1); // Cho phép UI cập nhật ngay lập tức

            _cancellationTokenSource = new CancellationTokenSource();
            _pauseCts = new CancellationTokenSource();
            _pauseEvent.Set(); // Mặc định là không pause
            BtnPause.Content = "Pause";
            BtnStop.IsEnabled = true;
            BtnPause.IsEnabled = true;
            BtnInstall.IsEnabled = false;

            var tasks = new List<(Func<Task> Action, CheckBox CheckBox)>();

            if (ChkInstallNeatDM.IsChecked == true) tasks.Add((InstallNeatDMAsync, ChkInstallNeatDM));
            if (ChkOfficeToolPlus.IsChecked == true) tasks.Add((InstallOfficeToolPlusAsync, ChkOfficeToolPlus)); // Thêm task cho Office Tool Plus
            if (ChkPauseWindowsUpdate.IsChecked == true) tasks.Add((() => Task.Run(() => PauseWindowsUpdate()), ChkPauseWindowsUpdate));
            if (ChkInstallWinRAR.IsChecked == true) tasks.Add((InstallAndActivateWinRARAsync, ChkInstallWinRAR));
            if (ChkVcredist.IsChecked == true) tasks.Add((InstallVcredistAsync, ChkVcredist));
            if (ChkDirectX.IsChecked == true) tasks.Add((InstallDirectXAsync, ChkDirectX));
            if (ChkJava.IsChecked == true) tasks.Add((InstallJavaAsync, ChkJava));
            if (ChkOpenAL.IsChecked == true) tasks.Add((InstallOpenALAsync, ChkOpenAL));
            if (ChkChrome.IsChecked == true) tasks.Add((InstallChromeAsync, ChkChrome));
            if (ChkCocCoc.IsChecked == true) tasks.Add((InstallCocCocAsync, ChkCocCoc));
            if (ChkEdge.IsChecked == true) tasks.Add((InstallEdgeAsync, ChkEdge));
            if (ChkBrave.IsChecked == true) tasks.Add((InstallBraveAsync, ChkBrave));
            if (ChkPotPlayer.IsChecked == true) tasks.Add((InstallPotPlayerAsync, ChkPotPlayer));
            if (ChkFoxit.IsChecked == true) tasks.Add((InstallFoxitAsync, ChkFoxit));
            if (ChkAdvancedCodecPack.IsChecked == true) tasks.Add((InstallAdvancedCodecPackAsync, ChkAdvancedCodecPack));
            if (ChkInstallZalo.IsChecked == true) tasks.Add((InstallZaloAsync, ChkInstallZalo));
            if (Chk3DPChip.IsChecked == true) tasks.Add((Run3DPChipAsync, Chk3DPChip));
            if (Chk3DPNet.IsChecked == true) tasks.Add((Install3DPNetAsync, Chk3DPNet));
            if (ChkDeactivateWindows.IsChecked == true) tasks.Add((() => Task.Run(() => DeactivateWindows()), ChkDeactivateWindows));
            if (ChkDISMPP.IsChecked == true) tasks.Add((InstallDISMPPAsync, ChkDISMPP));
            if (ChkGMTPCFonts.IsChecked == true) tasks.Add((InstallGMTPCFontsAsync, ChkGMTPCFonts));
            if (ChkNotepadPlusPlus.IsChecked == true) tasks.Add((InstallNotepadPlusPlusAsync, ChkNotepadPlusPlus));
            if (ChkSubtitleEdit.IsChecked == true) tasks.Add((InstallSubtitleEditAsync, ChkSubtitleEdit));
            if (ChkVidCoder.IsChecked == true) tasks.Add((InstallVidCoderAsync, ChkVidCoder));
            if (ChkVibe.IsChecked == true) tasks.Add((InstallVibeAsync, ChkVibe));
            if (ChkMKVToolNix.IsChecked == true) tasks.Add((InstallMKVToolNixAsync, ChkMKVToolNix));
            if (ChkSubtitleDraftGMTPC.IsChecked == true) tasks.Add((InstallSubtitleDraftGMTPCAsync, ChkSubtitleDraftGMTPC));
            // Only add once to avoid duplicate install and MessageBox
            if (ChkMemReduct.IsChecked == true) tasks.Add((InstallMemReductAsync, ChkMemReduct));
            if (ChkVPN1111.IsChecked == true) tasks.Add((InstallVPN1111Async, ChkVPN1111));
            if (ChkGoogleDrive.IsChecked == true) tasks.Add((InstallGoogleDriveAsync, ChkGoogleDrive));
            if (ChkFolderSize.IsChecked == true) tasks.Add((InstallFolderSizeAsync, ChkFolderSize));
            if (ChkThrottlestop.IsChecked == true) tasks.Add((InstallThrottlestopAsync, ChkThrottlestop));
            if (ChkMSIAfterburner.IsChecked == true) tasks.Add((InstallMSIAfterburnerAsync, ChkMSIAfterburner));
            if (ChkLeagueOfLegends.IsChecked == true) tasks.Add((InstallLeagueOfLegendsVNAsync, ChkLeagueOfLegends));
            if (ChkPorofessor.IsChecked == true) tasks.Add((InstallPorofessorAsync, ChkPorofessor));
            if (ChkUltraviewer.IsChecked == true) tasks.Add((InstallUltraviewerAsync, ChkUltraviewer));
            if (ChkTeamViewerQS.IsChecked == true) tasks.Add((InstallTeamViewerQuickSupportAsync, ChkTeamViewerQS));
            if (ChkTeamViewerFull.IsChecked == true) tasks.Add((InstallTeamViewerFullAsync, ChkTeamViewerFull));
            if (ChkAnyDesk.IsChecked == true) tasks.Add((InstallAnyDeskAsync, ChkAnyDesk));
            if (ChkWin11_26H1.IsChecked == true) tasks.Add((InstallWin11_26H1Async, ChkWin11_26H1));
            if (ChkVentoy.IsChecked == true) tasks.Add((InstallVentoyAsync, ChkVentoy));

            CheckBox currentTaskCheckBox = null;
            bool hadError = false;
            bool wasCancelled = false;
            try
            {
                foreach (var taskInfo in tasks)
                {
                    currentTaskCheckBox = taskInfo.CheckBox;

                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        UpdateStatus("Quá trình cài đặt đã bị dừng.", "Red");
                        wasCancelled = true;
                        break;
                    }

                    // Delay 500ms giữa các task để tránh tải nhiều file cùng lúc gây lỗi server
                    if (tasks.Count > 1)
                    {
                        await Task.Delay(500);
                    }

                    await taskInfo.Action();

                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        if (taskInfo.CheckBox != null)
                        {
                            taskInfo.CheckBox.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                        }
                        UpdateStatus("Quá trình cài đặt đã bị dừng.", "Red");
                        wasCancelled = true;
                        break;
                    }

                    if (taskInfo.CheckBox != null)
                    {
                        taskInfo.CheckBox.IsChecked = false;
                        taskInfo.CheckBox.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Cyan);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                wasCancelled = true;
                if (currentTaskCheckBox != null)
                {
                    currentTaskCheckBox.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                }
                UpdateStatus("Quá trình cài đặt đã bị hủy.", "Red");
            }
            catch (Exception ex)
            {
                hadError = true;
                UpdateStatus($"Đã xảy ra lỗi: {ex.Message}", "Red");
            }
            finally
            {
                BtnStop.IsEnabled = false;
                BtnPause.IsEnabled = false;
                UpdateInstallButtonState();
                if (hadError)
                {
                    UpdateStatus("Đã hoàn tất hàng đợi tác vụ, nhưng có lỗi trong quá trình cài đặt.", "Red");
                }
                else if (wasCancelled)
                {
                    UpdateStatus("Đã dừng hàng đợi tác vụ.", "Yellow");
                }
                else
                {
                    UpdateStatus("Hoàn tất tất cả các tác vụ.", "Green");
                }
                
                // Đặt lại trạng thái không còn cài đặt
                SetInstallingState(false);
            }
        }

        // Cache các link download khi hover
        private List<string> _cachedDownloadLinks = new List<string>();

        private void BtnDownloadPage_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Xây dựng danh sách link download khi hover
            _cachedDownloadLinks.Clear();


            if (ChkInstallWinRAR?.IsChecked == true)
                _cachedDownloadLinks.Add("https://github.com/ghostminhtoan/MMT/releases/download/v1.0/WinRAR.7.13.exe");



            if (ChkPauseWindowsUpdate?.IsChecked == true)
                _cachedDownloadLinks.Add("https://github.com/ghostminhtoan/MMT/releases/download/test/pause.update.win.11.ps1");

            if (ChkVcredist?.IsChecked == true)
                _cachedDownloadLinks.Add("https://github.com/ghostminhtoan/MMT/releases/download/v1.0/vcredist.all.in.one.by.MMT.Windows.Tech.exe");

            if (ChkDirectX?.IsChecked == true)
                _cachedDownloadLinks.Add("https://github.com/ghostminhtoan/MMT/releases/download/v1.0/DirectX.exe");

            if (ChkJava?.IsChecked == true)
                _cachedDownloadLinks.Add("https://javadl.oracle.com/webapps/download/AutoDL?BundleId=252627_99a6cb9582554a09bd4ac60f73f9b8e6");

            if (ChkOpenAL?.IsChecked == true)
                _cachedDownloadLinks.Add("https://github.com/ghostminhtoan/MMT/releases/download/v1.0/OpenAL.exe");

            if (ChkChrome?.IsChecked == true)
                _cachedDownloadLinks.Add("https://dl.google.com/tag/s/appguid%3D%7B8A69D345-D564-463C-AFF1-A69D9E530F96%7D%26iid%3D%7BE9FD60DA-2FFA-E657-6449-67646C84E6C0%7D%26lang%3Dvi%26browser%3D5%26usagestats%3D1%26appname%3DGoogle%2520Chrome%26needsadmin%3Dprefers%26ap%3Dx64-statsdef_1%26installdataindex%3Dempty/update2/installers/ChromeSetup.exe");

            if (ChkCocCoc?.IsChecked == true)
                _cachedDownloadLinks.Add("https://files.coccoc.com/browser/coccoc_standalone_vi.exe");

            if (ChkEdge?.IsChecked == true)
                _cachedDownloadLinks.Add("https://c2rsetup.officeapps.live.com/c2r/downloadEdge.aspx?platform=Default&source=EdgeStablePage&Channel=Stable&language=vi&brand=M100");

            if (ChkBrave?.IsChecked == true)
                _cachedDownloadLinks.Add("https://laptop-updates.brave.com/download/BRV010?bitness=64");


            if (Chk3DPNet?.IsChecked == true)
                _cachedDownloadLinks.Add("https://github.com/ghostminhtoan/MMT/releases/download/v1.0/3DP.Net.exe");

            if (Chk3DPChip?.IsChecked == true)
                _cachedDownloadLinks.Add("https://github.com/ghostminhtoan/MMT/releases/download/v1.0/3DP.Chip.exe");


            if (ChkDISMPP?.IsChecked == true)
                _cachedDownloadLinks.Add("https://github.com/ghostminhtoan/MMT/releases/download/WinPE/DISM++.exe");



            if (ChkUltraviewer?.IsChecked == true)
                _cachedDownloadLinks.Add("https://dl2.ultraviewer.net/UltraViewer_setup_6.6_vi.exe");

            if (ChkPotPlayer?.IsChecked == true)
                _cachedDownloadLinks.Add("https://t1.daumcdn.net/potplayer/PotPlayer/Version/Latest/PotPlayerSetup64.exe");


            if (ChkFoxit?.IsChecked == true)
                _cachedDownloadLinks.Add("https://cdn01.foxitsoftware.com/product/reader/desktop/win/2025.2.0/FoxitPDFReader20252_L10N_Setup_Prom_x64.exe");


            if (ChkAdvancedCodecPack?.IsChecked == true)
                _cachedDownloadLinks.Add("https://github.com/ghostminhtoan/MMT/releases/download/v1.0/ADVANCED_Codec_Pack.exe");


            if (ChkVPN1111?.IsChecked == true)
                _cachedDownloadLinks.Add("https://1111-releases.cloudflareclient.com/win/latest");

            if (ChkTeamViewerQS?.IsChecked == true)
                _cachedDownloadLinks.Add("https://dl.teamviewer.com/download/TeamViewerQS_x64.exe");

            if (ChkTeamViewerFull?.IsChecked == true)
                _cachedDownloadLinks.Add("https://tinyurl.com/teamviewerlatest");

            if (ChkAnyDesk?.IsChecked == true)
                _cachedDownloadLinks.Add("https://tinyurl.com/anydesk621");


            if (ChkOfficeToolPlus?.IsChecked == true)
                _cachedDownloadLinks.Add(OFFICE_TOOL_PLUS_RELEASES_URL);



            if (ChkGMTPCFonts?.IsChecked == true)
                _cachedDownloadLinks.Add(GMTPC_FONTS_DOWNLOAD_URL);

            if (ChkNotepadPlusPlus?.IsChecked == true)
                _cachedDownloadLinks.Add(NOTEPAD_PLUS_PLUS_DOWNLOAD_URL);

            if (ChkInstallNeatDM?.IsChecked == true)
                _cachedDownloadLinks.Add("https://neatdownloadmanager.com/file/NeatDM_setup.exe");

            if (ChkInstallZalo?.IsChecked == true)
                _cachedDownloadLinks.Add("https://zalo.me/download/zalo-pc?utm=90000");


            if (ChkVentoy?.IsChecked == true)
                _cachedDownloadLinks.Add(VENTOY_RELEASES_URL);


            if (ChkSubtitleEdit?.IsChecked == true)
                _cachedDownloadLinks.Add(SUBTITLE_EDIT_DOWNLOAD_URL);

            if (ChkVidCoder?.IsChecked == true)
                _cachedDownloadLinks.Add(VIDCODER_DOWNLOAD_URL);


            if (ChkVibe?.IsChecked == true)
                _cachedDownloadLinks.Add("https://github.com/ghostminhtoan/MMT/releases/download/v1.0/vibe.exe");

            if (ChkMKVToolNix?.IsChecked == true)
                _cachedDownloadLinks.Add("https://github.com/ghostminhtoan/MMT/releases/download/v1.0/mkvtoolnix-mkvcleaver.exe");

            if (ChkSubtitleDraftGMTPC?.IsChecked == true)
                _cachedDownloadLinks.Add(SUBTITLE_DRAFT_GMTPC_DOWNLOAD_URL);



            if (ChkMemReduct?.IsChecked == true)
                _cachedDownloadLinks.Add(MEMREDUCT_DOWNLOAD_URL);

            if (ChkGoogleDrive?.IsChecked == true)
                _cachedDownloadLinks.Add("https://dl.google.com/drive-file-stream/GoogleDriveSetup.exe");


            if (ChkFolderSize?.IsChecked == true)
                _cachedDownloadLinks.Add("https://github.com/ghostminhtoan/MMT/releases/download/v1.0/FolderSize-2.6-x64.msi");



            if (ChkThrottlestop?.IsChecked == true)
                _cachedDownloadLinks.Add("https://github.com/ghostminhtoan/MMT/releases/download/v1.0/Throttlestop.zip");

            if (ChkMSIAfterburner?.IsChecked == true)
                _cachedDownloadLinks.Add("https://github.com/ghostminhtoan/MMT/releases/download/v1.0/MSI.Afterburner.exe");

            if (ChkLeagueOfLegends?.IsChecked == true)
                _cachedDownloadLinks.Add("https://lol.secure.dyn.riotcdn.net/channels/public/x/installer/current/live.vn2.exe");

            if (ChkPorofessor?.IsChecked == true)
                _cachedDownloadLinks.Add("https://download.overwolf.com/installer/prod/339334cdda5e1ea8a3c8a31ba816fb37/Porofessor%20Standalone%20-%20Installer.exe");



            if (ChkWin11_26H1?.IsChecked == true)
                _cachedDownloadLinks.Add("https://archive.org/download/microsoft-win11-26h2-february-2026/en-us_windows_11_consumer_editions_version_26h1_x64_dvd_5208fe5b.iso");


            // Hiển thị tooltip với danh sách link
            string tooltipText;
            if (_cachedDownloadLinks.Count == 0)
            {
                tooltipText = "Vui lòng chọn (check) các checkbox ứng với phần mềm muốn tải để xem link download trực tiếp";
            }
            else
            {
                tooltipText = $"Click để mở {_cachedDownloadLinks.Count} link:\n" + string.Join("\n", _cachedDownloadLinks);
            }

            BtnDownloadPage.ToolTip = new System.Windows.Controls.ToolTip
            {
                Content = tooltipText,
                Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse
            };
        }

        private void BtnDownloadPage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BtnDownloadPage_MouseEnter(sender, null);

                // Sử dụng danh sách link mới nhất theo checkbox đang chọn
                if (_cachedDownloadLinks.Count == 0)
                {
                    UpdateStatus("Vui lòng chọn (check) các checkbox ứng với phần mềm muốn tải trước", "Orange");
                }
                else
                {
                    // Mở tất cả các liên kết đã chọn
                    foreach (var link in _cachedDownloadLinks)
                    {
                        Process.Start(link);
                    }
                    UpdateStatus($"Đã mở {_cachedDownloadLinks.Count} liên kết tải về tương ứng", "Green");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi mở trang download: {ex.Message}", "Red");
            }
        }

        private void BtnDownloadPage_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            BtnDownloadPage_MouseEnter(sender, null);

            var contextMenu = new ContextMenu();
            var copyItem = new MenuItem { Header = "Copy Link" };
            copyItem.Click += (s, args) =>
            {
                if (_cachedDownloadLinks.Count > 0)
                {
                    string allLinks = string.Join("\n", _cachedDownloadLinks);
                    Clipboard.SetText(allLinks);
                    UpdateStatus($"Đã copy {_cachedDownloadLinks.Count} link vào clipboard", "Green");
                }
                else
                {
                    UpdateStatus("Không có link nào để copy", "Orange");
                }
            };
            contextMenu.Items.Add(copyItem);
            contextMenu.IsOpen = true;
            e.Handled = true;
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                UpdateStatus("Đang dừng quá trình cài đặt...", "Yellow");
                BtnStop.IsEnabled = false;
                BtnPause.IsEnabled = false;
                BtnInstall.IsEnabled = true;

                // Resume event if cancelled while paused
                if (!_pauseEvent.IsSet)
                {
                    _pauseEvent.Set();
                    BtnPause.Content = "Pause";
                }

                // Đặt lại tiến độ khi bị hủy
                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 0;
                    ConnectionTraceGrid.Children.Clear();
                    ProgressTextBlock.Text = "";
                    SpeedTextBlock.Text = "";
                });
            }
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            // Only allow pause during download
            if (_pauseEvent == null || !BtnPause.IsEnabled)
                return;

            if (_pauseEvent.IsSet)
            {
                // Đang chạy -> Tạm dừng
                _pauseEvent.Reset();
                if (_pauseCts != null && !_pauseCts.IsCancellationRequested)
                    _pauseCts.Cancel(); // Ngắt ngay lập tức mạng
                BtnPause.Content = "Resume";
                UpdateStatus("Đã tạm dừng quá trình tải xuống (Đang ngắt kết nối...)", "Yellow");
            }
            else
            {
                // Đang tạm dừng -> Chạy tiếp
                if (_pauseCts == null || _pauseCts.IsCancellationRequested)
                    _pauseCts = new CancellationTokenSource();
                _pauseEvent.Set();
                BtnPause.Content = "Pause";
                UpdateStatus("Đang tiếp tục quá trình tải xuống...", "Cyan");
            }
        }

        private void BtnRefreshColor_Click(object sender, RoutedEventArgs e)
        {
            var allCheckBoxes = new System.Windows.Controls.CheckBox[]
            {
                ChkInstallWinRAR, ChkDeactivateWindows, ChkPauseWindowsUpdate, ChkVcredist, ChkDirectX, ChkJava, ChkOpenAL,
                Chk3DPChip, Chk3DPNet, ChkOfficeToolPlus, ChkGMTPCFonts,
                ChkPotPlayer, ChkFoxit, ChkAdvancedCodecPack,
                ChkDISMPP, ChkFolderSize, ChkMemReduct, ChkVPN1111, ChkGoogleDrive,
                ChkThrottlestop, ChkMSIAfterburner, ChkLeagueOfLegends, ChkPorofessor,
                ChkChrome, ChkCocCoc, ChkEdge,
                ChkBrave,
                ChkUltraviewer, ChkTeamViewerQS, ChkTeamViewerFull, ChkAnyDesk, // ChkWin10ProWorkstations22H2 removed
            };

            foreach (var chk in allCheckBoxes)
            {
                if (chk != null)
                    chk.ClearValue(System.Windows.Controls.Control.ForegroundProperty);
            }
        }

        // Hiển thị link download khi hover vào checkbox
        private void Checkbox_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                string link = null;
                switch (checkBox.Name)
                {
                    case "ChkInstallNeatDM":
                        link = "https://neatdownloadmanager.com/file/NeatDM_setup.exe";
                        break;
                    case "ChkInstallWinRAR":
                        link = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/WinRAR.7.13.exe";
                        break;
                    case "ChkDeactivateWindows":
                        link = "Built-in Windows flow: slmgr /dlv -> copy Activation ID -> slmgr /upk x";
                        break;
                    case "ChkPauseWindowsUpdate":
                        link = "https://github.com/ghostminhtoan/MMT/releases/download/test/pause.update.win.11.ps1";
                        break;
                    case "ChkVcredist":
                        link = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/vcredist.all.in.one.by.MMT.Windows.Tech.exe";
                        break;
                    case "ChkDirectX":
                        link = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/DirectX.exe";
                        break;
                    case "ChkJava":
                        link = "https://javadl.oracle.com/webapps/download/AutoDL?BundleId=252627_99a6cb9582554a09bd4ac60f73f9b8e6";
                        break;
                    case "ChkOpenAL":
                        link = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/OpenAL.exe";
                        break;
                    case "Chk3DPChip":
                        link = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/3DP.Chip.exe";
                        break;
                    case "Chk3DPNet":
                        link = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/3DP.Net.exe";
                        break;
                    case "ChkOfficeToolPlus":
                        link = OFFICE_TOOL_PLUS_RELEASES_URL;
                        break;
                    case "ChkPotPlayer":
                        link = "https://t1.daumcdn.net/potplayer/PotPlayer/Version/Latest/PotPlayerSetup64.exe";
                        break;
                    case "ChkFoxit":
                        link = "https://cdn01.foxitsoftware.com/product/reader/desktop/win/2025.2.0/FoxitPDFReader20252_L10N_Setup_Prom_x64.exe";
                        break;
                    case "ChkAdvancedCodecPack":
                        link = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/ADVANCED_Codec_Pack.exe";
                        break;
                    case "ChkVPN1111":
                        link = "https://1111-releases.cloudflareclient.com/win/latest";
                        break;
                    case "ChkDISMPP":
                        link = "https://github.com/ghostminhtoan/MMT/releases/download/WinPE/DISM++.exe";
                        break;
                    case "ChkMemReduct":
                        link = MEMREDUCT_DOWNLOAD_URL;
                        break;
                    case "ChkGoogleDrive":
                        link = "https://dl.google.com/drive-file-stream/GoogleDriveSetup.exe";
                        break;
                    case "ChkThrottlestop":
                        link = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/Throttlestop.exe";
                        break;
                    case "ChkMSIAfterburner":
                        link = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/MSI.Afterburner.exe";
                        break;
                    case "ChkChrome":
                        link = "https://dl.google.com/tag/s/appguid%3D%7B8A69D345-D564-463C-AFF1-A69D9E530F96%7D%26iid%3D%7BE9FD60DA-2FFA-E657-6449-67646C84E6C0%7D%26lang%3Dvi%26browser%3D5%26usagestats%3D1%26appname%3DGoogle%2520Chrome%26needsadmin%3Dprefers%26ap%3Dx64-statsdef_1%26installdataindex%3Dempty/update2/installers/ChromeSetup.exe";
                        break;
                    case "ChkCocCoc":
                        link = "https://files.coccoc.com/browser/coccoc_standalone_vi.exe";
                        break;
                    case "ChkEdge":
                        link = "https://c2rsetup.officeapps.live.com/c2r/downloadEdge.aspx?platform=Default&source=EdgeStablePage&Channel=Stable&language=vi&brand=M100";
                        break;
                    case "ChkBrave":
                        link = "https://laptop-updates.brave.com/download/BRV010?bitness=64";
                        break;
                    case "ChkUltraviewer":
                        link = "https://dl2.ultraviewer.net/UltraViewer_setup_6.6_vi.exe";
                        break;
                    case "ChkTeamViewerQS":
                        link = "https://download.teamviewer.com/download/TeamViewerQS.exe";
                        break;
                    case "ChkTeamViewerFull":
                        link = "https://download.teamviewer.com/download/TeamViewer_Setup.exe";
                        break;
                    case "ChkAnyDesk":
                        link = "https://anydesk.com/en/downloads/thank-you?dv=win_exe";
                        break;
                    case "ChkGMTPCFonts":
                        link = GMTPC_FONTS_DOWNLOAD_URL;
                        break;
                    case "ChkNotepadPlusPlus":
                        link = NOTEPAD_PLUS_PLUS_DOWNLOAD_URL;
                        break;
                    case "ChkSubtitleEdit":
                        link = SUBTITLE_EDIT_DOWNLOAD_URL;
                        break;
                    case "ChkFolderSize":
                        link = "https://github.com/ghostminhtoan/MMT/releases/download/v1.0/FolderSize.exe";
                        break;
                    case "ChkLeagueOfLegends":
                        link = "https://lol.qq.com/download/";
                        break;
                    case "ChkPorofessor":
                        link = "https://porofessor.gg/downloads";
                        break;
                    case "ChkVidCoder":
                        link = VIDCODER_DOWNLOAD_URL;
                        break;
                    case "ChkVibe":
                        link = VIBE_DOWNLOAD_URL;
                        break;
                    case "ChkMKVToolNix":
                        link = MKVTOOLNIX_DOWNLOAD_URL;
                        break;
                    case "ChkSubtitleDraftGMTPC":
                        link = SUBTITLE_DRAFT_GMTPC_DOWNLOAD_URL;
                        break;
                    case "ChkWin11_26H1":
                        link = "https://archive.org/download/microsoft-win11-26h2-february-2026/en-us_windows_11_consumer_editions_version_26h1_x64_dvd_5208fe5b.iso";
                        break;
                    case "ChkVentoy":
                        link = VENTOY_RELEASES_URL;
                        break;
                    case "ChkInstallZalo":
                        link = "https://zalo.me/download/zalo-pc?utm=90000";
                        break;
                }

                if (!string.IsNullOrEmpty(link))
                {
                    checkBox.ToolTip = new ToolTip
                    {
                        Content = "Link: " + link,
                        Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse,
                        HorizontalOffset = 10,
                        VerticalOffset = 10
                    };
                }
            }
        }
    }
}

