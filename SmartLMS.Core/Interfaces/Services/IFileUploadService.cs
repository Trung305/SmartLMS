using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.Interfaces.Services
{
    public interface IFileUploadService
    {
        public Task<string> SaveFileAsync(Stream fileStream, string fileName);
        public bool DeleteFile(string filePath);
    }
}
