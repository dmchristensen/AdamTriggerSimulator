using System.Windows;
using System.Windows.Input;
using AdamTriggerSimulator.ViewModels;
using AdamTriggerSimulator.Services;
using AdamTriggerSimulator.Models;

namespace AdamTriggerSimulator.Views;

/// <summary>
/// Interaction logic for ManualCommandDialog.xaml
/// </summary>
public partial class ManualCommandDialog : Window
{
    public ManualCommandDialogViewModel ViewModel { get; }

    public ManualCommandDialog(IAdamCommunicationService communicationService, DeviceProfile profile)
    {
        InitializeComponent();
        ViewModel = new ManualCommandDialogViewModel(communicationService);
        ViewModel.SetProfile(profile);
        DataContext = ViewModel;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }
}
