// AI Summary: 2026-04-23 - Fixed blank-line rendering between CPU sockets and RAM slots in System Information.
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
using System.Text;

namespace GMTPC.Tool
{
    public partial class MainWindow
    {
        private void PopulateSystemInfo()
        {
            try
            {
                TbGPU.Text = BuildGpuInfo();
                TbCPU.Text = BuildCpuInfo();
                TbRAM.Text = BuildRamInfo();
                TbMainboard.Text = BuildMainboardInfo();
            }
            catch (Exception ex)
            {
                // don't crash UI
                try { TbMainboard.Text = "Error: " + ex.Message; } catch { }
            }
        }

        private string BuildGpuInfo()
        {
            StringBuilder sb = new StringBuilder();
            int index = 1;

            foreach (ManagementObject gpu in QueryWmi("Win32_VideoController"))
            {
                AppendLine(sb, $"=== GPU {index} ===");
                AppendLine(sb, "Tên", GetValue(gpu, "Name"));
                AppendLine(sb, "Driver Version", GetValue(gpu, "DriverVersion"));
                AppendLine(sb, "VRAM", FormatBytes(GetUlongValue(gpu, "AdapterRAM")));
                AppendLine(sb, "Video Processor", GetValue(gpu, "VideoProcessor"));
                AppendLine(sb, "Độ phân giải", FormatResolution(gpu));
                AppendLine(sb, "Refresh Rate", AppendUnit(GetValue(gpu, "CurrentRefreshRate"), "Hz"));
                AppendLine(sb, "Bit Depth", AppendUnit(GetValue(gpu, "CurrentBitsPerPixel"), "bit"));
                AppendLine(sb, "Trạng thái", GetValue(gpu, "Status"));
                AppendLine(sb, "PNP Device ID", GetValue(gpu, "PNPDeviceID"));
                index++;
            }

            return sb.Length > 0 ? sb.ToString().TrimEnd() : "Unknown";
        }

        private string BuildCpuInfo()
        {
            StringBuilder sb = new StringBuilder();
            int index = 1;

            foreach (ManagementObject cpu in QueryWmi("Win32_Processor"))
            {
                AppendLine(sb, $"=== CPU {index} ===");
                AppendLine(sb, "Tên", GetValue(cpu, "Name"));
                AppendLine(sb, "Hãng sản xuất", GetValue(cpu, "Manufacturer"));
                AppendLine(sb, "Kiến trúc", GetArchitectureName(GetValue(cpu, "Architecture")));
                AppendLine(sb, "Socket", GetValue(cpu, "SocketDesignation"));
                AppendLine(sb, "Số nhân", GetValue(cpu, "NumberOfCores"));
                AppendLine(sb, "Số luồng", GetValue(cpu, "NumberOfLogicalProcessors"));
                AppendLine(sb, "Xung nhịp hiện tại", AppendUnit(GetValue(cpu, "CurrentClockSpeed"), "MHz"));
                AppendLine(sb, "Xung nhịp tối đa", AppendUnit(GetValue(cpu, "MaxClockSpeed"), "MHz"));
                AppendLine(sb, "Cache L2", AppendUnit(GetValue(cpu, "L2CacheSize"), "KB"));
                AppendLine(sb, "Cache L3", AppendUnit(GetValue(cpu, "L3CacheSize"), "KB"));
                AppendLine(sb, "Virtualization", GetValue(cpu, "VirtualizationFirmwareEnabled"));
                AppendLine(sb, "Processor ID", GetValue(cpu, "ProcessorId"));
                AppendLine(sb, "");
                index++;
            }

            return sb.Length > 0 ? sb.ToString().TrimEnd() : "Unknown";
        }

