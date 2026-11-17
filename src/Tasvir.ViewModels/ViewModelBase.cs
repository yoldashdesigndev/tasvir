using Tasvir.Core;

namespace Tasvir.ViewModels;

public abstract class ViewModelBase : ObservableObject, IDisposable
{
    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
