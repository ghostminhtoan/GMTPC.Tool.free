using System;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GMTPC.Tool
{
    public partial class MainWindow
    {
        // =======================================================================
        // Parent tab DPI scaling
        // Any top-level tab should route here so the window resets to 100% DPI
        // and then fits itself back to the largest visible scale.
        // =======================================================================
        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!ReferenceEquals(sender, MainTabControl)) return;
            QueueParentTabDpiWorkflow();
        }

        private void QueueParentTabDpiWorkflow()
        {
            QueueSelectedTabScaleWorkflow();
        }

        private void QueueSelectedTabScaleWorkflow()
        {
            int requestId = ++_tabScaleFitRequestId;
            _suppressResponsiveAutoFitQueue = true;
            CancellationTokenSource previousCts = Interlocked.Exchange(ref _tabScaleFitDelayCts, new CancellationTokenSource());
            if (previousCts != null)
            {
                try { previousCts.Cancel(); } catch { }
                previousCts.Dispose();
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                if (requestId != _tabScaleFitRequestId) return;
                _ = StartSelectedTabScaleWorkflowAsync(requestId, _tabScaleFitDelayCts.Token);
            }));
        }

        private void ScrollSelectedTabToTop()
        {
            try
            {
                ScrollViewer selectedScrollViewer = GetSelectedTabScrollViewer();
                if (selectedScrollViewer != null)
                {
                    selectedScrollViewer.ScrollToTop();
                    selectedScrollViewer.ScrollToLeftEnd();
                }
            }
            catch
            {
            }
        }

        // =======================================================================
        // Child tab DPI scaling
        // Any nested tab should route here so the active child resets to 100% DPI,
        // scrolls back to the top, and then fits itself just like a parent tab.
        // =======================================================================
        private void WindowsTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!ReferenceEquals(sender, WindowsTabControl)) return;
            QueueChildTabDpiWorkflow();
        }

        private void QueueChildTabDpiWorkflow()
        {
            QueueSelectedTabScaleWorkflow();

            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(ScrollSelectedTabToTop));
        }
    }
}
