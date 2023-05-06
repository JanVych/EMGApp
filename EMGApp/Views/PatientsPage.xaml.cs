using EMGApp.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace EMGApp.Views;

public sealed partial class PatientsPage : Page
{
    public PatientsViewModel ViewModel
    {
        get;
    }

    public PatientsPage()
    {
        ViewModel = App.GetService<PatientsViewModel>();
        InitializeComponent();
    }
}
