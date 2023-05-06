using EMGApp.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace EMGApp.Views;

public sealed partial class MeasurementsPage : Page
{
    public MeasurementsViewModel ViewModel
    {
        get;
    }

    public MeasurementsPage()
    {
        ViewModel = App.GetService<MeasurementsViewModel>();
        InitializeComponent();
    }
}
