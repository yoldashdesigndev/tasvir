# Security Policy — Tasvir

## Security architecture

- **Least privilege:** Tasvir does not request administrator rights. It runs as the signed-in user.
- **No shell execution:** Every external process is started with `UseShellExecute = false` and a fixed
  executable path. Arguments are passed individually through `ProcessStartInfo.ArgumentList`, so external
  input can never be reinterpreted by a command shell.
- **Input validation:** Host names, ports, device serials and pairing codes are checked with strict,
  culture-invariant regular expressions before any process is started. Untrusted ADB output is sanitized
  (control characters removed, length capped) before being displayed or logged.
- **Engine resolution:** `adb.exe` and `scrcpy.exe` are resolved from the app-local `tools/` directory first.
  The file names are fixed constants; no user-supplied path is ever executed.
- **Explicit approval:** Wireless `connect` and `pair` operations only run after the user clicks the
  corresponding button in the dialog. Tasvir opens no listening ports or network services of its own.
- **Local data only:** Settings are stored as JSON under `%APPDATA%\Tasvir\settings.json`, and crash
  diagnostics under `%LOCALAPPDATA%\Tasvir\error.log`. No passwords, pairing codes or device content are
  persisted. No telemetry is collected or transmitted.
- **Process lifecycle:** ADB and Scrcpy processes are started behind interfaces, serialized to prevent
  duplicates, monitored for unexpected exit, bounded by timeouts, cancellable, and terminated on shutdown.
- **Release integrity:** ADB and Scrcpy archives are downloaded in CI and verified against pinned SHA-256
  hashes before packaging. A SPDX SBOM is generated for every release artifact.
- **Automated scanning:** CI runs a .NET vulnerability audit on every build, CodeQL static analysis for C#,
  and a Microsoft Defender on-demand scan over all files in the release output before artifacts are
  published. If the Windows runner has no Defender CLI, the scan is skipped with a visible warning.

## Known limitations

- Scrcpy and ADB are powerful tools. Once a user authorizes USB debugging or wireless pairing on the
  Android device, the corresponding process has the permissions granted by that authorization. Tasvir
  cannot restrict what the underlying engine is allowed to do on the device.
- If the app-local `tools/adb.exe` is absent, Tasvir falls back to an `adb` found on the system `PATH`.
  The system PATH is trusted machine configuration, but a machine with a malicious `adb` earlier in PATH
  could affect the app. Release packages always include the verified app-local copy.
- Pinned hashes of third-party archives must be updated deliberately when upstream releases change.
  A build fails closed when an archive no longer matches, by design.
- The Scrcpy window is embedded with Win32 `SetParent`/`HwndHost`. If the engine recreates its native
  window (for example after certain fullscreen transitions), the embedded view may need a restart of
  mirroring. This is an integration constraint of window embedding, not a security control.
- No real Android hardware is available in the CI environment. Hardware-dependent behavior is not covered
  by automated tests. The application is not claimed to be crash-proof.

## Third-party components

| Component | Version | License | Role |
|---|---|---|---|
| Scrcpy | 5.0.1 | Apache-2.0 | Screen mirroring and input engine (native binary) |
| Android SDK Platform-Tools (adb) | pinned archive | Android SDK License Agreement | Device communication |
| SDL3, FFmpeg DLLs | as shipped with Scrcpy | SDL zlib license; FFmpeg LGPL/GPL build | Bundled with the Scrcpy release |
| Microsoft.Extensions.DependencyInjection | 10.0.0 | MIT | Runtime dependency-injection container |
| xunit.v3, Microsoft.NET.Test.Sdk | 4.0.3 / 17.14.1 | Apache-2.0 / MIT | Test-only dependencies |

NuGet dependencies were audited on 2026-11-17: no known vulnerabilities and no deprecated runtime
packages. The exact component list for each release is published as an SPDX SBOM in the build artifacts.

## Reporting a vulnerability

Please do **not** open a public issue for security problems. Use GitHub's private vulnerability reporting
for this repository (`Security` → `Report a vulnerability`), or contact the repository owner privately.
Include the affected version, reproduction steps and impact. Reports are acknowledged as soon as possible,
and fixes are released with an updated SBOM and pinned third-party hashes.
