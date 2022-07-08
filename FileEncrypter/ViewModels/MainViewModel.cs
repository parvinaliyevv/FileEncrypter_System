namespace FileEncrypter.ViewModels;

public class MainViewModel: INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;


    public RelayCommand GetFilePathCommand { get; set; }

    public RelayCommand StartOperationCommand { get; set; }
    public RelayCommand CancelOperationCommand { get; set; }

    public RelayCommand EncryptSelectedCommand { get; set; }
    public RelayCommand DecryptSelectedCommand { get; set; }


    public CancellationTokenSource Token { get; set; } = new();
    public uint EncryptKey { get; set; } = default;
    
    public bool IsEncryptOperation { get; set; } = true;
    public bool OperationStarted { get; set; } = false;


    private string? _filePathValue;
    public string? FilePathValue
    {
        get { return _filePathValue; }
        set { _filePathValue = value; OnPropertyChanged(); }
    }


    private long _progressBarValue;
    public long ProgressBarValue
    {
        get { return _progressBarValue; }
        set { _progressBarValue = value; OnPropertyChanged(); }
    }

    private long _progressBarMaximumValue;
    public long ProgressBarMaximumValue
    {
        get { return _progressBarMaximumValue; }
        set { _progressBarMaximumValue = value; OnPropertyChanged(); }
    }


    public MainViewModel()
    {
        GetFilePathCommand = new((sender) =>
        {
            FilePathValue = FileDialogService.GetFilePathWithFilter("TXT Files | *.txt");
        }, (sender) => !OperationStarted);

        StartOperationCommand = new((sender) => StartOperation(), (sender) => !OperationStarted && !string.IsNullOrWhiteSpace(FilePathValue) && EncryptKey != default);
        CancelOperationCommand = new((sender) => CancelOperation(), (sender) => OperationStarted && !Token.Token.IsCancellationRequested);

        EncryptSelectedCommand = new((sender) => IsEncryptOperation = true, (sender) => !OperationStarted);
        DecryptSelectedCommand = new((sender) => IsEncryptOperation = false, (sender) => !OperationStarted);

        FilePathValue = string.Empty;

        ProgressBarValue = default;
        ProgressBarMaximumValue = 100;
    }


    public void StartOperation()
    {
        if (!File.Exists(FilePathValue))
        {
            MessageBox.Show("File path wrong.", "File not found!", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            if (IsEncryptOperation) ThreadPool.QueueUserWorkItem((object? state) => EncryptFile());
            else ThreadPool.QueueUserWorkItem((object? state) => DecryptFile());

            OperationStarted = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void CancelOperation() => Token.Cancel();

    private void EncryptFile()
    {
        using var fileStream = new FileStream(FilePathValue, FileMode.Open, FileAccess.ReadWrite);
        ProgressBarMaximumValue = fileStream.Length;

        long position = 0;
        byte bufferLength = 10;
        var buffer = new byte[bufferLength];

        for (; position < ProgressBarMaximumValue; position += bufferLength)
        {
            if (Token.Token.IsCancellationRequested)
            {
                for (int i = 0; i < position; i += bufferLength)
                {
                    fileStream.Position = i;
                    fileStream.Read(buffer, 0, bufferLength);

                    EncryptService.DecryptByteArray(ref buffer, EncryptKey);

                    fileStream.Position = i;
                    fileStream.Write(buffer, 0, bufferLength);

                    ProgressBarValue -= bufferLength;

                    Thread.Sleep(5);
                }

                Token?.Dispose();
                Token = new();

                OperationStarted = false;

                MessageBox.Show("Encrypt operation cancelled.", "Operation successfully cancelled!", MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }

            fileStream.Position = position;
            fileStream.Read(buffer, 0, bufferLength);

            var list = buffer.ToList();
            list.RemoveAll((data) => data == default);

            EncryptService.EncryptByteArray(ref buffer, EncryptKey);
            
            fileStream.Position = position;

            if (position + 10 > ProgressBarMaximumValue)
                fileStream.Write(buffer, 0, (int)(ProgressBarMaximumValue % bufferLength));
            else
                fileStream.Write(buffer, 0, bufferLength);

            ProgressBarValue += bufferLength;

            Thread.Sleep(5);
        }

        OperationStarted = false;

        MessageBox.Show("Encrypt operation completed.", "Operation successfully completed!", MessageBoxButton.OK, MessageBoxImage.Information);

        ProgressBarValue = default;
        ProgressBarMaximumValue = 100;
    }

    private void DecryptFile()
    {
        using var fileStream = new FileStream(FilePathValue, FileMode.Open, FileAccess.ReadWrite);
        ProgressBarMaximumValue = fileStream.Length;

        long position = 0;
        byte bufferLength = 10;
        var buffer = new byte[bufferLength];

        for (; position < ProgressBarMaximumValue; position += bufferLength)
        {
            if (Token.Token.IsCancellationRequested)
            {
                for (int i = 0; i < position; i += bufferLength)
                {
                    fileStream.Position = i;
                    fileStream.Read(buffer, 0, bufferLength);

                    EncryptService.EncryptByteArray(ref buffer, EncryptKey);

                    fileStream.Position = i;
                    fileStream.Write(buffer, 0, bufferLength);

                    ProgressBarValue -= bufferLength;

                    Thread.Sleep(5);
                }

                Token?.Dispose();
                Token = new();

                OperationStarted = false;

                MessageBox.Show("Decrypt operation cancelled.", "Operation successfully cancelled!", MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }

            fileStream.Position = position;
            fileStream.Read(buffer, 0, bufferLength);

            EncryptService.DecryptByteArray(ref buffer, EncryptKey);

            fileStream.Position = position;

            if (position + 10 > ProgressBarMaximumValue)
                fileStream.Write(buffer, 0, (int)(ProgressBarMaximumValue % bufferLength));
            else
                fileStream.Write(buffer, 0, bufferLength);

            ProgressBarValue += bufferLength;

            Thread.Sleep(5);
        }

        OperationStarted = false;

        MessageBox.Show("Decrypt operation completed.", "Operation successfully completed!", MessageBoxButton.OK, MessageBoxImage.Information);

        ProgressBarValue = default;
        ProgressBarMaximumValue = 100;
    }

    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChangedEventHandler? handler = PropertyChanged;
        if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
}
