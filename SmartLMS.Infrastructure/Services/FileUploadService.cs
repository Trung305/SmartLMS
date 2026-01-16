using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SmartLMS.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Infrastructure.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IFileStoragePathProvider _pathProvider;

        public FileUploadService(IFileStoragePathProvider pathProvider)
        {
            _pathProvider = pathProvider;
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName)
        {
            var uploadFolder = _pathProvider.GetRootPath() + "\\courses";

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }
            var filePath = Path.Combine(uploadFolder, fileName);

            using (var file = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(file);
            }
            filePath = Path.Combine("/uploads/courses/", fileName);
            return filePath;
        }

        public bool DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }
    }
}
