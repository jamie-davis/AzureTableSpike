using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AzureStorage
{
    public interface IBlobContext
    {
        IBlobContainer GetContainer(string containerName);

    }

    public interface IBlobContainer
    {
        string GetSharedAccessSignature(int validSeconds);
        void UploadText(string blobName, string content);
        IEnumerable<string> ListBlobs(string relativeAddress);
        void DeleteBlob(string blobName);
        void Upload(string blobName, Stream data);
        Stream OpenStream(string blobName);
    }
}
