// AI Summary: 2026-04-25 - Added maximize-then-fit DPI logic for tab switching so each selected tab auto-zooms to the largest fully visible scale.
// AI Summary: 2026-04-23 - Added sparse Windows tab overflow detection so DPI reduces before content overlaps buttons.
// WrapPanels now size to the computed column count instead of stretching across the whole monitor.
// =======================================================================
// MainWindow.ResponsiveLayout.cs
// Chuc nang: Desktop responsive layout cho man hinh ngang/doc va multi-monitor.
//            Khong chua logic cai dat, chi dieu chinh kich thuoc va mat do UI.
// =======================================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Forms = System.Windows.Forms;

namespace GMTPC.Tool
{
    public partial class MainWindow
    {
        private bool _isApplyingResponsiveLayout;
        private bool _isAutoFittingScale;
        private bool _hasCompletedInitialTabScaleFit;
        private bool _suppressResponsiveAutoFitQueue;
        private int _tabScaleFitRequestId;
        private CancellationTokenSource _tabScaleFitDelayCts;
        private const int TabScaleFitDelayMs = 700;
        private const int TabScaleFitStepDelayMs = 80;

        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x00010000;

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            DisableMaximizeButton();
            AttachDownloadedFileCleanupOnClose();
        }

        private void DisableMaximizeButton()
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            if (handle == IntPtr.Zero) return;

