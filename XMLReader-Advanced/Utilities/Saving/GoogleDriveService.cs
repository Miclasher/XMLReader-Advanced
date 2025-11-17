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

    private static readonly string[] Scopes = { DriveService.Scope.Drive };
    private static readonly string ApplicationName = "XMLConverterLab";

    public GoogleDriveService(string credentialsPath, string folderId)
    {
        _folderId = folderId;
        InitializeService(credentialsPath);
    }

    public string GetFileIdByName(string fileName, string folderId = null!)
    {
        var request = _service.Files.List();

        var query = $"name = '{fileName}' and trashed = false";

        if (!string.IsNullOrEmpty(folderId))
        {
            query += $" and '{folderId}' in parents";
        }

        request.Q = query;
        request.Fields = "files(id)";
        request.PageSize = 1;

        try
        {
            var result = request.Execute();
            var file = result.Files.FirstOrDefault();
            return file?.Id;
        }
        catch (Exception ex)
        {
            Logger.Instance.Log(Logger.LoggingLevel.Error, $"Помилка пошуку файлу '{fileName}': {ex.Message}");
            return null;
        }
    }

    private void InitializeService(string credentialsPath)
    {
        try
        {
            if (!File.Exists(credentialsPath))
            {
                throw new FileNotFoundException(
                    "Файл credentials.json не знайдено. Помістіть його у папку з  програмою.");
            }

            using var fs = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read);
            var credFolderPath = "token.json";

            var userCredential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(fs).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credFolderPath, true)).Result;

            _service = new(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredential,
                ApplicationName = ApplicationName
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
