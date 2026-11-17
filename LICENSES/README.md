# Third-party licenses and notices

Tasvir uses a small number of external components. This file records their licenses; the Windows release
also ships the full license text of the Scrcpy engine under `licenses/`.

## Scrcpy 5.0.1

- Upstream: https://github.com/Genymobile/scrcpy
- License: Apache License 2.0
- Files used: `scrcpy.exe`, `scrcpy-server`, `SDL3.dll`, FFmpeg DLLs, `libusb-1.0.dll`
- The upstream `LICENSE.txt` is copied unchanged into release `licenses/scrcpy-LICENSE.txt`.
- Scrcpy is used as an underlying engine only; the user-facing application is Tasvir.

## Android SDK Platform-Tools (adb)

- Upstream: https://developer.android.com/tools/releases/platform-tools
- License: Android Software Development Kit License Agreement
- Files used: `adb.exe`, `AdbWinApi.dll`, `AdbWinUsbApi.dll`
- The archive is downloaded in CI and verified against a pinned SHA-256 hash before packaging.

## Microsoft.Extensions.DependencyInjection 10.0.0

- License: MIT
- Used only for object composition at application startup.

## Test dependencies

- xunit.v3 4.0.3 — Apache-2.0 (test-only)
- Microsoft.NET.Test.Sdk 17.14.1 — MIT (test-only)

## SBOM

Each release build produces an SPDX Software Bill of Materials in `artifacts/sbom/` listing all detected
components. It is published together with the Windows x64 artifact.