            if (IntPtr.Size == 8)
            {
                IntPtr style = GetWindowLongPtr64(handle, GWL_STYLE);
                long newStyle = style.ToInt64() & ~WS_MAXIMIZEBOX;
                SetWindowLongPtr64(handle, GWL_STYLE, new IntPtr(newStyle));
            }
            else
            {
                int style = GetWindowLong32(handle, GWL_STYLE);
                SetWindowLong32(handle, GWL_STYLE, style & ~WS_MAXIMIZEBOX);
            }
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source != MainTabControl) return;

            int requestId = ++_tabScaleFitRequestId;
            _suppressResponsiveAutoFitQueue = true;
            CancellationTokenSource previousCts = Interlocked.Exchange(ref _tabScaleFitDelayCts, new CancellationTokenSource());
            if (previousCts != null)
            {
                try { previousCts.Cancel(); } catch { }
                previousCts.Dispose();
            }

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
            {
                if (requestId != _tabScaleFitRequestId) return;
                _ = StartSelectedTabScaleWorkflowAsync(requestId, _tabScaleFitDelayCts.Token);
            }));
        }

        private void ApplyResponsiveLayout()
        {
            if (_isApplyingResponsiveLayout || MainGrid == null) return;

            try
            {
                _isApplyingResponsiveLayout = true;

                Rect workArea = GetCurrentMonitorWorkAreaDip();
                double windowWidth = GetUsableWindowWidth(workArea);
                double rawWindowWidth = GetRawWindowWidth(workArea);
                bool isMonitorPortrait = IsPortrait(workArea);
                bool isWindowPortrait = ActualHeight > 0 && ActualWidth > 0 && ActualHeight > ActualWidth * 1.08;
                bool isCompact = isMonitorPortrait || isWindowPortrait || rawWindowWidth < 760;
                double monitorWidth = GetUsableMonitorWidth(workArea);

                ApplyWindowBounds(workArea, isMonitorPortrait);
                ApplyOuterSpacing(isCompact);
                ApplyTabItemSizing(monitorWidth, isMonitorPortrait, isCompact);
                ApplySparseTabSizing(isCompact);
                ApplyCommandSizing(windowWidth, isCompact);
                ApplyProgressSizing(isCompact);
                UpdateSystemInformationChromeVisibility();
                KeepWindowInsideCurrentMonitor(workArea);
                if (!_hasCompletedInitialTabScaleFit && MainTabControl?.SelectedItem != null && !_suppressResponsiveAutoFitQueue)
                {
                    _hasCompletedInitialTabScaleFit = true;
                    QueueTabScaleFrom100ToBestFit(_tabScaleFitRequestId);
                }
                else if (!_suppressResponsiveAutoFitQueue && !ShouldSkipAutoFitScale())
                {
                    QueueAutoFitScaleToCurrentMonitor();
                }
            }
            finally
            {
                _isApplyingResponsiveLayout = false;
            }
        }

        private Rect GetCurrentMonitorWorkAreaDip()
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            Forms.Screen screen = handle == IntPtr.Zero ? Forms.Screen.PrimaryScreen : Forms.Screen.FromHandle(handle);
            System.Drawing.Rectangle bounds = screen.WorkingArea;

            Point topLeft = new Point(bounds.Left, bounds.Top);
            Point bottomRight = new Point(bounds.Right, bounds.Bottom);

            PresentationSource source = PresentationSource.FromVisual(this);
            if (source != null && source.CompositionTarget != null)
            {
                Matrix transform = source.CompositionTarget.TransformFromDevice;
                topLeft = transform.Transform(topLeft);
                bottomRight = transform.Transform(bottomRight);
            }

            return new Rect(topLeft, bottomRight);
        }

        private double GetUsableWindowWidth(Rect workArea)
        {
            double width = ActualWidth > 0 ? ActualWidth : Width;
            if (double.IsNaN(width) || width <= 0) width = workArea.Width;
            return Math.Max(320, width / Math.Max(0.5, currentDPIScale));
        }

        private double GetRawWindowWidth(Rect workArea)
        {
            double width = ActualWidth > 0 ? ActualWidth : Width;
            if (double.IsNaN(width) || width <= 0) width = workArea.Width;
            return Math.Max(320, width);
        }

        private double GetUsableMonitorWidth(Rect workArea)
        {
            return Math.Max(320, workArea.Width);
        }

        private bool IsPortrait(Rect workArea)
        {
            return workArea.Width < workArea.Height;
        }

        private void ApplyWindowBounds(Rect workArea, bool isMonitorPortrait)
        {
            double margin = 12;
            double targetMaxWidth = isMonitorPortrait ? Math.Min(760, workArea.Width - margin) : workArea.Width - margin;
            double targetMaxHeight = Math.Max(480, workArea.Height - margin);

            MinWidth = isMonitorPortrait ? 560 : 700;
            MaxWidth = Math.Max(MinWidth, targetMaxWidth);
            MaxHeight = targetMaxHeight;
        }

        private void ApplyOuterSpacing(bool isCompact)
        {
            Thickness outerMargin = isCompact ? new Thickness(8) : new Thickness(15);
            Thickness sectionMargin = isCompact ? new Thickness(0, 0, 0, 8) : new Thickness(0, 0, 19, 10);
            Thickness sectionPadding = isCompact ? new Thickness(8) : new Thickness(10);

            MainGrid.Margin = outerMargin;
            if (MainGrid.RowDefinitions.Count > 0)
            {
                MainGrid.RowDefinitions[0].Height = GridLength.Auto;
            }

            SetSectionChrome(HeaderBorder, sectionMargin, sectionPadding);
            SetSectionChrome(TabHostBorder, sectionMargin, sectionPadding);
            SetSectionChrome(ProgressBorder, sectionMargin, sectionPadding);

            if (ButtonsBorder != null)
            {
                ButtonsBorder.Margin = sectionMargin;
            }
        }

        private void SetSectionChrome(Border border, Thickness margin, Thickness padding)
        {
            if (border == null) return;
            border.Margin = margin;
            border.Padding = padding;
        }

        private void ApplyTabItemSizing(double monitorWidth, bool isMonitorPortrait, bool isCompact)
        {
            bool denseLandscape = !isMonitorPortrait && currentDPIScale >= 1.2;
            bool zoomedOutLandscape = !isMonitorPortrait && currentDPIScale < 1.0;
            double available = Math.Max(260, monitorWidth - (denseLandscape ? 54 : (isCompact ? 48 : 86)));
            int maxColumns = isMonitorPortrait ? 2 : 4;

            foreach (WrapPanel panel in GetInstallPanels())
            {
                if (panel == null) continue;

                if (ApplySparseWindowsPanelSizing(panel, monitorWidth, isCompact))
                {
                    continue;
                }

                bool longTextPanel = panel == WindowsPanel || panel == WindowsModPanel;
                double desiredItemWidth = longTextPanel ? (denseLandscape ? 270 : 300) : (denseLandscape ? 190 : (zoomedOutLandscape ? 245 : 205));
                int columns = Math.Max(1, Math.Min(maxColumns, (int)Math.Floor(available / desiredItemWidth)));

                double itemWidth = Math.Floor((available - ((columns - 1) * 6)) / columns);
                double minItemWidth = longTextPanel ? (denseLandscape ? 270 : 300) : (denseLandscape ? 190 : (zoomedOutLandscape ? 220 : 190));
                double maxItemWidth = longTextPanel ? 520 : (denseLandscape ? 226 : (zoomedOutLandscape ? 300 : 260));
                itemWidth = Math.Max(minItemWidth, Math.Min(maxItemWidth, itemWidth));

                double itemSlotWidth = itemWidth + (denseLandscape ? 3 : 6);
                panel.Orientation = Orientation.Horizontal;
                panel.ItemWidth = itemSlotWidth;
                panel.Width = Math.Ceiling(itemSlotWidth * columns);
                panel.HorizontalAlignment = zoomedOutLandscape || denseLandscape ? HorizontalAlignment.Center : HorizontalAlignment.Stretch;
                panel.Margin = isCompact || denseLandscape ? new Thickness(0) : new Thickness(0, 1, 0, 0);
            }
        }

        private IEnumerable<WrapPanel> GetInstallPanels()
        {
            yield return CheckBoxPanel;
            yield return OfficePanel;
            yield return SubtitlePanel;
            yield return MultimediaPanel;
            yield return SystemPanel;
            yield return PartitionPanel;
            yield return GamingPanel;
            yield return DriverPanel;
            yield return BrowserPanel;
            yield return RemoteDesktopPanel;
            yield return WindowsPanel;
            yield return WindowsModPanel;
        }

        private bool ApplySparseWindowsPanelSizing(WrapPanel panel, double monitorWidth, bool isCompact)
        {
            bool isWindowsTab = panel == WindowsPanel && IsSelectedTab("Windows - Microsoft");
            bool isWindowsModTab = panel == WindowsModPanel && IsSelectedTab("Windows Mod MMT");
            if (!isWindowsTab && !isWindowsModTab) return false;

            double scaledViewportWidth = monitorWidth / Math.Max(1.0, currentDPIScale);
            double available = Math.Max(260, scaledViewportWidth - (isCompact ? 52 : 96));
            int columns = isWindowsTab ? 1 : 2;
            double gap = 8;
            double itemSlotWidth = Math.Floor((available - ((columns - 1) * gap)) / columns);
            itemSlotWidth = Math.Max(260, Math.Min(isWindowsTab ? 620 : 560, itemSlotWidth));

            panel.Orientation = Orientation.Horizontal;
            panel.ItemWidth = itemSlotWidth;
            panel.Width = Math.Ceiling((itemSlotWidth * columns) + ((columns - 1) * gap));
            panel.HorizontalAlignment = HorizontalAlignment.Center;
            panel.Margin = new Thickness(0);

            SetSparseWindowsChildWidths(panel, itemSlotWidth);
            return true;
        }

        private void SetSparseWindowsChildWidths(WrapPanel panel, double itemSlotWidth)
        {
            double childWidth = Math.Max(180, itemSlotWidth - 10);

            if (panel == WindowsPanel)
            {
                if (ChkWin11_26H1 != null) ChkWin11_26H1.Width = childWidth;
                return;
            }

            if (panel == WindowsModPanel)
            {
                if (ChkWin10LtscIot21H2 != null) ChkWin10LtscIot21H2.Width = childWidth;
                if (ChkWin10_22H2_2024_December != null) ChkWin10_22H2_2024_December.Width = childWidth;
                if (ChkWintoHDD != null) ChkWintoHDD.Width = childWidth;
                if (BtnWinPEToHDD != null) BtnWinPEToHDD.Width = childWidth;
            }
        }

        private void ApplySparseTabSizing(bool isCompact)
        {
            bool isWindowsTab = IsSelectedTab("Windows - Microsoft");
            bool isWindowsModTab = IsSelectedTab("Windows Mod MMT");
            bool isSparseWindowsTab = isWindowsTab || isWindowsModTab;
            double panelMinHeight = isCompact ? 74 : 88;

            if (TabHostBorder != null)
            {
                TabHostBorder.MinHeight = isSparseWindowsTab ? (isCompact ? 130 : 150) : 0;
            }

            if (WindowsPanel != null)
            {
                WindowsPanel.MinHeight = isWindowsTab ? panelMinHeight : 0;
                WindowsPanel.VerticalAlignment = isWindowsTab ? VerticalAlignment.Center : VerticalAlignment.Top;
            }

            if (WindowsModPanel != null)
            {
                WindowsModPanel.MinHeight = isWindowsModTab ? panelMinHeight : 0;
                WindowsModPanel.VerticalAlignment = isWindowsModTab ? VerticalAlignment.Center : VerticalAlignment.Top;
            }
        }

        private void ApplyCommandSizing(double windowWidth, bool isCompact)
        {
            double available = Math.Max(260, windowWidth - (isCompact ? 42 : 82));

            if (CommandWrapPanel != null)
            {
                CommandWrapPanel.HorizontalAlignment = isCompact ? HorizontalAlignment.Left : HorizontalAlignment.Center;
                CommandWrapPanel.Margin = isCompact ? new Thickness(0, 0, 0, 8) : new Thickness(0, 0, 0, 10);
            }

            if (BtnDownloadPage != null)
            {
                BtnDownloadPage.Width = isCompact ? Math.Max(260, available) : 363;
                BtnDownloadPage.Height = isCompact ? 40 : 50;
                BtnDownloadPage.HorizontalAlignment = isCompact ? HorizontalAlignment.Stretch : HorizontalAlignment.Center;
            }
        }

        private void ApplyProgressSizing(bool isCompact)
        {
            if (DownloadProgressBar != null)
            {
                DownloadProgressBar.Height = isCompact ? 16 : 20;
            }

            if (ConnectionTraceBorder != null)
            {
                ConnectionTraceBorder.Height = isCompact ? 14 : 18;
                originalHeight = ConnectionTraceBorder.Height;
            }
        }

        private void UpdateSystemInformationChromeVisibility()
        {
            bool isSystemInformationTab = IsSystemInformationTabSelected();
            Visibility chromeVisibility = isSystemInformationTab ? Visibility.Collapsed : Visibility.Visible;

            if (ButtonsBorder != null) ButtonsBorder.Visibility = chromeVisibility;
            if (ProgressBorder != null) ProgressBorder.Visibility = chromeVisibility;
        }

        private bool IsSystemInformationTabSelected()
        {
            return IsSelectedTab("System Information");
        }

        private bool IsSelectedTab(string header)
        {
            try
            {
                if (MainTabControl != null && MainTabControl.SelectedItem is TabItem selectedTab)
                {
                    return selectedTab.Header != null &&
                           selectedTab.Header.ToString() == header;
                }
            }
            catch { }

            return false;
        }

        private bool ShouldSkipAutoFitScale()
        {
            if (IsSystemInformationTabSelected()) return true;
            return !(IsSelectedTab("Windows - Microsoft") || IsSelectedTab("Windows Mod MMT"));
        }

        private void KeepWindowInsideCurrentMonitor()
        {
            KeepWindowInsideCurrentMonitor(GetCurrentMonitorWorkAreaDip());
        }

        private void KeepWindowInsideCurrentMonitor(Rect workArea)
        {
            const double margin = 8.0;

            try
            {
                double maxAllowedHeight = Math.Max(0, workArea.Height - margin);
                double maxAllowedWidth = Math.Max(0, workArea.Width - margin);

                if (ActualHeight > maxAllowedHeight) Height = maxAllowedHeight;
                if (ActualWidth > maxAllowedWidth) Width = maxAllowedWidth;

                double currentWidth = !double.IsNaN(Width) && Width > 0 ? Width : ActualWidth;
                double currentHeight = !double.IsNaN(Height) && Height > 0 ? Height : ActualHeight;

                if (currentWidth <= 0) currentWidth = MinWidth;
                if (currentHeight <= 0) currentHeight = MinHeight;

                double minLeft = workArea.Left + margin;
                double minTop = workArea.Top + margin;
                double maxLeft = workArea.Right - margin - currentWidth;
                double maxTop = workArea.Bottom - margin - currentHeight;

                if (Left < minLeft) Left = minLeft;
                if (Top < minTop) Top = minTop;

                if (Left > maxLeft) Left = Math.Max(minLeft, maxLeft);
                if (Top > maxTop) Top = Math.Max(minTop, maxTop);
            }
            catch { }
        }

        private void QueueAutoFitScaleToCurrentMonitor(bool maximizeFirst = false)
        {
            if (!maximizeFirst && ShouldSkipAutoFitScale()) return;
            if (_isAutoFittingScale) return;

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                AutoFitScaleToCurrentMonitor(maximizeFirst);
            }));
        }

        private void AutoFitScaleToCurrentMonitor(bool maximizeFirst = false)
        {
            if (!maximizeFirst && ShouldSkipAutoFitScale()) return;
            if (_isAutoFittingScale || currentDPIScale <= 0.5) return;
            bool reducedScale = false;
            int originalIndex = GetClosestDpiStepIndex((int)Math.Round(currentDPIScale * 100.0));
            int targetIndex = originalIndex;

            try
            {
                _isAutoFittingScale = true;

                Rect workArea = GetCurrentMonitorWorkAreaDip();
                int startIndex = maximizeFirst ? DPI_STEPS.Length - 1 : originalIndex;

                if (!maximizeFirst && !IsCurrentScaleOverflowing(workArea)) return;

                for (int idx = startIndex; idx >= 0; idx--)
                {
                    if (idx != targetIndex)
                    {
                        SetCurrentDpiStepSilently(idx);
                        ApplyDPIScale();
                    }

                    MainGrid.UpdateLayout();
                    UpdateLayout();

                    if (!IsCurrentScaleOverflowing(workArea))
                    {
                        targetIndex = idx;
                        reducedScale = idx < startIndex;
                        break;
                    }
                }
            }
            finally
            {
                _isAutoFittingScale = false;
            }

            if (reducedScale)
            {
                UpdateSecondaryStatus($"Tự giảm DPI để hiển thị toàn bộ: {DPI_STEPS[targetIndex]}%", "Cyan");
                QueueAutoFitScaleToCurrentMonitor();
            }
        }

        private void QueueTabScaleFrom100ToBestFit(int requestId)
        {
            if (_isAutoFittingScale) return;

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                _ = FitSelectedTabStartingFrom100PercentAsync(requestId);
            }));
        }

        private async Task StartSelectedTabScaleWorkflowAsync(int requestId, CancellationToken token)
        {
            try
            {
                if (requestId != _tabScaleFitRequestId) return;

                _suppressResponsiveAutoFitQueue = true;
                _suppressPrimaryDpiStatus = true;

                ApplyResponsiveLayout();
                MainGrid.UpdateLayout();
                UpdateSystemInformationChromeVisibility();
                ResetCurrentTabDpiTo100Percent();
                _hasCompletedInitialTabScaleFit = true;

                if (IsSystemInformationTabSelected())
                {
                    return;
                }

                await Task.Delay(TabScaleFitDelayMs, token).ConfigureAwait(true);
                if (token.IsCancellationRequested || requestId != _tabScaleFitRequestId) return;

                QueueTabScaleFrom100ToBestFit(requestId);
            }
            catch (TaskCanceledException)
            {
            }
            finally
            {
                if (requestId == _tabScaleFitRequestId && IsSystemInformationTabSelected())
                {
                    _suppressPrimaryDpiStatus = false;
                    _suppressResponsiveAutoFitQueue = false;
                }
            }
        }

        private async Task FitSelectedTabStartingFrom100PercentAsync(int requestId)
        {
            if (_isAutoFittingScale || currentDPIScale <= 0.5) return;
            if (requestId != _tabScaleFitRequestId) return;

            bool changedScale = false;
            bool reducedScale = false;
            int baseIndex = Array.IndexOf(DPI_STEPS, 100);
            if (baseIndex < 0) baseIndex = GetClosestDpiStepIndexForTabFit(100);
            int appliedIndex = baseIndex;
            int targetIndex = baseIndex;
            Rect workArea = GetCurrentMonitorWorkAreaDip();

            _isAutoFittingScale = true;
            _suppressPrimaryDpiStatus = true;
            _suppressResponsiveAutoFitQueue = true;

            try
            {
                currentDPIScale = DPI_STEPS[baseIndex] / 100.0;
                SetDPIComboIndexSilently(baseIndex);
                ApplyDPIScale();
                MainGrid.UpdateLayout();
                UpdateLayout();
                changedScale = true;

                await Task.Yield();
                await Task.Delay(TabScaleFitStepDelayMs);

                if (requestId != _tabScaleFitRequestId) return;

                for (int idx = baseIndex + 1; idx < DPI_STEPS.Length; idx++)
                {
                    if (requestId != _tabScaleFitRequestId) return;

                    currentDPIScale = DPI_STEPS[idx] / 100.0;
                    SetDPIComboIndexSilently(idx);
                    ApplyDPIScale();
                    MainGrid.UpdateLayout();
                    UpdateLayout();
                    appliedIndex = idx;
                    changedScale = true;

                    await Task.Yield();
                    await Task.Delay(TabScaleFitStepDelayMs);

                    if (requestId != _tabScaleFitRequestId) return;

                    if (!IsCurrentScaleOverflowingForTabFit(workArea))
                    {
                        targetIndex = idx;
                        continue;
                    }

                    targetIndex = idx - 1;
                    reducedScale = true;
                    break;
                }

                if (targetIndex != appliedIndex)
                {
                    if (requestId != _tabScaleFitRequestId) return;

                    currentDPIScale = DPI_STEPS[targetIndex] / 100.0;
                    SetDPIComboIndexSilently(targetIndex);
                    ApplyDPIScale();
                    changedScale = true;
                }

                if (changedScale && reducedScale)
                {
                    UpdateSecondaryStatus($"Tự giảm DPI để hiển thị toàn bộ: {DPI_STEPS[targetIndex]}%", "Cyan");
                }
            }
            finally
            {
                _suppressPrimaryDpiStatus = false;
                _suppressResponsiveAutoFitQueue = false;
                _isAutoFittingScale = false;
            }
        }

        private void ResetCurrentTabDpiTo100Percent()
        {
            int baseIndex = Array.IndexOf(DPI_STEPS, 100);
            if (baseIndex < 0) baseIndex = GetClosestDpiStepIndexForTabFit(100);

            currentDPIScale = DPI_STEPS[baseIndex] / 100.0;
            SetDPIComboIndexSilently(baseIndex);
            ApplyDPIScale();
            MainGrid.UpdateLayout();
            UpdateLayout();
        }

        private int GetClosestDpiStepIndexForTabFit(int percent)
        {
            int closest = 0;
            int minDiff = int.MaxValue;

            for (int i = 0; i < DPI_STEPS.Length; i++)
            {
                int diff = Math.Abs(DPI_STEPS[i] - percent);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    closest = i;
                }
            }

            return closest;
        }

        private bool IsCurrentScaleOverflowingForTabFit(Rect workArea)
        {
            if (IsSelectedTab("Windows - Microsoft") || IsSelectedTab("Windows Mod MMT"))
            {
                return HasSparseWindowsTabOverflow();
            }

            if (IsSystemInformationTabSelected()) return false;

            ScrollViewer selectedScrollViewer = GetSelectedTabScrollViewer();
            if (selectedScrollViewer != null && HasSelectedTabContentOverflow(selectedScrollViewer))
            {
                return true;
            }

            const double margin = 10.0;
            double maxAllowedWidth = Math.Max(0, workArea.Width - margin);
            double maxAllowedHeight = Math.Max(0, workArea.Height - margin);

            double desiredWidth = Math.Max(ActualWidth, DesiredSize.Width);
            double desiredHeight = Math.Max(ActualHeight, DesiredSize.Height);
            bool tooWide = desiredWidth > maxAllowedWidth + 1;
            bool tooTall = desiredHeight > maxAllowedHeight + 1;

            return tooWide || tooTall || HasSparseWindowsTabOverflow();
        }

        private int GetClosestDpiStepIndex(int percent)
        {
            return GetClosestDpiStepIndexForTabFit(percent);
        }

        private bool IsCurrentScaleOverflowing(Rect workArea)
        {
            return IsCurrentScaleOverflowingForTabFit(workArea);
        }

        private void SetCurrentDpiStepSilently(int index)
        {
            currentDPIScale = DPI_STEPS[Math.Max(0, Math.Min(DPI_STEPS.Length - 1, index))] / 100.0;
            SetDPIComboIndexSilently(Math.Max(0, Math.Min(DPI_STEPS.Length - 1, index)));
        }

        private void SetDPIComboIndexSilently(int index)
        {
            try
            {
                if (CboDPIValue == null || index < 0 || index >= CboDPIValue.Items.Count) return;
                _isUpdatingDpiSelection = true;
                CboDPIValue.SelectedIndex = index;
            }
            catch { }
            finally
            {
                _isUpdatingDpiSelection = false;
            }
        }

        private bool HasClippedScrollViewerContent()
        {
            foreach (ScrollViewer scrollViewer in FindVisualChildren<ScrollViewer>(MainTabControl))
            {
                if (!scrollViewer.IsVisible) continue;
                if (scrollViewer.ActualWidth <= 0 || scrollViewer.ActualHeight <= 0) continue;

                FrameworkElement content = scrollViewer.Content as FrameworkElement;
                if (content == null) continue;

                if (IsElementClippedByScrollViewer(content, scrollViewer))
                {
                    return true;
                }

                foreach (FrameworkElement element in FindVisualChildren<FrameworkElement>(content))
                {
                    if (!element.IsVisible || element.ActualWidth <= 0 || element.ActualHeight <= 0) continue;
                    if (IsElementClippedByScrollViewer(element, scrollViewer))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool HasSparseWindowsTabOverflow()
        {
            if (!IsSelectedTab("Windows - Microsoft") && !IsSelectedTab("Windows Mod MMT")) return false;

            try
            {
                if (TabHostBorder == null || ButtonsBorder == null || MainGrid == null) return false;
                if (TabHostBorder.ActualWidth <= 0 || TabHostBorder.ActualHeight <= 0) return false;
                if (ButtonsBorder.ActualWidth <= 0 || ButtonsBorder.ActualHeight <= 0) return false;

                Rect tabBounds = TabHostBorder.TransformToAncestor(MainGrid)
                                              .TransformBounds(new Rect(0, 0, TabHostBorder.ActualWidth, TabHostBorder.ActualHeight));
                Rect buttonsBounds = ButtonsBorder.TransformToAncestor(MainGrid)
                                                  .TransformBounds(new Rect(0, 0, ButtonsBorder.ActualWidth, ButtonsBorder.ActualHeight));

                if (tabBounds.Bottom > buttonsBounds.Top - 2)
                {
                    return true;
                }

                WrapPanel selectedPanel = IsSelectedTab("Windows - Microsoft") ? WindowsPanel : WindowsModPanel;
                if (selectedPanel == null || selectedPanel.ActualWidth <= 0 || selectedPanel.ActualHeight <= 0) return false;

                Rect panelBounds = selectedPanel.TransformToAncestor(TabHostBorder)
                                                .TransformBounds(new Rect(0, 0, selectedPanel.ActualWidth, selectedPanel.ActualHeight));
                double leftLimit = Math.Max(0, TabHostBorder.Padding.Left);
                double rightLimit = Math.Max(0, TabHostBorder.ActualWidth - TabHostBorder.Padding.Right);
                double bottomLimit = Math.Max(0, TabHostBorder.ActualHeight - TabHostBorder.Padding.Bottom);

                return panelBounds.Left < leftLimit - 2 ||
                       panelBounds.Right > rightLimit + 2 ||
                       panelBounds.Bottom > bottomLimit + 2;
            }
            catch
            {
                return false;
            }
        }

        private ScrollViewer GetSelectedTabScrollViewer()
        {
            try
            {
                if (MainTabControl?.SelectedItem is TabItem selectedTab)
                {
                    return selectedTab.Content as ScrollViewer;
                }
            }
            catch
            {
            }

            return null;
        }

        private bool HasSelectedTabContentOverflow(ScrollViewer scrollViewer)
        {
            const double tolerance = 1.0;
            if (scrollViewer == null) return false;
            if (scrollViewer.ActualWidth <= 0 || scrollViewer.ActualHeight <= 0) return false;
            if (!(scrollViewer.Content is WrapPanel panel)) return false;

            double maxRight = 0;
            double maxBottom = 0;
            bool foundChild = false;

            foreach (FrameworkElement child in panel.Children.OfType<FrameworkElement>())
            {
                if (!child.IsVisible || child.ActualWidth <= 0 || child.ActualHeight <= 0) continue;

                Rect bounds;
                try
                {
                    bounds = child.TransformToAncestor(scrollViewer)
                                  .TransformBounds(new Rect(0, 0, child.ActualWidth, child.ActualHeight));
                }
                catch
                {
                    continue;
                }

                maxRight = Math.Max(maxRight, bounds.Right);
                maxBottom = Math.Max(maxBottom, bounds.Bottom);
                foundChild = true;
            }

            if (!foundChild) return false;

            return maxRight > scrollViewer.ActualWidth + tolerance ||
                   maxBottom > scrollViewer.ActualHeight + tolerance;
        }

        private bool IsElementClippedByScrollViewer(FrameworkElement element, ScrollViewer scrollViewer)
        {
            const double tolerance = 1.0;

            try
            {
                Rect bounds = element.TransformToAncestor(scrollViewer)
                                     .TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));

                return bounds.Left < -tolerance ||
                       bounds.Top < -tolerance ||
                       bounds.Right > scrollViewer.ActualWidth + tolerance ||
                       bounds.Bottom > scrollViewer.ActualHeight + tolerance;
            }
            catch
            {
                return false;
            }
        }

        private IEnumerable<T> FindVisualChildren<T>(DependencyObject root) where T : DependencyObject
        {
            if (root == null) yield break;

            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(root, i);
                if (child is T typedChild)
                {
                    yield return typedChild;
                }

                foreach (T descendant in FindVisualChildren<T>(child))
                {
                    yield return descendant;
                }
            }
        }
    }
}

