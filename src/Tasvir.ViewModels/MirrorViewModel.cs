using Tasvir.Core;
using Tasvir.Core.Exceptions;
using Tasvir.Core.Models;
using Tasvir.Core.Services;
using Tasvir.Services.Resources;

namespace Tasvir.ViewModels;

public sealed class MirrorViewModel : LocalizedViewModel
{
    private readonly ISettingsService _settings;
    private readonly IAdbService _adb;
    private readonly IDeviceMonitor _monitor;
    private readonly IScrcpyService _scrcpy;

    private string _mirrorTitle;
    private string _statusText;
    private string _deviceName;
    private string _deviceSubtitle;
    private string _usbLabel;
    private string _wifiLabel;
    private string _startText;
    private string _stopText;
    private string _quickSettingsText;
    private string _diagnosticsText;
    private string _previewPlaceholder;
    private string _connectionInfoText;
    private string _resolutionLabel;
    private string _bitrateLabel;
    private string _frameRateLabel;
    private string _rotationLabel;
    private string _deviceCountLabel;
    private string _monitoringLabel;
    private string _settingsErrorLabel;
    private string _engineStatusText;
    private string _refreshLabel;
    private string _connectWirelessLabel;
    private string _adbVersionLabel;
    private string _lastErrorLabel;
    private string _deviceSerialLabel;
    private string _disconnectLabel;
    private string _fullscreenLabel;
    private string _mirroringStatusText;
    private string _noDeviceSelected;
    private IntPtr _windowHandle;
    private MirroringState _mirroringState = MirroringState.Stopped;
    private string _adbVersionText = string.Empty;
    private string _lastErrorText = string.Empty;
    private string _settingsErrorText = string.Empty;
    private string _deviceSerialText = string.Empty;
    private IReadOnlyList<DeviceInfo> _devices = Array.Empty<DeviceInfo>();
    private DeviceInfo? _selectedDevice;
    private string _selectedResolution;
    private string _selectedBitrate;
    private string _selectedFrameRate;
    private string _selectedRotation;
    private string? _lastMirroredSerial;
    private bool _autoReconnectArmed;
    private bool _reconnecting;
    private bool _isUsbSelected;
    private bool _isBusy;

    public MirrorViewModel(
        ISettingsService settings,
        ILocalizationService localization,
        IAdbService adb,
        IDeviceMonitor monitor,
        IScrcpyService scrcpy)
        : base(localization)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(adb);
        ArgumentNullException.ThrowIfNull(monitor);
        ArgumentNullException.ThrowIfNull(scrcpy);

        _settings = settings;
        _adb = adb;
        _monitor = monitor;
        _scrcpy = scrcpy;
        _settings.SettingsChanged += OnSettingsChanged;
        _monitor.DevicesChanged += OnDevicesChanged;
        _scrcpy.StateChanged += OnScrcpyStateChanged;
        _scrcpy.WindowReady += OnScrcpyWindowReady;

        _mirrorTitle = Strings.MirrorTitle;
        _statusText = Strings.StatusDisconnected;
        _deviceName = Strings.NoDevice;
        _deviceSubtitle = Strings.UsbInstructions;
        _usbLabel = Strings.Usb;
        _wifiLabel = Strings.Wifi;
        _startText = Strings.StartMirroring;
        _stopText = Strings.StopMirroring;
        _quickSettingsText = Strings.QuickSettings;
        _diagnosticsText = Strings.Diagnostics;
        _previewPlaceholder = Strings.PreviewPlaceholder;
        _connectionInfoText = Strings.ConnectionInfo;
        _resolutionLabel = Strings.Resolution;
        _bitrateLabel = Strings.Bitrate;
        _frameRateLabel = Strings.FrameRate;
        _rotationLabel = Strings.Rotation;
        _deviceCountLabel = Strings.DeviceCount;
        _monitoringLabel = Strings.Monitoring;
        _settingsErrorLabel = Strings.SettingsError;
        _engineStatusText = Strings.EngineStatus;
        _refreshLabel = Strings.Refresh;
        _connectWirelessLabel = Strings.ConnectWireless;
        _adbVersionLabel = Strings.AdbVersion;
        _lastErrorLabel = Strings.LastError;
        _deviceSerialLabel = Strings.DeviceSerial;
        _disconnectLabel = Strings.Disconnect;
        _fullscreenLabel = Strings.Fullscreen;
        _noDeviceSelected = Strings.NoDeviceSelected;
        _mirroringStatusText = Strings.MirrorStopped;
        _settingsErrorText = settings.LastError ?? string.Empty;

