using Microsoft.AspNetCore.Http;
using SarhneApp.Core.Services.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Service
{
    public class FileService : IFileService
    {
        public string GenerateFileName(string originalFileName)
        {
            return Path.GetFileNameWithoutExtension(originalFileName) + Guid.NewGuid().ToString() + Path.GetExtension(originalFileName);
        }

        public async Task SaveImageAsync(IFormFile image, string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
        }
    }
}
