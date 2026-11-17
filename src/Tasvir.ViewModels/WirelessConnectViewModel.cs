using System.Globalization;
using Tasvir.Core;
using Tasvir.Core.Services;
using Tasvir.Services;
using Tasvir.Services.Resources;

namespace Tasvir.ViewModels;

public sealed class WirelessConnectViewModel : LocalizedViewModel
{
    private readonly IAdbService _adb;
    private string _title;
    private string _hostLabel;
    private string _portLabel;
    private string _connectLabel;
    private string _cancelLabel;
    private string _instructions;
    private string _pairLabel;
    private string _pairCodeLabel;
    private string _pairPortLabel;
    private string _host = string.Empty;
    private string _port = "5555";
    private string _pairPort = "37000";
    private string _pairCode = string.Empty;
    private string _errorMessage = string.Empty;
    private string _infoMessage = string.Empty;
    private bool _isBusy;
    private bool _isPairMode;

    public WirelessConnectViewModel(IAdbService adb, ILocalizationService localization)
        : base(localization)
    {
        ArgumentNullException.ThrowIfNull(adb);
        _adb = adb;

        _title = Strings.WirelessTitle;
        _hostLabel = Strings.HostLabel;
        _portLabel = Strings.PortLabel;
        _connectLabel = Strings.Connect;
        _cancelLabel = Strings.Cancel;
        _instructions = Strings.WirelessInstructions;
        _pairLabel = Strings.PairMode;
        _pairCodeLabel = Strings.PairCode;
        _pairPortLabel = Strings.PairPort;

        ConnectCommand = new AsyncRelayCommand(ConnectAsync, () => !IsBusy);
        PairCommand = new AsyncRelayCommand(PairAsync, () => !IsBusy);
        ToggleModeCommand = new RelayCommand(() => IsPairMode = !IsPairMode);
        CancelCommand = new RelayCommand(() => RequestClose?.Invoke(this, EventArgs.Empty));
    }

    public event EventHandler? RequestClose;

    public string Title { get => _title; private set => SetProperty(ref _title, value); }
    public string HostLabel { get => _hostLabel; private set => SetProperty(ref _hostLabel, value); }
    public string CancelLabel { get => _cancelLabel; private set => SetProperty(ref _cancelLabel, value); }
    public string PairCodeLabel { get => _pairCodeLabel; private set => SetProperty(ref _pairCodeLabel, value); }
    public string Host { get => _host; set => SetProperty(ref _host, value); }
    public string Port { get => _port; set => SetProperty(ref _port, value); }
    public string PairPort { get => _pairPort; set => SetProperty(ref _pairPort, value); }
    public string PairCode { get => _pairCode; set => SetProperty(ref _pairCode, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public string InfoMessage { get => _infoMessage; private set => SetProperty(ref _infoMessage, value); }

    public string PortLabel => IsPairMode ? _pairPortLabel : _portLabel;
    public string ActionLabel => IsPairMode ? _pairLabel : _connectLabel;
    public string OtherModeLabel => IsPairMode ? _connectLabel : _pairLabel;
    public string Instructions => IsPairMode ? Strings.PairInstructions : Strings.WirelessInstructions;
    public bool IsConnectModeVisible => !IsPairMode;
    public bool IsPairModeVisible => IsPairMode;

    public bool IsPairMode
    {
        get => _isPairMode;
        set
        {
            if (!SetProperty(ref _isPairMode, value))
            {
                return;
            }

            ErrorMessage = string.Empty;
            InfoMessage = string.Empty;
            OnPropertyChanged(nameof(PortLabel));
            OnPropertyChanged(nameof(ActionLabel));
            OnPropertyChanged(nameof(OtherModeLabel));
            OnPropertyChanged(nameof(Instructions));
            OnPropertyChanged(nameof(IsConnectModeVisible));
            OnPropertyChanged(nameof(IsPairModeVisible));
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                ConnectCommand.RaiseCanExecuteChanged();
                PairCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public AsyncRelayCommand ConnectCommand { get; }
    public AsyncRelayCommand PairCommand { get; }
    public RelayCommand ToggleModeCommand { get; }
    public RelayCommand CancelCommand { get; }

    protected override void OnLocalizationChanged()
    {
        Title = Strings.WirelessTitle;
        _hostLabel = Strings.HostLabel;
        _portLabel = Strings.PortLabel;
        _connectLabel = Strings.Connect;
        _pairLabel = Strings.PairMode;
        _pairCodeLabel = Strings.PairCode;
        _pairPortLabel = Strings.PairPort;
        CancelLabel = Strings.Cancel;

        OnPropertyChanged(nameof(HostLabel));
        OnPropertyChanged(nameof(PortLabel));
        OnPropertyChanged(nameof(ActionLabel));
        OnPropertyChanged(nameof(OtherModeLabel));
        OnPropertyChanged(nameof(PairCodeLabel));
        OnPropertyChanged(nameof(Instructions));

        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ErrorMessage = Strings.ConnectionFailed;
        }
    }

    public async Task ConnectAsync()
    {
        ErrorMessage = string.Empty;
        var host = Host.Trim();

        if (!AdbInputValidator.IsValidHost(host))
        {
            ErrorMessage = Strings.InvalidHost;
            return;
        }

        if (!TryParsePort(Port, out var port))
        {
            ErrorMessage = Strings.InvalidPort;
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _adb.ConnectWirelessAsync(host, port).ConfigureAwait(true);
            if (result.Success)
            {
                RequestClose?.Invoke(this, EventArgs.Empty);
                return;
            }

            ErrorMessage = string.IsNullOrWhiteSpace(result.Message) ? Strings.ConnectionFailed : result.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task PairAsync()
    {
        ErrorMessage = string.Empty;
        InfoMessage = string.Empty;
        var host = Host.Trim();

        if (!AdbInputValidator.IsValidHost(host))
        {
            ErrorMessage = Strings.InvalidHost;
            return;
        }

        if (!TryParsePort(PairPort, out var port))
        {
            ErrorMessage = Strings.InvalidPort;
            return;
        }

        var code = PairCode.Trim();
        if (!AdbInputValidator.IsValidPairCode(code))
        {
            ErrorMessage = Strings.InvalidPairCode;
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _adb.PairAsync(host, port, code).ConfigureAwait(true);
            if (result.Success)
            {
                IsPairMode = false;
                InfoMessage = Strings.Paired;
                return;
            }

            ErrorMessage = string.IsNullOrWhiteSpace(result.Message) ? Strings.ConnectionFailed : result.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static bool TryParsePort(string value, out int port) =>
        int.TryParse(value.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out port) &&
        AdbInputValidator.IsValidPort(port);
}
