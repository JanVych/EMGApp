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

    private void ListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        ViewModel.PatientClickCommand.Execute(e.ClickedItem);
    }
}
