using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Minio;
using MinioFileStorage.Api.Configuration;
using MinioFileStorage.Api.Services;

namespace MinioFileStorage.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddS3Storage(this IServiceCollection services, IConfiguration configuration)
    {
        var minioOptions = configuration.GetSection(nameof(MinioStorageOptions)).Get<MinioStorageOptions>()!;

        services
            .AddSingleton(minioOptions)
            .AddSingleton<MinioStorageService>()
            .AddSingleton(new MinioClient()
                .WithEndpoint(minioOptions.ServiceUrl)
                .WithCredentials(minioOptions.AccessKey, minioOptions.SecretAccessKey)
                .Build());
    }
}