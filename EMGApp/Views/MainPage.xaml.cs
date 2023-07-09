using System.Diagnostics;
using EMGApp.Models;
using EMGApp.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace EMGApp.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
        
    }

    // :(
    private void ToggleButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ViewModel._measurementService.IsRecording == false)
        {
            var t1 = ((TextBlock)((Grid)((ToggleButton)sender).Parent).FindName("muscleTypeString")).Text;
            var t2 = ((TextBlock)((Grid)((ToggleButton)sender).Parent).FindName("sideString")).Text;
            var n1 = MeasurementData.MuscleTypeStrings.FirstOrDefault(x => x.Value == t1).Key;
            var n2 = MeasurementData.SideStrings.FirstOrDefault(x => x.Value == t2).Key;
            ViewModel.MuscleButton(n1, n2);
            itemsRepeater.UpdateLayout();
        }
        else
        {
            ((ToggleButton)sender).IsChecked = false;
        }
    }
    // :{
    private void Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        mFlyout.Hide();
    }
}