        private string BuildRamInfo()
        {
            StringBuilder sb = new StringBuilder();
            ulong totalRamBytes = 0;
            int slot = 1;

            foreach (ManagementObject ram in QueryWmi("Win32_PhysicalMemory"))
            {
                ulong capacity = GetUlongValue(ram, "Capacity");
                totalRamBytes += capacity;

                AppendLine(sb, $"=== RAM Slot {slot} ===");
                AppendLine(sb, "Dung lượng", FormatBytes(capacity));
                AppendLine(sb, "Hãng sản xuất", GetValue(ram, "Manufacturer"));
                AppendLine(sb, "Part Number", GetValue(ram, "PartNumber"));
                AppendLine(sb, "Serial", GetValue(ram, "SerialNumber"));
                AppendLine(sb, "Bank", GetValue(ram, "BankLabel"));
                AppendLine(sb, "Vị trí", GetValue(ram, "DeviceLocator"));
                AppendLine(sb, "Tốc độ", AppendUnit(GetValue(ram, "Speed"), "MHz"));
                AppendLine(sb, "Configured Speed", AppendUnit(GetValue(ram, "ConfiguredClockSpeed"), "MHz"));
                AppendLine(sb, "Form Factor", GetMemoryFormFactor(GetValue(ram, "FormFactor")));
                AppendLine(sb, "");
                AppendLine(sb, "");
                slot++;
            }

            if (totalRamBytes > 0)
            {
                sb.Insert(0, $"Tổng RAM vật lý: {FormatBytes(totalRamBytes)}{Environment.NewLine}{Environment.NewLine}");
            }

            return sb.Length > 0 ? sb.ToString().TrimEnd() : "Unknown";
        }

        private string BuildMainboardInfo()
        {
            StringBuilder sb = new StringBuilder();
            ManagementObject board = FirstOrDefault(QueryWmi("Win32_BaseBoard"));
            ManagementObject bios = FirstOrDefault(QueryWmi("Win32_BIOS"));
            ManagementObject system = FirstOrDefault(QueryWmi("Win32_ComputerSystem"));

            AppendLine(sb, "Hãng sản xuất", GetValue(board, "Manufacturer"));
            AppendLine(sb, "Model", GetValue(board, "Product"));
            AppendLine(sb, "Serial", GetValue(board, "SerialNumber"));
            AppendLine(sb, "Version", GetValue(board, "Version"));
            AppendLine(sb, "Trạng thái", GetValue(board, "Status"));
            AppendLine(sb, "");
            AppendLine(sb, "BIOS", GetValue(bios, "Manufacturer"));
            AppendLine(sb, "BIOS Version", GetValue(bios, "SMBIOSBIOSVersion"));
            AppendLine(sb, "Ngày phát hành", FormatWmiDate(GetValue(bios, "ReleaseDate")));
            AppendLine(sb, "");
            AppendLine(sb, "System", GetValue(system, "Manufacturer"));
            AppendLine(sb, "System Model", GetValue(system, "Model"));
            AppendLine(sb, "System Type", GetValue(system, "SystemType"));

            return sb.Length > 0 ? sb.ToString().TrimEnd() : "Unknown";
        }

        private IEnumerable<ManagementObject> QueryWmi(string wmiClass)
        {
            List<ManagementObject> results = new List<ManagementObject>();

            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT * FROM {wmiClass}"))
                {
                    foreach (ManagementObject item in searcher.Get())
                    {
                        results.Add(item);
                    }
                }
            }
            catch { }

            return results;
        }

        private ManagementObject FirstOrDefault(IEnumerable<ManagementObject> items)
        {
            foreach (ManagementObject item in items)
            {
                return item;
            }

            return null;
        }

        private string GetValue(ManagementObject item, string propertyName)
        {
            try
            {
                if (item == null || item[propertyName] == null) return "Unknown";
                string value = item[propertyName].ToString().Trim();
                return string.IsNullOrWhiteSpace(value) ? "Unknown" : value;
            }
            catch
            {
                return "Unknown";
            }
        }

        private ulong GetUlongValue(ManagementObject item, string propertyName)
        {
            try
            {
                if (item == null || item[propertyName] == null) return 0;
                return Convert.ToUInt64(item[propertyName]);
            }
            catch
            {
                return 0;
            }
        }

        private void AppendLine(StringBuilder sb, string text)
        {
            if (text == null) return;
            sb.AppendLine(text);
        }

