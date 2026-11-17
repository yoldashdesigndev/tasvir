using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Tasvir.ViewModels;

namespace Tasvir.App.Views;

public partial class MirrorView : UserControl
{
    private MirrorViewModel? _model;

    public MirrorView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += (_, _) => SyncHost();
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_model is not null)
        {
            _model.PropertyChanged -= OnModelPropertyChanged;
        }

        _model = e.NewValue as MirrorViewModel;
        if (_model is not null)
        {
            _model.PropertyChanged += OnModelPropertyChanged;
        }

        SyncHost();
    }

    private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MirrorViewModel.WindowHandle))
        {
            SyncHost();
        }
    }

    private void SyncHost()
    {
        var handle = _model?.WindowHandle ?? IntPtr.Zero;
        if (handle == IntPtr.Zero)
        {
            PreviewHost.Detach();
        }
        else
        {
            PreviewHost.Attach(handle);
        }
    }
}
