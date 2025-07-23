using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Security.Principal;

namespace WindowInsiderTool
{
    public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            CheckForAdmin();
            UpdateBypassStatus();
            UpdateInsiderStatus();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void GitHubButton_Click(object sender, RoutedEventArgs e)
        {
            var url = "https://github.com/phalchanouksa/Windows-insider-requirements-bypass-script";
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open link: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CheckForAdmin()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    MessageBox.Show("This application requires administrator privileges to function correctly. Please restart it as an administrator.", "Administrator Required", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                }
            }
        }

        // =================================================================
        // ==                 UPGRADE BYPASS SECTION                    ==
        // =================================================================
        #region Upgrade Bypass
        private const string IFEO_KEY_PATH = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\SetupHost.exe";
        private const string SCRIPT_DIR = @"C:\Scripts";
        private const string SCRIPT_PATH = @"C:\Scripts\W11Bypass.cmd";
        private const string BYPASS_SCRIPT_CONTENT = @"
@echo off
if /i ""%~f0"" neq ""%SystemDrive%\Scripts\get11.cmd"" if ""%~1""=="" powershell -win 1 -nop -c "";""
powershell -win 1 -nop -c "";""
set CLI=%*& set SOURCES=%SystemDrive%\$WINDOWS.~BT\Sources& set MEDIA=.& set MOD=CLI& set PRE=WUA& set /a VER=11
if not defined CLI (exit /b) else if not exist %SOURCES%\SetupHost.exe (exit /b)
if not exist %SOURCES%\WindowsUpdateBox.exe mklink /h %SOURCES%\WindowsUpdateBox.exe %SOURCES%\SetupHost.exe
reg add HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate /f /v DisableWUfBSafeguards /d 1 /t reg_dword
reg add HKLM\SYSTEM\Setup\MoSetup /f /v AllowUpgradesWithUnsupportedTPMorCPU /d 1 /t reg_dword
set OPT=/Compat IgnoreWarning /MigrateDrivers All /Telemetry Disable
set /a restart_application=0x800705BB & (call set CLI=%%CLI:%1 =%%)
set /a incorrect_parameter=0x80070057 & (set SRV=%CLI:/Product Client =%)
set /a launch_option_error=0xc190010a & (set SRV=%SRV:/Product Server =%)
for %%W in (%CLI%) do if /i %%W == /PreDownload (set MOD=SRV)
for %%W in (%CLI%) do if /i %%W == /InstallFile (set PRE=ISO& set ""MEDIA="") else if not defined MEDIA set ""MEDIA=%%~dpW""
if %VER% == 11 for %%W in (""%MEDIA%appraiserres.dll"") do if exist %%W if %%~zW == 0 set AlreadyPatched=1 & set /a VER=10
if %VER% == 11 findstr /r ""P.r.o.d.u.c.t.V.e.r.s.i.o.n...1.0.\..0.\..2.[2-9]"" %SOURCES%\SetupHost.exe >nul 2>nul || set /a VER=10
if %VER% == 11 if not exist ""%MEDIA%EI.cfg"" (echo;[Channel]>%SOURCES%\EI.cfg & echo;_Default>>%SOURCES%\EI.cfg)
if %VER%_%PRE% == 11_ISO (%SOURCES%\WindowsUpdateBox.exe /Product Server /PreDownload /Quiet %OPT%)
if %VER%_%PRE% == 11_ISO (del /f /q %SOURCES%\appraiserres.dll 2>nul & cd.>%SOURCES%\appraiserres.dll & call :canary)
if %VER%_%MOD% == 11_SRV (set ARG=%OPT% %SRV% /Product Server)
if %VER%_%MOD% == 11_CLI (set ARG=%OPT% %CLI%)
%SOURCES%\WindowsUpdateBox.exe %ARG%
if %errorlevel% == %restart_application% (call :canary & %SOURCES%\WindowsUpdateBox.exe %ARG%)
exit /b
:canary
set C=  $X='%SOURCES%\hwreqchk.dll'; $Y='SQ_TpmVersion GTE 1'; $Z='SQ_TpmVersion GTE 0'; if (test-path $X) { 
set C=%C%  try { takeown.exe /f $X /a; icacls.exe $X /grant *S-1-5-32-544:f; attrib -R -S $X; [io.file]::OpenWrite($X).close() }
set C=%C%  catch { return }; $R=[Text.Encoding]::UTF8.GetBytes($Z); $l=$R.Length; $i=2; $w=!1; 
set C=%C%  $B=[io.file]::ReadAllBytes($X); $H=[BitConverter]::ToString($B) -replace '-'; 
set C=%C%  $S=[BitConverter]::ToString([Text.Encoding]::UTF8.GetBytes($Y)) -replace '-'; 
set C=%C%  do { $i=$H.IndexOf($S, $i + 2); if ($i -gt 0) { $w=!0; for ($k=0; $k -lt $l; $k++) { $B[$k + $i / 2]=$R[$k] } } }
set C=%C%  until ($i -lt 1); if ($w) { [io.file]::WriteAllBytes($X, $B); [GC]::Collect() } }
if %VER%_%PRE% == 11_ISO powershell -nop -c iex($env:C) >nul 2>nul
exit /b
";

        private void UpdateBypassStatus()
        {
            using (var key = Registry.LocalMachine.OpenSubKey(IFEO_KEY_PATH))
            {
                if (key != null && key.GetValue("Debugger") as string == SCRIPT_PATH)
                {
                    BypassStatusText.Text = "Status: Active";
                    ActivateButton.IsEnabled = false;
                    DeactivateButton.IsEnabled = true;
                }
                else
                {
                    BypassStatusText.Text = "Status: Inactive";
                    ActivateButton.IsEnabled = true;
                    DeactivateButton.IsEnabled = false;
                }
            }
        }

        private void ActivateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Directory.CreateDirectory(SCRIPT_DIR);
                File.WriteAllText(SCRIPT_PATH, BYPASS_SCRIPT_CONTENT.Replace("W11Bypass.cmd", "get11.cmd"));

                using (var key = Registry.LocalMachine.CreateSubKey(IFEO_KEY_PATH))
                {
                    key.SetValue("Debugger", SCRIPT_PATH, RegistryValueKind.String);
                }
                MessageBox.Show("Bypass activated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to activate bypass: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            UpdateBypassStatus();
        }

        private void DeactivateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(IFEO_KEY_PATH, true))
                {
                    if (key != null)
                    {
                        Registry.LocalMachine.DeleteSubKeyTree(IFEO_KEY_PATH, false);
                    }
                }

                if (File.Exists(SCRIPT_PATH)) File.Delete(SCRIPT_PATH);
                MessageBox.Show("Bypass deactivated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to deactivate bypass: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            UpdateBypassStatus();
        }
        #endregion

        // =================================================================
        // ==                 INSIDER ENROLL SECTION                    ==
        // =================================================================
        #region Insider Enrollment

        private void UpdateInsiderStatus()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\WindowsSelfHost\Applicability"))
                {
                    if (key != null && key.GetValue("BranchName") != null)
                    {
                        var channel = key.GetValue("BranchName").ToString();
                        InsiderStatusText.Text = $"Current Status: Enrolled in {channel}";
                        EnrollButton.Content = "Change Enrollment";
                    }
                    else
                    {
                        InsiderStatusText.Text = "Current Status: Not Enrolled";
                        EnrollButton.Content = "Enroll in Selected Channel";
                    }
                }
            }
            catch
            {
                InsiderStatusText.Text = "Current Status: Not Enrolled";
                EnrollButton.Content = "Enroll in Selected Channel";
            }
        }

        private void EnrollButton_Click(object sender, RoutedEventArgs e)
        {
            if (InsiderChannelComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a channel first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedItem = (InsiderChannelComboBox.SelectedItem as ComboBoxItem).Content.ToString();
            string channel, fancy, brl, content, ring, rid;

            switch (selectedItem)
            {
                case "Canary Channel":
                    channel = "CanaryChannel"; fancy = "Canary Channel"; brl = ""; content = "Mainline"; ring = "External"; rid = "11";
                    break;
                case "Dev Channel":
                    channel = "Dev"; fancy = "Dev Channel"; brl = "2"; content = "Mainline"; ring = "External"; rid = "11";
                    break;
                case "Beta Channel":
                    channel = "Beta"; fancy = "Beta Channel"; brl = "4"; content = "Mainline"; ring = "External"; rid = "11";
                    break;
                case "Release Preview Channel":
                    channel = "ReleasePreview"; fancy = "Release Preview Channel"; brl = "8"; content = "Mainline"; ring = "External"; rid = "11";
                    break;
                default:
                    return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                ResetInsiderConfig(ring);
                AddInsiderConfig(channel, fancy, brl, content, ring, rid);
                RunCommand("bcdedit /set {current} flightsigning yes");

                Mouse.OverrideCursor = null;
                UpdateInsiderStatus();
                MessageBox.Show($"Successfully enrolled in the {fancy}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                PromptForReboot();
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopInsiderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                ResetInsiderConfig("External"); // Use a default ring value for cleanup
                RunCommand("bcdedit /deletevalue {current} flightsigning");

                Mouse.OverrideCursor = null;
                UpdateInsiderStatus();
                MessageBox.Show("Successfully unenrolled from the Windows Insider Program.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                PromptForReboot();
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetInsiderConfig(string ring)
        {
            string[] keysToDelete = {
                @"SOFTWARE\Microsoft\WindowsSelfHost\Account", @"SOFTWARE\Microsoft\WindowsSelfHost\Applicability",
                @"SOFTWARE\Microsoft\WindowsSelfHost\Cache", @"SOFTWARE\Microsoft\WindowsSelfHost\ClientState",
                @"SOFTWARE\Microsoft\WindowsSelfHost\UI", @"SOFTWARE\Microsoft\WindowsSelfHost\Restricted",
                @"SOFTWARE\Microsoft\WindowsSelfHost\ToastNotification", @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\SLS\Programs\WUMUDCat",
                $@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\SLS\Programs\Ring{ring}", @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\SLS\Programs\RingExternal",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\SLS\Programs\RingPreview", @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\SLS\Programs\RingInsiderSlow",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\SLS\Programs\RingInsiderFast"
            };

            foreach (var keyPath in keysToDelete)
            {
                try { Registry.LocalMachine.DeleteSubKeyTree(keyPath, false); } catch { /* Ignore if key doesn't exist */ }
            }
            try { Registry.CurrentUser.DeleteSubKeyTree(@"SOFTWARE\Microsoft\PCHC", false); } catch { /* Ignore */ }

            RunCommand(@"reg delete HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection /v AllowTelemetry /f");
            RunCommand(@"reg delete HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate /v BranchReadinessLevel /f");
            RunCommand(@"reg delete HKLM\SYSTEM\Setup\LabConfig /f");
        }

        private void AddInsiderConfig(string channel, string fancy, string brl, string content, string ring, string rid)
        {
            // Set required values
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WindowsSelfHost\Applicability", "BranchName", channel, RegistryValueKind.String);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WindowsSelfHost\Applicability", "Ring", ring, RegistryValueKind.String);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WindowsSelfHost\Applicability", "ContentType", content, RegistryValueKind.String);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WindowsSelfHost\Applicability", "EnablePreviewBuilds", 2, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WindowsSelfHost\Applicability", "IsBuildFlightingEnabled", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", 3, RegistryValueKind.DWord);

            // Set optional BranchReadinessLevel
            if (!string.IsNullOrEmpty(brl))
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "BranchReadinessLevel", int.Parse(brl), RegistryValueKind.DWord);
            }
            // Add bypass checks for good measure
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\Setup\MoSetup", "AllowUpgradesWithUnsupportedTPMOrCPU", 1, RegistryValueKind.DWord);
        }

        private void PromptForReboot()
        {
            var result = MessageBox.Show("A reboot is required to finish applying changes. Would you like to reboot now?", "Reboot Required", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                RunCommand("shutdown /r /t 0");
            }
        }

        private void RunCommand(string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            var process = Process.Start(processInfo);
            process.WaitForExit();
        }

        #endregion
    }
}