        private void AppendLine(StringBuilder sb, string label, string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "Unknown") return;
            sb.AppendLine($"{label}: {value}");
        }

        private string AppendUnit(string value, string unit)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "Unknown") return "Unknown";
            return $"{value} {unit}";
        }

        private string FormatResolution(ManagementObject gpu)
        {
            string width = GetValue(gpu, "CurrentHorizontalResolution");
            string height = GetValue(gpu, "CurrentVerticalResolution");
            if (width == "Unknown" || height == "Unknown") return "Unknown";
            return $"{width} x {height}";
        }

        private string FormatWmiDate(string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value) || value == "Unknown") return "Unknown";
                DateTime date = ManagementDateTimeConverter.ToDateTime(value);
                return date.ToString("dd/MM/yyyy");
            }
            catch
            {
                return "Unknown";
            }
        }

        private string GetArchitectureName(string value)
        {
            switch (value)
            {
                case "0": return "x86";
                case "1": return "MIPS";
                case "2": return "Alpha";
                case "3": return "PowerPC";
                case "5": return "ARM";
                case "6": return "Itanium";
                case "9": return "x64";
                case "12": return "ARM64";
                default: return value;
            }
        }

        private string GetMemoryFormFactor(string value)
        {
            switch (value)
            {
                case "8": return "DIMM";
                case "12": return "SODIMM";
                case "13": return "SRIMM";
                default: return value;
            }
        }

        private string GetDirectXVersion()
        {
            try
            {
                // Most modern Windows (10/11) strictly use DirectX 12
                // We can also check the registry for a broad version string
                string versionStr = "";
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\DirectX"))
                {
                    versionStr = key?.GetValue("Version") as string ?? "";
                }

                // Map common internal version strings to friendly names
                if (versionStr.StartsWith("4.09.00.0904"))
                {
                    // This is the common string for DirectX 9.0c, but also often remains on Win10/11
                    // Detection by OS version is more reliable for modern DX
                    if (IsWindows10Or11()) return "DirectX 12";
                    return "DirectX 9.0c";
                }

                // Detailed mapping if needed
                switch (versionStr)
                {
                    case "4.09.00.0904": return "DirectX 9.0c";
                    case "4.09.00.0902": return "DirectX 9.0b";
                    case "4.09.00.0900": return "DirectX 9.0";
                    case "4.08.01.0881": return "DirectX 8.1";
                    case "4.08.00.0400": return "DirectX 8.0";
                    case "4.07.00.0700": return "DirectX 7.0";
                }

                if (IsWindows10Or11()) return "DirectX 12";
                if (IsWindows8Or81()) return "DirectX 11.1/11.2";
                if (IsWindows7()) return "DirectX 11";

                return !string.IsNullOrEmpty(versionStr) ? $"DirectX {versionStr}" : "Unknown";
            }
            catch { return "Unknown"; }
        }

        private bool IsWindows10Or11()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    string buildStr = key?.GetValue("CurrentBuild")?.ToString() ?? "";
                    if (int.TryParse(buildStr, out int build))
                    {
                        return build >= 10240; // Windows 10 build 10240 is the first release
                    }
                }
            }
            catch { }
            return Environment.OSVersion.Version.Major >= 10;
        }

        private bool IsWindows8Or81()
        {
            return Environment.OSVersion.Version.Major == 6 && (Environment.OSVersion.Version.Minor == 2 || Environment.OSVersion.Version.Minor == 3);
        }

        private bool IsWindows7()
        {
            return Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1;
        }

        private string GetWmiSingleValue(string wmiClass, string property)
        {
            try
            {
                var searcher = new ManagementObjectSearcher($"select {property} from {wmiClass}");
                foreach (ManagementObject mo in searcher.Get())
                {
                    if (mo[property] != null)
                    {
                        return mo[property].ToString();
                    }
                }
            }
            catch { }
            return null;
        }

        private string FormatBytes(ulong bytes)
        {
            double gb = bytes / (1024.0 * 1024.0 * 1024.0);
            return $"{gb:F2} GB";
        }

        private Button _btnRestartBios;

        private void InitRestartBiosButton()
        {
            _btnRestartBios = new Button
            {
                Content = "Restart to BIOS",
                Width = 120,
                Height = 24,
                Background = Brushes.DarkRed,
                Foreground = Brushes.White,
                Margin = new Thickness(20, 0, 0, 0)
            };
            _btnRestartBios.Click += BtnRestartBios_Click;
        }

        public void SetupRestartBiosButton(StackPanel hardwareHeaderPanel)
        {
            if (_btnRestartBios == null) InitRestartBiosButton();
            hardwareHeaderPanel.Orientation = Orientation.Horizontal;
            hardwareHeaderPanel.Children.Add(_btnRestartBios);
        }

        private void BtnRestartBios_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Restart into BIOS! Yes or No?",
                "Restart to BIOS",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "shutdown",
                        Arguments = "/r /fw /t 0",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    Process.Start(psi);
                }
                catch { }
            }
        }
    }
}
