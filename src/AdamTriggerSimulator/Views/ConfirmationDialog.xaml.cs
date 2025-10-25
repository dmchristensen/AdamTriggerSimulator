using System.Windows;
using System.Windows.Input;

namespace AdamTriggerSimulator.Views;

/// <summary>
/// Interaction logic for ConfirmationDialog.xaml
/// </summary>
public partial class ConfirmationDialog : Window
{
    public ConfirmationDialog(string title, string message)
    {
        InitializeComponent();
        TitleText.Text = title;
        MessageText.Text = message;
    }

    private void YesButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void NoButton_Click(object sender, RoutedEventArgs e)
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

    /// <summary>
    /// Shows a confirmation dialog with custom title and message.
    /// </summary>
    public static bool Show(string title, string message, Window owner)
    {
        var dialog = new ConfirmationDialog(title, message)
        {
            Owner = owner
        };
        return dialog.ShowDialog() == true;
    }
}
