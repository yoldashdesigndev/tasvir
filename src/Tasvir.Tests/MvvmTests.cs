using Tasvir.Core;
using Xunit;

namespace Tasvir.Tests;

[Collection("Serial")]
public sealed class MvvmTests
{
    [Fact]
    public void SetProperty_RaisesOnceAndIgnoresEqualValue()
    {
        var target = new TestObservable();
        var raises = 0;
        target.PropertyChanged += (_, _) => raises++;

        target.Value = "first";
        target.Value = "first";

        Assert.Equal(1, raises);
        Assert.Equal("first", target.Value);
    }

    [Fact]
    public void RelayCommand_HonoursCanExecuteAndRaisesChanged()
    {
        var allowed = false;
        var executed = 0;
        var command = new RelayCommand(() => executed++, () => allowed);
        var raises = 0;
        command.CanExecuteChanged += (_, _) => raises++;

        Assert.False(command.CanExecute(null));
        command.Execute(null);
        Assert.Equal(0, executed);

        allowed = true;
        command.RaiseCanExecuteChanged();
        Assert.True(command.CanExecute(null));
        command.Execute(null);
        Assert.Equal(1, executed);
        Assert.Equal(1, raises);
    }

    [Fact]
    public void RelayCommand_RejectsNullExecute()
    {
        Assert.Throws<ArgumentNullException>(() => new RelayCommand(null!));
    }

    private sealed class TestObservable : ObservableObject
    {
        private string _value = string.Empty;

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
    }
}
