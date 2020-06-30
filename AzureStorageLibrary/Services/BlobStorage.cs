using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureStorageLibrary.Services
{
    public class BlobStorage : IBlobStorage
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobStorage()
        {
            _blobServiceClient = new BlobServiceClient(ConnectionStrings.AzureStorageConnectionString);
        }

        public string BlobUrl => "https://udemyrealstorageaccount.blob.core.windows.net";

        public async Task DeleteAsync(string fileName, EContainerName eContainerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(eContainerName.ToString());

            var blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.DeleteAsync();
        }

        public async Task<Stream> DownloadAsync(string fileName, EContainerName eContainerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(eContainerName.ToString());

            var blobClient = containerClient.GetBlobClient(fileName);

            var info = await blobClient.DownloadAsync();

            return info.Value.Content;
        }

        public async Task<List<string>> GetLogAsync(string fileName)
        {
            List<string> logs = new List<string>();
            var containerClient = _blobServiceClient.GetBlobContainerClient(EContainerName.Logs.ToString());

            var appendBlobClient = containerClient.GetAppendBlobClient(fileName);

            await appendBlobClient.CreateIfNotExistsAsync();

            var info = await appendBlobClient.DownloadAsync();

            using (StreamReader sr = new StreamReader(info.Value.Content))
            {
                string line = string.Empty;

                while ((line = sr.ReadLine()) != null)
                {
                    logs.Add(line);
                }
            }
            return logs;
        }

        public List<string> GetNames(EContainerName eContainerName)
        {
            List<string> blobNames = new List<string>();

            var containerClient = _blobServiceClient.GetBlobContainerClient(eContainerName.ToString());

            var blobs = containerClient.GetBlobs();

            blobs.ToList().ForEach(x =>
            {
                blobNames.Add(x.Name);
            });

            return blobNames;
        }

        public async Task SetLogAsync(string text, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(EContainerName.Logs.ToString());

            var appendBlobClient = containerClient.GetAppendBlobClient(fileName);

            await appendBlobClient.CreateIfNotExistsAsync();

            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms))
                {
                    sw.Write($"{DateTime.Now}: {text} /n");

                    sw.Flush();
                    ms.Position = 0;

                    await appendBlobClient.AppendBlockAsync(ms);
                }
            }
        }

        public async Task UploadAsync(Stream fileStream, string fileName, EContainerName eContainerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(eContainerName.ToString());

            await containerClient.CreateIfNotExistsAsync();

            await containerClient.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer);

            var blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.UploadAsync(fileStream);
        }
    }
}