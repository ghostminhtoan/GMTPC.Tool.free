// AI Summary: 2026-04-22 - Added tab-change auto-fit behavior;
// switching from a sparse tab to a denser tab now rechecks clipping and steps DPI down automatically.
// =======================================================================
// MainWindow.ResponsiveLayout.cs
// Chuc nang: Desktop responsive layout cho man hinh ngang/doc va multi-monitor.
//            Khong chua logic cai dat, chi dieu chinh kich thuoc va mat do UI.
// =======================================================================
using System;
using System.Collections.Generic;
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

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
            {
                ApplyResponsiveLayout();
                MainGrid.UpdateLayout();
                QueueAutoFitScaleToCurrentMonitor();
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
                ApplyCommandSizing(windowWidth, isCompact);
                ApplyProgressSizing(isCompact);
                KeepWindowInsideCurrentMonitor(workArea);
                QueueAutoFitScaleToCurrentMonitor();
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
            double available = Math.Max(260, monitorWidth - (denseLandscape ? 54 : (isCompact ? 48 : 86)));
            int maxColumns = isMonitorPortrait ? 2 : 4;

            foreach (WrapPanel panel in GetInstallPanels())
            {
                if (panel == null) continue;

                bool longTextPanel = panel == WindowsPanel || panel == WindowsModPanel;
                double desiredItemWidth = longTextPanel ? (denseLandscape ? 270 : 300) : (denseLandscape ? 190 : 205);
                int columns = Math.Max(1, Math.Min(maxColumns, (int)Math.Floor(available / desiredItemWidth)));

                double itemWidth = Math.Floor((available - ((columns - 1) * 6)) / columns);
                double minItemWidth = longTextPanel ? (denseLandscape ? 270 : 300) : (denseLandscape ? 190 : 190);
                double maxItemWidth = longTextPanel ? 520 : (denseLandscape ? 226 : 260);
                itemWidth = Math.Max(minItemWidth, Math.Min(maxItemWidth, itemWidth));

                panel.Orientation = Orientation.Horizontal;
                panel.ItemWidth = itemWidth + (denseLandscape ? 3 : 6);
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

        private void QueueAutoFitScaleToCurrentMonitor()
        {
            if (_isAutoFittingScale) return;

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                AutoFitScaleToCurrentMonitor();
            }));
        }

        private void AutoFitScaleToCurrentMonitor()
        {
            if (_isAutoFittingScale || currentDPIScale <= 0.5) return;
            bool reducedScale = false;

            try
            {
                _isAutoFittingScale = true;

                Rect workArea = GetCurrentMonitorWorkAreaDip();
                const double margin = 10.0;
                double maxAllowedWidth = Math.Max(0, workArea.Width - margin);
                double maxAllowedHeight = Math.Max(0, workArea.Height - margin);

                double desiredWidth = Math.Max(ActualWidth, DesiredSize.Width);
                double desiredHeight = Math.Max(ActualHeight, DesiredSize.Height);
                bool tooWide = desiredWidth > maxAllowedWidth + 1;
                bool tooTall = desiredHeight > maxAllowedHeight + 1 || ActualHeight >= maxAllowedHeight - 1;
                bool clippedContent = HasClippedScrollViewerContent();

                if (!tooWide && !tooTall && !clippedContent) return;

                int currentPercent = (int)Math.Round(currentDPIScale * 100.0);
                int idx = Array.IndexOf(DPI_STEPS, currentPercent);
                if (idx <= 0) return;

                currentDPIScale = DPI_STEPS[idx - 1] / 100.0;
                SetDPIComboIndexSilently(idx - 1);
                ApplyDPIScale();
                UpdateSecondaryStatus($"Tự giảm DPI để hiển thị toàn bộ: {DPI_STEPS[idx - 1]}%", "Cyan");
                reducedScale = true;
            }
            finally
            {
                _isAutoFittingScale = false;
            }

            if (reducedScale)
            {
                QueueAutoFitScaleToCurrentMonitor();
            }
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
