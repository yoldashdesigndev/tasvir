using Tasvir.Core.Models;
using Tasvir.Core.Services;
using Tasvir.Services.Resources;

namespace Tasvir.ViewModels;

public sealed class SettingsViewModel : LocalizedViewModel
{
    private readonly ISettingsService _settings;
    private string _title;
    private string _languageLabel;
    private string _qualityLabel;
    private string _resolutionLabel;
    private string _bitrateLabel;
    private string _frameRateLabel;
    private string _rotationLabel;
    private string _connectionLabel;
    private string _autoReconnectLabel;
    private LanguageOption _selectedLanguage;
    private string _selectedResolution;
    private string _selectedBitrate;
    private string _selectedFrameRate;
    private string _selectedRotation;
    private bool _preferUsb;
    private bool _autoReconnect;

    public SettingsViewModel(ISettingsService settings, ILocalizationService localization)
        : base(localization)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings;
        _settings.SettingsChanged += OnSettingsChanged;

        _title = Strings.SettingsTitle;
        _languageLabel = Strings.Language;
        _qualityLabel = Strings.Quality;
        _resolutionLabel = Strings.Resolution;
        _bitrateLabel = Strings.Bitrate;
        _frameRateLabel = Strings.FrameRate;
        _rotationLabel = Strings.Rotation;
        _connectionLabel = Strings.Connection;
        _autoReconnectLabel = Strings.AutoReconnect;

        _selectedLanguage = CreateLanguageOption(Localization.Language);
        _selectedResolution = SettingOptions.FormatResolution(settings.Current.MaxWidth, settings.Current.MaxHeight);
        _selectedBitrate = SettingOptions.FormatBitrate(settings.Current.BitrateMbps);
        _selectedFrameRate = SettingOptions.FormatFrameRate(settings.Current.MaxFps);
        _selectedRotation = SettingOptions.FormatRotation(settings.Current.Rotation);
        _preferUsb = settings.Current.PreferredConnection == ConnectionMode.Usb;
        _autoReconnect = settings.Current.AutoReconnect;
    }

    public string Title { get => _title; private set => SetProperty(ref _title, value); }
    public string LanguageLabel { get => _languageLabel; private set => SetProperty(ref _languageLabel, value); }
    public string QualityLabel { get => _qualityLabel; private set => SetProperty(ref _qualityLabel, value); }
    public string ResolutionLabel { get => _resolutionLabel; private set => SetProperty(ref _resolutionLabel, value); }
    public string BitrateLabel { get => _bitrateLabel; private set => SetProperty(ref _bitrateLabel, value); }
    public string FrameRateLabel { get => _frameRateLabel; private set => SetProperty(ref _frameRateLabel, value); }
    public string RotationLabel { get => _rotationLabel; private set => SetProperty(ref _rotationLabel, value); }
    public string ConnectionLabel { get => _connectionLabel; private set => SetProperty(ref _connectionLabel, value); }
    public string AutoReconnectLabel { get => _autoReconnectLabel; private set => SetProperty(ref _autoReconnectLabel, value); }

    public IReadOnlyList<LanguageOption> Languages => new[]
    {
        new LanguageOption("en", Strings.English),
        new LanguageOption("tr", Strings.Turkish)
    };

    public IReadOnlyList<string> ResolutionOptions => SettingOptions.Resolutions;
    public IReadOnlyList<string> BitrateOptions => SettingOptions.Bitrates;
    public IReadOnlyList<string> FrameRateOptions => SettingOptions.FrameRates;
    public IReadOnlyList<string> RotationOptions => SettingOptions.Rotations;

    public LanguageOption SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (value is null || string.Equals(value.Code, _selectedLanguage.Code, StringComparison.Ordinal))
            {
                return;
            }

            _selectedLanguage = value;
            OnPropertyChanged();
            Localization.SetLanguage(value.Code);
        }
    }

    public string SelectedResolution
    {
        get => _selectedResolution;
        set
        {
            if (!SetProperty(ref _selectedResolution, value))
            {
                return;
            }

            if (SettingOptions.TryParseResolution(value, out var width, out var height))
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
            if (!SetProperty(ref _selectedBitrate, value))
            {
                return;
            }

            if (SettingOptions.TryParseLeadingInt(value, out var bitrate))
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
            if (!SetProperty(ref _selectedFrameRate, value))
            {
                return;
            }

            if (SettingOptions.TryParseLeadingInt(value, out var fps))
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

    public bool PreferUsb
    {
        get => _preferUsb;
        set
        {
            if (!SetProperty(ref _preferUsb, value))
            {
                return;
            }

            _settings.Update(s => s.PreferredConnection = value ? ConnectionMode.Usb : ConnectionMode.Wifi);
            OnPropertyChanged(nameof(PreferWifi));
        }
    }

    public bool PreferWifi
    {
        get => !_preferUsb;
        set => PreferUsb = !value;
    }

    public bool AutoReconnect
    {
        get => _autoReconnect;
        set
        {
            if (!SetProperty(ref _autoReconnect, value))
            {
                return;
            }

            _settings.Update(s => s.AutoReconnect = value);
        }
    }

    protected override void OnLocalizationChanged()
    {
        Title = Strings.SettingsTitle;
        LanguageLabel = Strings.Language;
        QualityLabel = Strings.Quality;
        ResolutionLabel = Strings.Resolution;
        BitrateLabel = Strings.Bitrate;
        FrameRateLabel = Strings.FrameRate;
        RotationLabel = Strings.Rotation;
        ConnectionLabel = Strings.Connection;
        AutoReconnectLabel = Strings.AutoReconnect;
        _selectedLanguage = CreateLanguageOption(Localization.Language);
        OnPropertyChanged(nameof(SelectedLanguage));
        OnPropertyChanged(nameof(Languages));
    }

    private LanguageOption CreateLanguageOption(string code) =>
        string.Equals(code, "tr", StringComparison.Ordinal)
            ? new LanguageOption("tr", Strings.Turkish)
            : new LanguageOption("en", Strings.English);

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        SelectedResolution = SettingOptions.FormatResolution(_settings.Current.MaxWidth, _settings.Current.MaxHeight);
        SelectedBitrate = SettingOptions.FormatBitrate(_settings.Current.BitrateMbps);
        SelectedFrameRate = SettingOptions.FormatFrameRate(_settings.Current.MaxFps);
        SelectedRotation = SettingOptions.FormatRotation(_settings.Current.Rotation);
        PreferUsb = _settings.Current.PreferredConnection == ConnectionMode.Usb;
        AutoReconnect = _settings.Current.AutoReconnect;
    }

    public override void Dispose()
    {
        _settings.SettingsChanged -= OnSettingsChanged;
        base.Dispose();
    }
}
