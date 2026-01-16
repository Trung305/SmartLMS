using SmartLMS.Core.Interfaces.Services;

namespace SmartLMS.Web.Services
{
    public class WebFileStoragePathProvider : IFileStoragePathProvider
    {
        private readonly IWebHostEnvironment _env;

        public WebFileStoragePathProvider(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string GetRootPath()
        {
            return Path.Combine(_env.WebRootPath, "uploads");
        }
    }
}
