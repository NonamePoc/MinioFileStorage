using System.Net;
using Microsoft.AspNetCore.Mvc;
using MinioFileStorage.Api.Services;

namespace MinioFileStorage.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class MinioFileController : ControllerBase
{
    private readonly MinioStorageService _storageService;

    public MinioFileController(MinioStorageService storageService)
    {
        _storageService = storageService;
    }

    [HttpGet("bucket-files")]
    public async Task<IActionResult> GetBucketFiles()
    {
        var retval = await _storageService.GetBucketFiles();
        return Ok(retval);
    }

    [HttpGet]
    public async Task GetFile([FromQuery] string path)
    {
        var fileInfo = await _storageService.GetFileInfo(path);

        if (fileInfo is null)
        {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }

        string contentType = GetContentType(fileInfo.ObjectName); 

        Response.Headers.TryAdd("content-disposition",
            $"inline; filename=\"{Path.GetFileName(fileInfo.ObjectName)}\""); 
        Response.ContentType = contentType;
        Response.StatusCode = 200;

        await _storageService.GetFile(path, Response.Body);
    }

    private string GetContentType(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();
        switch (extension)
        {
            case ".pdf":
                return "application/pdf";
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".png":
                return "image/png";
            default:
                return "application/octet-stream"; 
        }
    }



    [HttpPost]
    public async Task<IActionResult> UploadFile([FromQuery] string? path, IFormFile file)
    {
        var filePath = await _storageService.UploadFile(path, file);
        return Ok(new { filePath });
    }
}