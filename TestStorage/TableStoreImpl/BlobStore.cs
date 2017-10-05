using System;
using System.Collections.Generic;
using System.IO;
using AzureStorage;

namespace TestStorage.TableStoreImpl
{
    public class BlobStore
    {
        private Dictionary<string, BlobContainer> _containers = new Dictionary<string, BlobContainer>();

        public IBlobContainer GetContainer(string containerName)
        {
            if (_containers.TryGetValue(containerName, out BlobContainer container))
                return container;

            container = new BlobContainer(containerName);
            _containers[containerName] = container;
            return container;
        }
    }

    internal class BlobContainer : IBlobContainer
    {
        public string ContainerName { get; }
        public Dictionary<string, byte[]> _blobs = new Dictionary<string, byte[]>();

        public BlobContainer(string containerName)
        {
            ContainerName = containerName;
        }

        #region Implementation of IBlobContainer

        public string GetSharedAccessSignature(int validSeconds)
        {
            return $"Fake Blob Container/{ContainerName}/ValidFor/{validSeconds}/seconds";
        }

        public void UploadText(string blobName, string content)
        {
            byte[] bytes;
            using (var ms = new MemoryStream())
            using (var tw = new StreamWriter(ms))
            {
                tw.Write(content);
                tw.Flush();
                ms.Flush();
                bytes = ms.ToArray();
            }

            StoreBlob(blobName, bytes);
        }

        private void StoreBlob(string blobName, byte[] bytes)
        {
            _blobs[blobName] = bytes;
        } 

        public IEnumerable<string> ListBlobs(string relativeAddress)
        {
            return _blobs.Keys;
        }

        public void DeleteBlob(string blobName)
        {
            _blobs.Remove(blobName);
        }

        public void Upload(string blobName, Stream data)
        {
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                data.CopyTo(ms);
                ms.Flush();
                bytes = ms.ToArray();
            }

            StoreBlob(blobName, bytes);
        }

        public Stream OpenStream(string blobName)
        {
            if (!_blobs.TryGetValue(blobName, out byte[] data))
                throw new Exception("Blob not found.");
            return new MemoryStream(data);
        }

        #endregion
    }
}