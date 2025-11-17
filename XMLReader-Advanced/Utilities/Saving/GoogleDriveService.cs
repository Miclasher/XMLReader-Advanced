using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace XMLReader_Advanced.Utilities.Saving;

public sealed class GoogleDriveService
{
    private DriveService _service;
    private readonly string _folderId;

    private static readonly string[] _scopes = { DriveService.Scope.DriveFile };
    private static readonly string _applicationName = "XMLConverterLab";

    public GoogleDriveService(string credentialsPath, string folderId)
    {
        _folderId = folderId;
        InitializeService(credentialsPath);
    }

    private void InitializeService(string credentialsPath)
    {
        try
        {
            UserCredential userCredential;

            if (!File.Exists(credentialsPath))
            {
                throw new FileNotFoundException(
                    "Файл credentials.json не знайдено. Помістіть його у папку з  програмою.");
            }

            using var fs = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read);
            var credFolderPath = "token.json";

            userCredential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(fs).Secrets,
                _scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credFolderPath, true)).Result;

            _service = new(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredential,
                ApplicationName = _applicationName
            });
        }
        catch (Exception e)
        {
            Logger.Instance.Log(Logger.LoggingLevel.Error, e.Message);
        }
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
