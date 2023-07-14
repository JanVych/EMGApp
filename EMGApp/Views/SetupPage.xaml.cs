using EMGApp.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace EMGApp.Views;

public sealed partial class SetupPage : Page
{
    public SetupViewModel ViewModel
    {
        get;
    }

    public SetupPage()
    {
        ViewModel = App.GetService<SetupViewModel>();
        InitializeComponent();
    }
}
