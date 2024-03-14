using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using MinioFileStorage.Api.Configuration;

namespace MinioFileStorage.Api.Services;

public class MinioStorageService
{
    private readonly IMinioClient _client;
    private readonly MinioStorageOptions _options;

    public MinioStorageService(IMinioClient client, MinioStorageOptions options)
    {
        _client = client;
        _options = options;
    }

    public async Task<string> UploadFile(string? path, IFormFile file)
    {
        var filePath = path is null
            ? file.FileName
            : Path.Combine(path, file.FileName);

        await using var fs = file.OpenReadStream();

        var request = new PutObjectArgs()
            .WithBucket(_options.BucketName)
            .WithStreamData(fs)
            .WithContentType(file.ContentType)
            .WithObject(filePath)
            .WithObjectSize(file.Length)
            .WithContentType(file.ContentType);

        await _client.PutObjectAsync(request);

        return filePath;
    }

    public async Task GetFile(string path, Stream responseStream)
    {
        try
        {
            using var memoryStream = new MemoryStream();
            var localMemoryStream = memoryStream;

            await _client.GetObjectAsync(
                new GetObjectArgs()
                    .WithBucket(_options.BucketName)
                    .WithObject(path)
                    .WithCallbackStream(stream => stream.CopyTo(localMemoryStream))
            );

            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(responseStream);
        }
        catch (Exception ex)
        {
            // To do: log exception
        }
        finally
        {
            responseStream.Close();
        }
    }


    public async Task<ObjectStat?> GetFileInfo(string path)
    {
        try
        {
            return await _client.StatObjectAsync(
                new StatObjectArgs()
                    .WithBucket(_options.BucketName)
                    .WithObject(path)
            );
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<IList<Item>> GetBucketFiles()
    {
        var args = new ListObjectsArgs()
            .WithBucket(_options.BucketName)
            .WithRecursive(true);

        var retval = await _client.ListObjectsAsync(args).ToList().ToTask();
        return retval;
    }
}