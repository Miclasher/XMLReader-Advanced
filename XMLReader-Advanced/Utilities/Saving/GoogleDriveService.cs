using System.Text;
using Google.Apis.Drive.v3;

namespace XMLReader_Advanced.Utilities.Saving;

public sealed class GoogleDriveService
{
    private readonly DriveService _service;
    private readonly string _folderId;

    public GoogleDriveService()
    {
        throw new NotImplementedException("Authentification not implemented yet.");
    }


    public string DownloadFile(string fileId)
    {
        Logger.Instance.Log(Logger.LoggingLevel.Saving, $"Завантаження файлу {fileId} з Google Drive.");
        var request = _service.Files.Get(fileId);
        var stream = new MemoryStream();
        request.Download(stream);
        stream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public void SaveFile(string content, string fileName, string mimeType)
    {
        Logger.Instance.Log(Logger.LoggingLevel.Saving, $"Збереження файлу {fileName} на Google Drive.");
        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = fileName
        };
        if (!string.IsNullOrEmpty(_folderId))
        {
            fileMetadata.Parents = [_folderId];
        }

        var byteArray = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(byteArray);

        var request = _service.Files.Create(fileMetadata, stream, mimeType);
        request.Upload();
    }
}
