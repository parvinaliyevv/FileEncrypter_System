namespace FileEncrypter.Services;

public static class FileDialogService
{
    private static VistaOpenFileDialog? _fileDialog { get; set; }


    public static string? GetFilePath()
    {
        _fileDialog = new();

        return (_fileDialog.ShowDialog() is true) ? _fileDialog.FileName : null;
    }

    public static string? GetFilePathWithFilter(string filter)
    {
        _fileDialog = new();
        _fileDialog.Filter = filter;

        return (_fileDialog.ShowDialog() is true) ? _fileDialog.FileName : null;
    }
}
