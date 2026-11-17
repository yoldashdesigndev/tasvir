# Architecture

Tasvir is split into five projects with dependencies pointing inward only:

```
Tasvir.App  ->  Tasvir.ViewModels  ->  Tasvir.Services  ->  Tasvir.Core
Tasvir.Tests -> (Core, Services, ViewModels)
```

- **Core** holds models, service contracts and the MVVM base classes. It has no NuGet dependencies and
  no platform-specific types, so it can be tested anywhere.
- **Services** implements ADB communication, Scrcpy process control, device monitoring, settings
  persistence and localization. All external processes are launched through `IProcessRunner` or
  `IProcessLauncher`, which allows tests to replace them with fakes.
- **ViewModels** contains presentation logic. It never references WPF types; the native window handle is
  passed to the view as data.
- **App** is the WPF shell: views, design-system resources, the Win32 `HwndHost` embedding of the Scrcpy
  window, the composition root and centralized exception handling.
- **Tests** uses xunit.v3 with fake process runners and device monitors. Hardware-dependent behavior is
  explicitly out of scope for CI.

## Runtime flow

1. `App.OnStartup` loads settings, builds the DI container and starts `IDeviceMonitor`.
2. The monitor polls `adb devices -l` and publishes device changes.
3. `MirrorViewModel` shows devices and starts `IScrcpyService` on user request.
4. Scrcpy is launched with validated arguments; its window handle is exposed to `MirrorView`, which
   embeds it with `PhonePreviewHost`.
5. Process exit, cancellation and shutdown are observed and reflected in the UI diagnostics.
