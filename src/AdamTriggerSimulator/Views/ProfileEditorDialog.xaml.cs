using System.Windows;
using System.Windows.Input;
using AdamTriggerSimulator.ViewModels;

namespace AdamTriggerSimulator.Views;

/// <summary>
/// Interaction logic for ProfileEditorDialog.xaml
/// </summary>
public partial class ProfileEditorDialog : Window
{
    public ProfileEditorDialogViewModel ViewModel { get; }

    public ProfileEditorDialog()
    {
        InitializeComponent();
        ViewModel = new ProfileEditorDialogViewModel();
        DataContext = ViewModel;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ViewModel.ProfileName))
        {
            MessageBox.Show("Profile name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
            Close();
        }
    }
}
