using System;
using System.Collections.Generic;
using System.IO;
using AzureStorage.Utilities;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureStorage.DataAccess
{
    internal class BlobContainer : IBlobContainer
    {
        private readonly CloudBlobContainer _container;

        public BlobContainer(CloudBlobContainer container)
        {
            _container = container;
        }

        #region Implementation of IBlobContainer

        public string GetSharedAccessSignature(int validSeconds)
        {
            var policy = new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Create | SharedAccessBlobPermissions.Write,
                SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddSeconds(validSeconds)
            };
            return _container.Uri + _container.GetSharedAccessSignature(policy);
        }

        public void UploadText(string blobName, string content)
        {
            var blockBlobRef = _container.GetBlockBlobReference(blobName);
            blockBlobRef.UploadTextAsync(content).Wait();
        }

        public IEnumerable<string> ListBlobs(string relativeAddress)
        {
            return new RowBuffer<string>(() => DoListBlobs(relativeAddress));
        }

        public void DeleteBlob(string blobName)
        {
            var blockBlobRef = _container.GetBlockBlobReference(blobName);
            blockBlobRef.DeleteAsync().Wait();
        }

        public void Upload(string blobName, Stream data)
        {
            var blockBlobRef = _container.GetBlockBlobReference(blobName);
            blockBlobRef.UploadFromStreamAsync(data).Wait();
        }

        public Stream OpenStream(string blobName)
        {
            var blockBlobRef = _container.GetBlockBlobReference(blobName);
            return blockBlobRef.OpenReadAsync().Result;
        }

        private IEnumerable<string> DoListBlobs(string relativeAddress)
        {
            var dir = _container.GetDirectoryReference(relativeAddress);
            
            BlobContinuationToken token = null;
            do
            {
                var task = dir.ListBlobsSegmentedAsync(true, BlobListingDetails.All, null, null, null, null);
                var segment = task.Result;
                token = segment.ContinuationToken;
                foreach (var entity in segment.Results)
                {
                    yield return entity.Uri.AbsolutePath;
                }
            } while (token != null);

        }
        #endregion
    }
}