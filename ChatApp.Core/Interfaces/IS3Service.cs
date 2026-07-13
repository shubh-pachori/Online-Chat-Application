using System;
using System.IO;
using System.Threading.Tasks;

namespace ChatApp.Core.Interfaces
{
    public interface IS3Service
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        string GetPresignedUrl(string fileKey, TimeSpan expiry);
        Task DeleteFileAsync(string fileKey);
    }
}
