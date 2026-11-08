using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using XmlProcessorApp.Logging;

namespace XmlProcessorApp.Services
{
    public class GoogleDriveManager
    {
        private const string DefaultDataStoreFolderName = ".authdata";
        private const string ClientSecretsFileName = "client_secrets.json";

        private DriveService? _driveService;

        public bool IsInitialized => _driveService != null;

        public async Task InitClientAsync()
        {
            if (_driveService != null) return;

            string secretsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ClientSecretsFileName);
            if (!File.Exists(secretsPath))
            {
                var fallback = Path.Combine(Directory.GetCurrentDirectory(), ClientSecretsFileName);
                if (File.Exists(fallback)) secretsPath = fallback;
            }

            if (!File.Exists(secretsPath))
            {
                throw new FileNotFoundException(
                    $"Файл ключів авторизації не знайдено: {secretsPath}. Додайте client_secrets.json з Google Cloud Console.");
            }

            var clientSecrets = (await GoogleClientSecrets.FromFileAsync(secretsPath)).Secrets;
            var dataStore = new FileDataStore(DefaultDataStoreFolderName);
            var scopes = new[] { DriveService.Scope.DriveFile };

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets,
                scopes,
                "user",
                CancellationToken.None,
                dataStore);

            _driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "FacultyXmlProcessorApp"
            });
        }

        public async Task<List<Google.Apis.Drive.v3.Data.File>> ListXmlFilesAsync()
        {
            if (_driveService == null) await InitClientAsync();

            var listRequest = _driveService!.Files.List();
            listRequest.Spaces = "drive";
            listRequest.Fields = "files(id, name, mimeType, size, modifiedTime)";
            // Шукаємо xml, xsl, html файли
            listRequest.Q = "trashed = false and (name contains '.xml' or name contains '.xsl' or name contains '.html')";

            var response = await listRequest.ExecuteAsync();
            return response.Files?.ToList() ?? new List<Google.Apis.Drive.v3.Data.File>();
        }

        public async Task<string> UploadFileAsync(string fileName, Stream contentStream, string mimeType)
        {
            if (_driveService == null) await InitClientAsync();

            if (contentStream.CanSeek) contentStream.Position = 0;

            // Перевіряємо чи файл з таким ім'ям уже існує
            var listRequest = _driveService!.Files.List();
            listRequest.Spaces = "drive";
            listRequest.Fields = "files(id, name)";
            listRequest.Q = $"name = '{fileName.Replace("'", "''")}' and trashed = false";
            var listResponse = await listRequest.ExecuteAsync();

            var existingFile = listResponse.Files?.FirstOrDefault();
            if (existingFile != null)
            {
                // Оновлюємо існуючий файл
                var updateRequest = _driveService.Files.Update(new Google.Apis.Drive.v3.Data.File(), existingFile.Id, contentStream, mimeType);
                await updateRequest.UploadAsync();
                return existingFile.Id;
            }
            else
            {
                // Створюємо новий файл
                var fileMetadata = new Google.Apis.Drive.v3.Data.File
                {
                    Name = fileName
                };

                var createRequest = _driveService.Files.Create(fileMetadata, contentStream, mimeType);
                createRequest.Fields = "id";
                var uploadProgress = await createRequest.UploadAsync();

                if (uploadProgress.Exception != null)
                {
                    throw uploadProgress.Exception;
                }

                return createRequest.ResponseBody?.Id ?? string.Empty;
            }
        }

        public async Task DownloadFileAsync(string fileId, Stream destinationStream)
        {
            if (_driveService == null) await InitClientAsync();

            var request = _driveService!.Files.Get(fileId);
            await request.DownloadAsync(destinationStream);
            if (destinationStream.CanSeek) destinationStream.Position = 0;
        }

        public async Task<Stream?> DownloadFileByNameAsync(string fileName)
        {
            if (_driveService == null) await InitClientAsync();

            var files = await ListXmlFilesAsync();
            var target = files.FirstOrDefault(f => f.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            if (target == null) return null;

            var ms = new MemoryStream();
            await DownloadFileAsync(target.Id, ms);
            ms.Position = 0;
            return ms;
        }
    }
}
