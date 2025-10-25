using System.Windows;
using System.Windows.Input;

namespace AdamTriggerSimulator.Views;

/// <summary>
/// Interaction logic for WelcomeDialog.xaml
/// </summary>
public partial class WelcomeDialog : Window
{
    public WelcomeDialog()
    {
        InitializeComponent();
    }

    private void CreateProfileButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
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
