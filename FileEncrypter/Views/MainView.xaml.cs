namespace FileEncrypter.Views;

public partial class MainView : Window
{
    public MainView()
    {
        InitializeComponent();

        DataContext = new MainViewModel();
    }


    private void Window_OnClosing(object sender, CancelEventArgs e)
    {
        var dataContext = DataContext as MainViewModel;

        if (dataContext.OperationStarted)
        {
            dataContext.Token?.Cancel();
            e.Cancel = true;
        }
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        var control = sender as PasswordBox;
        var dataContext = DataContext as MainViewModel;

        uint password;

        try
        {
            password = Convert.ToUInt32(control.Password);

            dataContext.EncryptKey = password;
            control.BorderBrush = Brushes.Gray;
        }
        catch
        {
            dataContext.EncryptKey = default;
            control.BorderBrush = Brushes.Red;
        }
    }
}
