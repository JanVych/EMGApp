using EMGApp.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace EMGApp.Views;

public sealed partial class AddPage : Page
{
    public AddViewModel ViewModel { get; }
    public AddPage()
    {
        ViewModel = App.GetService<AddViewModel>();
        InitializeComponent();
    }
}
