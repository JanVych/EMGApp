using EMGApp.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace EMGApp.Views;

public sealed partial class EditPage : Page
{
    public EditViewModel ViewModel
    {
        get;
    }

    public EditPage()
    {
        ViewModel = App.GetService<EditViewModel>();
        InitializeComponent();
    }
}
