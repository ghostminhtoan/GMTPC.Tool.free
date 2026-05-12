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

/*
 * AI Summary:
 * Date: 2026-03-09
 * - Added ChkTeraCopy, ChkDeactivateWindows, and ChkVPN1111 references
 * - 2026-05-12: Added ChkDeactivateWindows status handler for built-in DLV -> UPK flow
 */

namespace GMTPC.Tool
{
    public partial class MainWindow
    {
        private void ChkTeraCopy_Click(object sender, RoutedEventArgs e)
        {
            if (ChkTeraCopy.IsChecked == true)
            {
                UpdateStatus("Đã chọn: TeraCopy", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: TeraCopy", "Yellow");
            }

            UpdateInstallButtonState();
        }

        private void ChkDeactivateWindows_Click(object sender, RoutedEventArgs e)
        {
            if (ChkDeactivateWindows.IsChecked == true)
            {
                UpdateStatus("Đã chọn: Gỡ kích hoạt Windows", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: Gỡ kích hoạt Windows", "Yellow");
            }

            UpdateInstallButtonState();
        }

        private void ChkVPN1111_Click(object sender, RoutedEventArgs e)
        {
            if (ChkVPN1111.IsChecked == true)
            {
                UpdateStatus("Đã chọn: VPN 1111 (Cloudflare)", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: VPN 1111 (Cloudflare)", "Yellow");
            }

            UpdateInstallButtonState();
        }

        private void ChkMemReduct_Click(object sender, RoutedEventArgs e)
        {
            if (ChkMemReduct.IsChecked == true)
            {
                UpdateStatus("Đã chọn: MemReduct", "Green");
            }
            else
            {
                UpdateStatus("Đã hủy chọn: MemReduct", "Yellow");
            }

            UpdateInstallButtonState();
        }
    }
}
