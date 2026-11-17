using Tasvir.Core;
using Tasvir.Core.Services;
using Tasvir.Services.Resources;

namespace Tasvir.ViewModels;

public sealed class ShellViewModel : LocalizedViewModel
{
    private readonly MirrorViewModel _mirror;
    private readonly SettingsViewModel _settings;
    private readonly AboutViewModel _about;
    private ViewModelBase _current;
    private bool _isMirrorSelected;
    private bool _isSettingsSelected;
    private bool _isAboutSelected;
    private string _appTitle;
    private string _tagline;
    private string _navMirror;
    private string _navSettings;
    private string _navAbout;
    private string _statusText;
    private string _languageSwitchText;

    public ShellViewModel(
        ILocalizationService localization,
        MirrorViewModel mirror,
        SettingsViewModel settings,
        AboutViewModel about)
        : base(localization)
    {
        _mirror = mirror;
        _settings = settings;
        _about = about;
        _current = mirror;
        _isMirrorSelected = true;

        _appTitle = Strings.AppTitle;
        _tagline = Strings.Tagline;
        _navMirror = Strings.NavMirror;
        _navSettings = Strings.NavSettings;
        _navAbout = Strings.NavAbout;
        _statusText = Strings.StatusDisconnected;
        _languageSwitchText = Strings.LanguageSwitch;

        NavigateMirrorCommand = new RelayCommand(NavigateMirror);
        NavigateSettingsCommand = new RelayCommand(NavigateSettings);
        NavigateAboutCommand = new RelayCommand(NavigateAbout);
        ToggleLanguageCommand = new RelayCommand(ToggleLanguage);
    }

    public string AppTitle { get => _appTitle; private set => SetProperty(ref _appTitle, value); }
    public string Tagline { get => _tagline; private set => SetProperty(ref _tagline, value); }
    public string NavMirror { get => _navMirror; private set => SetProperty(ref _navMirror, value); }
    public string NavSettings { get => _navSettings; private set => SetProperty(ref _navSettings, value); }
    public string NavAbout { get => _navAbout; private set => SetProperty(ref _navAbout, value); }
    public string StatusText { get => _statusText; private set => SetProperty(ref _statusText, value); }
    public string LanguageSwitchText { get => _languageSwitchText; private set => SetProperty(ref _languageSwitchText, value); }

    public MirrorViewModel Mirror => _mirror;

    public ViewModelBase CurrentViewModel
    {
        get => _current;
        private set => SetProperty(ref _current, value);
    }

    public bool IsMirrorSelected { get => _isMirrorSelected; private set => SetProperty(ref _isMirrorSelected, value); }
    public bool IsSettingsSelected { get => _isSettingsSelected; private set => SetProperty(ref _isSettingsSelected, value); }
    public bool IsAboutSelected { get => _isAboutSelected; private set => SetProperty(ref _isAboutSelected, value); }

    public RelayCommand NavigateMirrorCommand { get; }
    public RelayCommand NavigateSettingsCommand { get; }
    public RelayCommand NavigateAboutCommand { get; }
    public RelayCommand ToggleLanguageCommand { get; }

    protected override void OnLocalizationChanged()
    {
        AppTitle = Strings.AppTitle;
        Tagline = Strings.Tagline;
        NavMirror = Strings.NavMirror;
        NavSettings = Strings.NavSettings;
        NavAbout = Strings.NavAbout;
        StatusText = Strings.StatusDisconnected;
        LanguageSwitchText = Strings.LanguageSwitch;
    }

    private void NavigateMirror() => Select(_mirror);

    private void NavigateSettings() => Select(_settings);

    private void NavigateAbout() => Select(_about);

    private void Select(ViewModelBase target)
    {
        CurrentViewModel = target;
        IsMirrorSelected = ReferenceEquals(target, _mirror);
        IsSettingsSelected = ReferenceEquals(target, _settings);
        IsAboutSelected = ReferenceEquals(target, _about);
    }

    private void ToggleLanguage() =>
        Localization.SetLanguage(Localization.Language == "tr" ? "en" : "tr");

    public override void Dispose()
    {
        _mirror.Dispose();
        _settings.Dispose();
        _about.Dispose();
        base.Dispose();
    }
}
