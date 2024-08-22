using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Core.Services.Contract
{
    public interface IFileService
    {
        string GenerateFileName(string originalFileName);
        Task SaveImageAsync(IFormFile image, string fileName);
    }
}
