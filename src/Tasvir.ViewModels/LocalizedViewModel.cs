using Tasvir.Core.Services;

namespace Tasvir.ViewModels;

public abstract class LocalizedViewModel : ViewModelBase
{
    protected LocalizedViewModel(ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(localization);
        Localization = localization;
        Localization.LanguageChanged += OnLanguageChanged;
    }

    protected ILocalizationService Localization { get; }

    private void OnLanguageChanged(object? sender, EventArgs e) => OnLocalizationChanged();

    protected virtual void OnLocalizationChanged()
    {
    }

    public override void Dispose()
    {
        Localization.LanguageChanged -= OnLanguageChanged;
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
