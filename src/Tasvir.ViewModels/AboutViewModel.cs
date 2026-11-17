using Tasvir.Core.Services;
using Tasvir.Services.Resources;

namespace Tasvir.ViewModels;

public sealed class AboutViewModel : LocalizedViewModel
{
    private string _title;
    private string _appName;
    private string _tagline;
    private string _description;
    private string _versionLabel;
    private string _versionValue;
    private string _licensesLabel;
    private string _licensesText;
    private string _copyright;

    public AboutViewModel(ILocalizationService localization)
        : base(localization)
    {
        _title = Strings.AboutTitle;
        _appName = Strings.AppTitle;
        _tagline = Strings.Tagline;
        _description = Strings.AboutDescription;
        _versionLabel = Strings.Version;
        _versionValue = typeof(AboutViewModel).Assembly.GetName().Version?.ToString(3) ?? "1.0.0";
        _licensesLabel = Strings.Licenses;
        _licensesText = Strings.LicensesText;
        _copyright = Strings.Copyright;
    }

    public string Title { get => _title; private set => SetProperty(ref _title, value); }
    public string AppName { get => _appName; private set => SetProperty(ref _appName, value); }
    public string Tagline { get => _tagline; private set => SetProperty(ref _tagline, value); }
    public string Description { get => _description; private set => SetProperty(ref _description, value); }
    public string VersionLabel { get => _versionLabel; private set => SetProperty(ref _versionLabel, value); }
    public string VersionValue => _versionValue;
    public string LicensesLabel { get => _licensesLabel; private set => SetProperty(ref _licensesLabel, value); }
    public string LicensesText { get => _licensesText; private set => SetProperty(ref _licensesText, value); }
    public string Copyright { get => _copyright; private set => SetProperty(ref _copyright, value); }

    protected override void OnLocalizationChanged()
    {
        Title = Strings.AboutTitle;
        AppName = Strings.AppTitle;
        Tagline = Strings.Tagline;
        Description = Strings.AboutDescription;
        VersionLabel = Strings.Version;
        LicensesLabel = Strings.Licenses;
        LicensesText = Strings.LicensesText;
        Copyright = Strings.Copyright;
    }
}
