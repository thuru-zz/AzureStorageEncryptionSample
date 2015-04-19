using AzureEncryptionExtensions;
using AzureEncryptionExtensions.Providers;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebApplication.Models;

namespace WebApplication
{
    public class AttachmentService
    {
        public async Task<int> UploadInstanceAttachmentAsync(MultipartMemoryStreamProvider provider)
        {
            foreach (var file in provider.Contents)
            {
                var fileName = file.Headers.ContentDisposition.FileName.Replace("\"", "");

                var stream = await file.ReadAsStreamAsync();

                var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["InputAttachmentStorage"].ConnectionString);
                var blobClient = storageAccount.CreateCloudBlobClient();
                var inputContainer = blobClient.GetContainerReference("uploadcontainer");

                bool okToProceed = await inputContainer.ExistsAsync();

                if (!okToProceed)
                {
                    okToProceed = await inputContainer.CreateIfNotExistsAsync();
                }

                if (okToProceed)
                {
                    var path = "uploads" + "/" + fileName;
                    var block = inputContainer.GetBlockBlobReference(path);

                    // TODO :: create async encrypted upload :: Fork - https://github.com/thuru/azure-encryption-extensions
                    var encryptionProvider = new SymmetricBlobCryptoProvider(GetEncryptionKey(new object()));
                    block.UploadFromStreamEncrypted(encryptionProvider, stream);
                }
                else
                {
                    throw new ApplicationException("Unable to create / access container in Azure Storage");
                }
            }

            return 0;
        }

        public async Task<AttachmentDto> DownloadAttachment(int attachmentId)
        {
            var attachement = await _dbContext.<YourEntity>.SingleOrDefaultAsync(a => a.Id == attachmentId);
            if (attachement != null)
            {
                var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["InputAttachmentStorage"].ConnectionString);
                var blobClient = storageAccount.CreateCloudBlobClient();
                var inputContainer = blobClient.GetContainerReference("cpinputattachments");

                var block = inputContainer.GetBlobReferenceFromServer(attachement.Path);
                var memoryStream = new MemoryStream();

                // TODO :: create async encrypted upload :: Fork - https://github.com/thuru/azure-encryption-extensions
                var encryptionProvider = new SymmetricBlobCryptoProvider(GetEncryptionKey(new object()));
                block.DownloadToStreamEncrypted(encryptionProvider, memoryStream);

                memoryStream.Position = 0;

                return new AttachmentDto()
                {
                    FileStream = memoryStream,
                    FileName = attachement.FileName
                };
            }
            else
            {
                throw new ApplicationException("File does not exist");
            }
        }

        private byte[] GetEncryptionKey(object param)
        {
            string key = String.Empty; // Retrieve the key based on the parameter passed
            var keyArray = new byte[key.Length];

            Buffer.BlockCopy(key.ToCharArray(), 0, keyArray, 0, keyArray.Length);
            return keyArray;
        }
    }
}