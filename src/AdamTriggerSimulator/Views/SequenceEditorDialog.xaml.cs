using System.Windows;
using System.Windows.Input;
using AdamTriggerSimulator.ViewModels;

namespace AdamTriggerSimulator.Views;

/// <summary>
/// Interaction logic for SequenceEditorDialog.xaml
/// </summary>
public partial class SequenceEditorDialog : Window
{
    public SequenceEditorDialogViewModel ViewModel { get; }

    public SequenceEditorDialog()
    {
        InitializeComponent();
        ViewModel = new SequenceEditorDialogViewModel();
        DataContext = ViewModel;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Execute the save command
        ViewModel.SaveCommand.Execute(null);

        // Check if validation passed
        if (ViewModel.DialogResult)
        {
            DialogResult = true;
            Close();
        }
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
