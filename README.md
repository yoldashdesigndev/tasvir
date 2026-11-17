# Tasvir

**Your Phone. Your Desktop.**

Tasvir is a lightweight Windows application for mirroring and controlling an Android phone from the desktop.
It uses the official Scrcpy engine underneath, but provides its own interface, device management, wireless
connection workflow and diagnostics.

## Features

- Automatic USB device detection with model, serial and authorization state
- Wireless ADB connection and pairing with explicit user approval
- One-click screen mirroring embedded in the application window
- Mouse and keyboard control through the native Scrcpy window
- Fullscreen mode, resolution, bitrate, frame rate and rotation settings
- Automatic reconnection when a mirrored device returns
- Turkish and English interface, switchable at runtime
- Diagnostics panel with device state, ADB version and last errors
- Pinned, hash-verified ADB and Scrcpy binaries; SPDX SBOM in every release

## Requirements

- Windows 10 or Windows 11, x64
- [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) (the release is framework-dependent)
- Android device with USB debugging or Wireless debugging enabled
- USB data cable for USB connections

## Usage

1. Start `Tasvir.exe`.
2. **USB:** Connect the phone, enable USB debugging and accept the authorization prompt on the device.
3. **Wi-Fi:** Open *Connect via Wi-Fi*. Use *Pair device* with the pairing code and port from the phone's
   Wireless debugging settings, then connect using the connection port. Pairing and connection both require
   clicking the button in Tasvir and confirming on the phone.
4. Select the device in the device list and click **Start Mirroring**.
5. Click the embedded phone window to give it keyboard focus. Use **Fullscreen** for a larger view.

## Building from source

The solution targets .NET 10 and uses WPF. The XAML compiles on Linux with `EnableWindowsTargeting`, but the
graphical interface can only be run and visually verified on Windows.

```bash
dotnet restore Tasvir.sln
dotnet build Tasvir.sln -c Release
dotnet test src/Tasvir.Tests/Tasvir.Tests.csproj -c Release
dotnet publish src/Tasvir.App/Tasvir.App.csproj -c Release -r win-x64 --self-contained false -o artifacts/publish
```

GitHub Actions performs the authoritative Windows x64 build, tests, hash-verified bundling of ADB and
Scrcpy, license preservation and SBOM generation. Pushing a `v*` tag creates a portable GitHub Release.

## Release package

`Tasvir-win-x64-portable.zip` contains:

- `Tasvir.exe` and application assemblies
- `tools/` — verified `adb.exe`, `scrcpy.exe`, `scrcpy-server` and their native dependencies
- `licenses/` — third-party notices, Scrcpy Apache-2.0 license and the SPDX SBOM
- `README.md`

No installer is published: the portable package keeps the download small and avoids elevated privileges.
Unzip it into a writable folder and run `Tasvir.exe`.

## Project structure

- `src/Tasvir.Core` — models, service contracts and framework-free MVVM primitives
- `src/Tasvir.Services` — ADB/Scrcpy process management, validation, settings and localization
- `src/Tasvir.ViewModels` — UI-independent presentation logic
- `src/Tasvir.App` — WPF views, design system and composition root
- `src/Tasvir.Tests` — automated unit and resilience tests
- `docs/` — project documentation
- `SECURITY.md`, `LICENSES/`, `.github/workflows/build.yml`

## Security and limitations

Tasvir follows least-privilege principles: no administrator rights, no shell execution, strict input
validation and no telemetry. See [SECURITY.md](SECURITY.md) for the security architecture, known
limitations and the vulnerability reporting process.

Automated tests use simulated processes. Real-device connectivity, mirroring quality and long-running
sessions require Windows hardware and are not covered by CI. The application is not claimed to be
crash-proof.