        _isUsbSelected = settings.Current.PreferredConnection == ConnectionMode.Usb;
        _selectedResolution = SettingOptions.FormatResolution(settings.Current.MaxWidth, settings.Current.MaxHeight);
        _selectedBitrate = SettingOptions.FormatBitrate(settings.Current.BitrateMbps);
        _selectedFrameRate = SettingOptions.FormatFrameRate(settings.Current.MaxFps);
        _selectedRotation = SettingOptions.FormatRotation(settings.Current.Rotation);

        RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => !IsBusy);
        ConnectWirelessCommand = new RelayCommand(() => RequestWirelessConnection?.Invoke(this, EventArgs.Empty));
        DisconnectCommand = new AsyncRelayCommand(DisconnectAsync, () => SelectedDevice is not null && !IsBusy);
        SelectUsbCommand = new RelayCommand(() => IsUsbSelected = true);
        SelectWifiCommand = new RelayCommand(() => IsUsbSelected = false);
        StartMirroringCommand = new AsyncRelayCommand(StartMirroringAsync, () => CanStartMirroring);
        StopMirroringCommand = new AsyncRelayCommand(StopMirroringAsync, () => IsMirroring || _mirroringState == MirroringState.Starting);
        ToggleFullscreenCommand = new RelayCommand(() => RequestFullscreenToggle?.Invoke(this, EventArgs.Empty));
    }

    public event EventHandler? RequestWirelessConnection;
    public event EventHandler? RequestFullscreenToggle;

    public string MirrorTitle { get => _mirrorTitle; private set => SetProperty(ref _mirrorTitle, value); }
    public string StatusText { get => _statusText; private set => SetProperty(ref _statusText, value); }
    public string DeviceName { get => _deviceName; private set => SetProperty(ref _deviceName, value); }
    public string DeviceSubtitle { get => _deviceSubtitle; private set => SetProperty(ref _deviceSubtitle, value); }
    public string UsbLabel { get => _usbLabel; private set => SetProperty(ref _usbLabel, value); }
    public string WifiLabel { get => _wifiLabel; private set => SetProperty(ref _wifiLabel, value); }
    public string StartText { get => _startText; private set => SetProperty(ref _startText, value); }
    public string StopText { get => _stopText; private set => SetProperty(ref _stopText, value); }
    public string QuickSettingsText { get => _quickSettingsText; private set => SetProperty(ref _quickSettingsText, value); }
    public string DiagnosticsText { get => _diagnosticsText; private set => SetProperty(ref _diagnosticsText, value); }
    public string PreviewPlaceholder { get => _previewPlaceholder; private set => SetProperty(ref _previewPlaceholder, value); }
    public string ConnectionInfoText { get => _connectionInfoText; private set => SetProperty(ref _connectionInfoText, value); }
    public string ResolutionLabel { get => _resolutionLabel; private set => SetProperty(ref _resolutionLabel, value); }
    public string BitrateLabel { get => _bitrateLabel; private set => SetProperty(ref _bitrateLabel, value); }
    public string FrameRateLabel { get => _frameRateLabel; private set => SetProperty(ref _frameRateLabel, value); }
    public string RotationLabel { get => _rotationLabel; private set => SetProperty(ref _rotationLabel, value); }
    public string DeviceCountLabel { get => _deviceCountLabel; private set => SetProperty(ref _deviceCountLabel, value); }
    public string MonitoringLabel { get => _monitoringLabel; private set => SetProperty(ref _monitoringLabel, value); }
    public string SettingsErrorLabel { get => _settingsErrorLabel; private set => SetProperty(ref _settingsErrorLabel, value); }
    public string EngineStatusText { get => _engineStatusText; private set => SetProperty(ref _engineStatusText, value); }
    public string RefreshLabel { get => _refreshLabel; private set => SetProperty(ref _refreshLabel, value); }
    public string ConnectWirelessLabel { get => _connectWirelessLabel; private set => SetProperty(ref _connectWirelessLabel, value); }
    public string AdbVersionLabel { get => _adbVersionLabel; private set => SetProperty(ref _adbVersionLabel, value); }
    public string LastErrorLabel { get => _lastErrorLabel; private set => SetProperty(ref _lastErrorLabel, value); }
    public string DeviceSerialLabel { get => _deviceSerialLabel; private set => SetProperty(ref _deviceSerialLabel, value); }
    public string DisconnectLabel { get => _disconnectLabel; private set => SetProperty(ref _disconnectLabel, value); }
    public string FullscreenLabel { get => _fullscreenLabel; private set => SetProperty(ref _fullscreenLabel, value); }
    public string MirroringStatusText { get => _mirroringStatusText; private set => SetProperty(ref _mirroringStatusText, value); }
    public string NoDeviceSelected { get => _noDeviceSelected; private set => SetProperty(ref _noDeviceSelected, value); }
    public string AdbVersionText { get => _adbVersionText; private set => SetProperty(ref _adbVersionText, value); }
    public string LastErrorText { get => _lastErrorText; private set => SetProperty(ref _lastErrorText, value); }
    public string SettingsErrorText { get => _settingsErrorText; private set => SetProperty(ref _settingsErrorText, value); }
    public string DeviceSerialText { get => _deviceSerialText; private set => SetProperty(ref _deviceSerialText, value); }

    public IReadOnlyList<string> ResolutionOptions => SettingOptions.Resolutions;
    public IReadOnlyList<string> BitrateOptions => SettingOptions.Bitrates;
    public IReadOnlyList<string> FrameRateOptions => SettingOptions.FrameRates;
    public IReadOnlyList<string> RotationOptions => SettingOptions.Rotations;

    public IReadOnlyList<DeviceInfo> Devices
    {
        get => _devices;
        private set => SetProperty(ref _devices, value);
    }

    public DeviceInfo? SelectedDevice
    {
        get => _selectedDevice;
        set
        {
            if (SetProperty(ref _selectedDevice, value))
            {
                RefreshDeviceDetails();
                DisconnectCommand.RaiseCanExecuteChanged();
                StartMirroringCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
                DisconnectCommand.RaiseCanExecuteChanged();
                StartMirroringCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsConnected => SelectedDevice?.State == DeviceConnectionState.Connected;

    public IntPtr WindowHandle
    {
        get => _windowHandle;
        private set => SetProperty(ref _windowHandle, value);
    }

    public bool IsMirroring => _mirroringState == MirroringState.Running;

    public bool IsPlaceholderVisible => !IsMirroring;

    public bool IsPreviewVisible => IsMirroring && WindowHandle != IntPtr.Zero;

    public bool CanStartMirroring =>
        SelectedDevice?.State == DeviceConnectionState.Connected &&
        _mirroringState == MirroringState.Stopped &&
        !IsBusy;

    public bool IsUsbSelected
    {
        get => _isUsbSelected;
        set
        {
            if (!SetProperty(ref _isUsbSelected, value))
            {
                return;
            }

            _settings.Update(s => s.PreferredConnection = value ? ConnectionMode.Usb : ConnectionMode.Wifi);
            OnPropertyChanged(nameof(IsWifiSelected));
            RefreshDeviceDetails();
        }
    }

    public bool IsWifiSelected
    {
        get => !_isUsbSelected;
        set => IsUsbSelected = !value;
    }

    public string SelectedResolution
    {
        get => _selectedResolution;
        set
        {
            if (SetProperty(ref _selectedResolution, value) &&
                SettingOptions.TryParseResolution(value, out var width, out var height))
            {
                _settings.Update(s => { s.MaxWidth = width; s.MaxHeight = height; });
            }
        }
    }

    public string SelectedBitrate
    {
        get => _selectedBitrate;
        set
        {
            if (SetProperty(ref _selectedBitrate, value) &&
                SettingOptions.TryParseLeadingInt(value, out var bitrate))
            {
                _settings.Update(s => s.BitrateMbps = bitrate);
            }
        }
    }

    public string SelectedFrameRate
    {
        get => _selectedFrameRate;
        set
        {
            if (SetProperty(ref _selectedFrameRate, value) &&
                SettingOptions.TryParseLeadingInt(value, out var fps))
            {
                _settings.Update(s => s.MaxFps = fps);
            }
        }
    }

    public string SelectedRotation
    {
        get => _selectedRotation;
        set
        {
            if (SetProperty(ref _selectedRotation, value) &&
                SettingOptions.TryParseRotation(value, out var rotation))
            {
                _settings.Update(s => s.Rotation = rotation);
            }
        }
    }

    public string DevicesCountText => $"{Devices.Count} {Strings.DevicesFound}";

    public string MonitoringText => _monitor.IsRunning ? Strings.Active : Strings.Paused;

    public AsyncRelayCommand RefreshCommand { get; }
    public RelayCommand ConnectWirelessCommand { get; }
    public AsyncRelayCommand DisconnectCommand { get; }
    public RelayCommand SelectUsbCommand { get; }
    public RelayCommand SelectWifiCommand { get; }
    public AsyncRelayCommand StartMirroringCommand { get; }
    public AsyncRelayCommand StopMirroringCommand { get; }
    public RelayCommand ToggleFullscreenCommand { get; }

    protected override void OnLocalizationChanged()
    {
        MirrorTitle = Strings.MirrorTitle;
        UsbLabel = Strings.Usb;
        WifiLabel = Strings.Wifi;
        StartText = Strings.StartMirroring;
        StopText = Strings.StopMirroring;
        QuickSettingsText = Strings.QuickSettings;
        DiagnosticsText = Strings.Diagnostics;
        PreviewPlaceholder = Strings.PreviewPlaceholder;
        ConnectionInfoText = Strings.ConnectionInfo;
        ResolutionLabel = Strings.Resolution;
        BitrateLabel = Strings.Bitrate;
        FrameRateLabel = Strings.FrameRate;
        RotationLabel = Strings.Rotation;
        DeviceCountLabel = Strings.DeviceCount;
        MonitoringLabel = Strings.Monitoring;
        SettingsErrorLabel = Strings.SettingsError;
        EngineStatusText = Strings.EngineStatus;
        RefreshLabel = Strings.Refresh;
        ConnectWirelessLabel = Strings.ConnectWireless;
        AdbVersionLabel = Strings.AdbVersion;
        LastErrorLabel = Strings.LastError;
        DeviceSerialLabel = Strings.DeviceSerial;
        DisconnectLabel = Strings.Disconnect;
        FullscreenLabel = Strings.Fullscreen;
        NoDeviceSelected = Strings.NoDeviceSelected;
        RefreshMirroringText();
        RefreshDeviceDetails();
        OnPropertyChanged(nameof(DevicesCountText));
        OnPropertyChanged(nameof(MonitoringText));
    }

    private void OnDevicesChanged(object? sender, EventArgs e)
    {
        var previousSerial = _selectedDevice?.Serial;
        Devices = _monitor.Devices;
        SelectedDevice = Devices.FirstOrDefault(device => device.Serial == previousSerial)
                         ?? Devices.FirstOrDefault(device => device.State == DeviceConnectionState.Connected)
                         ?? (Devices.Count > 0 ? Devices[0] : null);
        LastErrorText = _monitor.LastError ?? string.Empty;
        OnPropertyChanged(nameof(DevicesCountText));
        OnPropertyChanged(nameof(MonitoringText));
        TryAutoReconnect();
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        IsUsbSelected = _settings.Current.PreferredConnection == ConnectionMode.Usb;
        SelectedResolution = SettingOptions.FormatResolution(_settings.Current.MaxWidth, _settings.Current.MaxHeight);
        SelectedBitrate = SettingOptions.FormatBitrate(_settings.Current.BitrateMbps);
        SelectedFrameRate = SettingOptions.FormatFrameRate(_settings.Current.MaxFps);
        SelectedRotation = SettingOptions.FormatRotation(_settings.Current.Rotation);
    }

    private void RefreshDeviceDetails()
    {
        if (_selectedDevice is null)
        {
            StatusText = Strings.StatusDisconnected;
            DeviceName = Strings.NoDevice;
            DeviceSubtitle = _isUsbSelected ? Strings.UsbInstructions : Strings.DevicePlaceholder;
            DeviceSerialText = string.Empty;
        }
        else
        {
            StatusText = _selectedDevice.State switch
            {
                DeviceConnectionState.Connected => Strings.StatusConnected,
                DeviceConnectionState.Unauthorized => Strings.StatusUnauthorized,
                DeviceConnectionState.Connecting => Strings.StatusConnecting,
                DeviceConnectionState.Offline => Strings.StatusOffline,
                _ => Strings.StatusError
            };
            DeviceName = string.IsNullOrWhiteSpace(_selectedDevice.DisplayName)
                ? _selectedDevice.Serial
                : _selectedDevice.DisplayName;
            DeviceSubtitle = _selectedDevice.Mode == ConnectionMode.Wifi ? Strings.Wifi : Strings.Usb;
            DeviceSerialText = _selectedDevice.Serial;
        }

        OnPropertyChanged(nameof(IsConnected));
    }

    public async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            await _monitor.RefreshAsync().ConfigureAwait(true);
            try
            {
                AdbVersionText = await _adb.GetVersionAsync().ConfigureAwait(true);
            }
            catch (AdbException exception)
            {
                AdbVersionText = exception.Message;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DisconnectAsync()
    {
        if (_selectedDevice is null)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _adb.DisconnectAsync(_selectedDevice.Serial).ConfigureAwait(true);
            if (!result.Success)
            {
                LastErrorText = result.Message;
            }

            await _monitor.RefreshAsync().ConfigureAwait(true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnScrcpyStateChanged(object? sender, MirroringState state)
    {
        _mirroringState = state;
        RefreshMirroringText();
        OnPropertyChanged(nameof(IsMirroring));
        OnPropertyChanged(nameof(IsPlaceholderVisible));
        OnPropertyChanged(nameof(IsPreviewVisible));
        OnPropertyChanged(nameof(CanStartMirroring));
        if (state == MirroringState.Error)
        {
            LastErrorText = _scrcpy.LastError ?? Strings.MirrorFailed;
            _autoReconnectArmed = false;
        }
        else if (state == MirroringState.Stopped && _reconnecting)
        {
            _reconnecting = false;
        }
        else if (state == MirroringState.Stopped && _autoReconnectArmed)
        {
            MirroringStatusText = Strings.DeviceDisconnected;
        }

        StartMirroringCommand.RaiseCanExecuteChanged();
        StopMirroringCommand.RaiseCanExecuteChanged();
    }

    private void OnScrcpyWindowReady(object? sender, EventArgs e)
    {
        WindowHandle = _scrcpy.WindowHandle;
        OnPropertyChanged(nameof(IsPreviewVisible));
    }

    private void RefreshMirroringText()
    {
        MirroringStatusText = _mirroringState switch
        {
            MirroringState.Starting => Strings.MirrorStarting,
            MirroringState.Running => Strings.MirrorRunning,
            MirroringState.Error => Strings.MirrorFailed,
            _ => Strings.MirrorStopped
        };
    }

    public async Task StartMirroringAsync()
    {
        if (SelectedDevice is null)
        {
            LastErrorText = Strings.NoDeviceSelected;
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _scrcpy.StartAsync(SelectedDevice, _settings.Current).ConfigureAwait(true);
            if (result.Success)
            {
                _lastMirroredSerial = SelectedDevice.Serial;
                _autoReconnectArmed = true;
            }
            else
            {
                LastErrorText = string.IsNullOrWhiteSpace(result.Message) ? Strings.MirrorFailed : result.Message;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task StopMirroringAsync()
    {
        _autoReconnectArmed = false;
        _lastMirroredSerial = null;
        IsBusy = true;
        try
        {
            await _scrcpy.StopAsync().ConfigureAwait(true);
            WindowHandle = IntPtr.Zero;
            OnPropertyChanged(nameof(IsPreviewVisible));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void TryAutoReconnect()
    {
        if (!_autoReconnectArmed || _reconnecting || !_settings.Current.AutoReconnect ||
            _mirroringState != MirroringState.Stopped || string.IsNullOrEmpty(_lastMirroredSerial))
        {
            return;
        }

        var device = Devices.FirstOrDefault(candidate =>
            candidate.Serial == _lastMirroredSerial &&
            candidate.State == DeviceConnectionState.Connected);
        if (device is null)
        {
            return;
        }

        _reconnecting = true;
        _ = ReconnectAsync(device);
    }

    private async Task ReconnectAsync(DeviceInfo device)
    {
        MirroringStatusText = Strings.Reconnecting;
        try
        {
            await Task.Delay(300).ConfigureAwait(true);
            var result = await _scrcpy.StartAsync(device, _settings.Current).ConfigureAwait(true);
            if (result.Success)
            {
                _lastMirroredSerial = device.Serial;
            }
            else
            {
                LastErrorText = result.Message;
                _autoReconnectArmed = false;
            }
        }
        catch (OperationCanceledException)
        {
            _autoReconnectArmed = false;
        }
        finally
        {
            _reconnecting = false;
        }
    }

    public override void Dispose()
    {
        _settings.SettingsChanged -= OnSettingsChanged;
        _monitor.DevicesChanged -= OnDevicesChanged;
        _scrcpy.StateChanged -= OnScrcpyStateChanged;
        _scrcpy.WindowReady -= OnScrcpyWindowReady;
        base.Dispose();
    }
}
