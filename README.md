# Windows Insider Tool

A user-friendly WPF application for managing Windows Insider Program enrollment and bypassing Windows 11 upgrade requirements directly from your desktop.

---

## Features
* **Windows 11 Upgrade Bypass:**
    * **Activate:** Implements a registry-based bypass to circumvent the TPM and CPU checks for in-place upgrades to Windows 11.
    * **Deactivate:** Safely removes the bypass script and registry keys, restoring the system to its default state.
* **Insider Program Enrollment:**
    * **Enroll/Change Channel:** Join the Canary, Dev, Beta, or Release Preview channels with a single click.
    * **Unenroll:** Stop receiving Insider Preview builds and clean up related registry configurations.
    * **Status Check:** Displays your current enrollment status and channel.
---

## Getting Started

### Prerequisites

* Windows 10 or Windows 11.
* **.NET Runtime:** This application requires a .NET runtime (e.g., .NET 6/7/8). If you do not have it installed, please use the **packaged** version of the release, which includes the necessary runtime files.
* **Administrator Privileges:** The application must be run as an administrator to modify the required system registry settings. The tool has a built-in check and will notify you if not run with the correct permissions.

### How to Use

1.  **Download:** Grab the latest release from the [Releases](https://github.com/phalchanouksa/WindowsInsiderTools/releases) page.
2.  **Run as Administrator:** Right-click the `WindowInsiderTool.exe` and select "Run as administrator".
3.  **Use the Interface:**
    * **For the Upgrade Bypass:**
        * Click **Activate** to apply the bypass.
        * Click **Deactivate** to remove it.
        * The status will update accordingly.
    * **For Insider Enrollment:**
        * Select your desired channel from the dropdown menu.
        * Click **Enroll in Selected Channel**.
        * To leave the program, click **Stop Receiving Insider Builds**.
        * Reboot your computer when prompted to apply the changes.

---

## ⚠️ Disclaimer

This application makes significant changes to the Windows Registry and system configuration files. It is intended for experienced users, developers, and IT professionals for testing and educational purposes.

* **Use at your own risk.** Modifying the registry can lead to system instability or issues with future updates if not done correctly.
* Bypassing hardware requirements for Windows 11 may result in a non-optimal or unsupported user experience.
* Always back up important data before making system-level changes.

---

## Contributing

Contributions are welcome! If you have suggestions for improvements or want to report a bug, please feel free to open an issue or submit a pull request.

---

## License

This project is distributed under the MIT License. See the `LICENSE` file for more information.
